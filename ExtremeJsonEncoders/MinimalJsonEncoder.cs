﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ExtremeJsonEncoders
{
	/// <summary>
	/// Escape only the minimal/what the RFC require: https://datatracker.ietf.org/doc/html/rfc8259#section-7
	/// 
	/// UnsafeRelaxedJsonEscaping escapes too much #86463 
	/// https://github.com/dotnet/runtime/issues/86463
	/// </summary>
	public class MinimalJsonEncoder : JavaScriptEncoder, IMustEscapeChar
	{
		public override int MaxOutputCharactersPerInputCharacter => 6;

		private readonly AsciiPreescapedData _asciiPreescapedData;
		private readonly ScalarEscaperBase _scalarEscaper;

		public static readonly MinimalJsonEncoder Shared = new();

		private readonly bool _lowerCaseHex;
		private readonly bool[] _mustEscapeAscii;

		public MinimalJsonEncoder(bool shortEscapes = true, bool lowerCaseHex = false, char[]? extraAsciiEscapeChars = default) 
		{
			if (extraAsciiEscapeChars != null)
				for (int i = 0; i < extraAsciiEscapeChars.Length; i++)
					if (extraAsciiEscapeChars[i] > 127)
						throw new ArgumentException($"Not ascii: {extraAsciiEscapeChars[i]} (0x{(int)extraAsciiEscapeChars[i]:X})");

			_lowerCaseHex = lowerCaseHex;
			_mustEscapeAscii = CreateEscapeMap(extraAsciiEscapeChars);

#if NET8_0_OR_GREATER
			_sv_allowed_ascii = new Lazy<SearchValues<char>>(GetAllowedAsciiSv);
			_sv_allowed_ascii_u8 = new Lazy<SearchValues<byte>>(GetAllowedAsciiSvU8);
#endif

			_scalarEscaper = shortEscapes ? EscaperImplementation.SingletonPreescape : EscaperImplementation.SingletonNoPreescape;
			_asciiPreescapedData.PopulatePreescapedData(this, _scalarEscaper, lowerCaseHex);
		}



		public override OperationStatus Encode(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock = true)
		{
			int srcIdx = 0;
			int dstIdx = 0;

			while (true)
			{
				if (!SpanUtility.IsValidIndex(source, srcIdx))
				{
					break; // EOF
				}

				char thisChar = source[srcIdx];
				if (!_asciiPreescapedData.TryGetPreescapedData(thisChar, out ulong preescapedEntry))
				{
					goto NotAscii; // forward jump predicted not taken
				}

				if (!SpanUtility.IsValidIndex(destination, dstIdx))
				{
					goto DestTooSmall; // forward jump predicted not taken
				}

				destination[dstIdx] = (char)(byte)preescapedEntry;
				if (((uint)preescapedEntry & 0xFF00) == 0)
				{
					dstIdx++; // predicted taken - only had to write a single char
					srcIdx++;
					continue;
				}

				// At this point, we're writing a multi-char output for a single-char input.
				// Copy over as many chars as we can.

				preescapedEntry >>= 8;
				int dstIdxTemp = dstIdx + 1;
				do
				{
					if (!SpanUtility.IsValidIndex(destination, dstIdxTemp))
					{
						goto DestTooSmall; // forward jump predicted not taken
					}

					destination[dstIdxTemp++] = (char)(byte)preescapedEntry;
				} while ((byte)(preescapedEntry >>= 8) != 0);

				dstIdx = dstIdxTemp;
				srcIdx++;
				continue;

			NotAscii:

				// don't escape anything outside of ascii?
				//destination[dstIdx] = thisChar;
				//dstIdx++;
				//srcIdx++;


				if (!Rune.TryCreate(thisChar, out Rune scalarValue))
				{
					int srcIdxTemp = srcIdx + 1;
					if (SpanUtility.IsValidIndex(source, srcIdxTemp))
					{
						if (Rune.TryCreate(thisChar, source[srcIdxTemp], out scalarValue))
						{
							goto CheckWhetherScalarValueAllowed; // successfully extracted scalar value
						}
					}
					else if (!isFinalBlock && char.IsHighSurrogate(thisChar))
					{
						goto NeedMoreData; // ended with a high surrogate, and caller said they'd provide more data
					}

					scalarValue = Rune.ReplacementChar; // fallback char
//					goto MustEncodeNonAscii;
				}

			CheckWhetherScalarValueAllowed:

				// All possible escape chars (all in _asciiPreescapedData) already handled at this point, no need to check IsScalarValueAllowed
				//if (IsScalarValueAllowed(scalarValue))
				{
					if (!scalarValue.TryEncodeToUtf16(destination.Slice(dstIdx), out int utf16CodeUnitCount))
					{
						goto DestTooSmall;
					}

					dstIdx += utf16CodeUnitCount;
					srcIdx += utf16CodeUnitCount;
					continue;
				}

			//MustEncodeNonAscii:

			//	// At this point, we know we need to encode.

			//	int charsWrittenJustNow = _scalarEscaper.EncodeUtf16(scalarValue, destination.Slice(dstIdx));
			//	if (charsWrittenJustNow < 0)
			//	{
			//		goto DestTooSmall;
			//	}

			//	dstIdx += charsWrittenJustNow;
			//	srcIdx += scalarValue.Utf16SequenceLength;
			}

			// And at this point, we're done!

			OperationStatus retVal = OperationStatus.Done;

		CommonReturn:
			charsConsumed = srcIdx;
			charsWritten = dstIdx;
			return retVal;

		DestTooSmall:
			retVal = OperationStatus.DestinationTooSmall;
			goto CommonReturn;

		NeedMoreData:
			retVal = OperationStatus.NeedMoreData;
			goto CommonReturn;
		}

		public override OperationStatus EncodeUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
		{
			_AssertThisNotNull(); // hoist "this != null" check out of hot loop below

			int srcIdx = 0;
			int dstIdx = 0;

			while (true)
			{
				if (!SpanUtility.IsValidIndex(source, srcIdx))
				{
					break; // EOF
				}

				//was uint
				byte thisByte = source[srcIdx];
				if (!_asciiPreescapedData.TryGetPreescapedData(thisByte, out ulong preescapedEntry))
				{
					goto NotAscii; // forward jump predicted not taken
				}

				// The common case is that the destination is large enough to hold 8 bytes of output,
				// so let's write the entire pre-escaped entry to it. In reality we're only writing up
				// to 6 bytes of output, so we'll only bump dstIdx by the number of useful bytes we
				// wrote.

				if (SpanUtility.TryWriteUInt64LittleEndian(destination, dstIdx, preescapedEntry))
				{
					dstIdx += (int)(preescapedEntry >> 56); // predicted taken
					srcIdx++;
					continue;
				}

				// We don't have enough space to hold a single QWORD copy, so let's write byte-by-byte
				// and see if we have enough room.

				int dstIdxTemp = dstIdx;
				do
				{
					if (!SpanUtility.IsValidIndex(destination, dstIdxTemp))
					{
						goto DestTooSmall; // forward jump predicted not taken
					}

					destination[dstIdxTemp++] = (byte)preescapedEntry;
				} while ((byte)(preescapedEntry >>= 8) != 0);

				dstIdx = dstIdxTemp;
				srcIdx++;
				continue;

			NotAscii:

				// don't escape anything outside of ascii?
				//destination[dstIdx] = thisByte;
				//dstIdx++;
				//srcIdx++;

				OperationStatus runeDecodeStatus = Rune.DecodeFromUtf8(source.Slice(srcIdx), out Rune scalarValue, out int bytesConsumedJustNow);
				if (runeDecodeStatus != OperationStatus.Done)
				{
					if (!isFinalBlock && runeDecodeStatus == OperationStatus.NeedMoreData)
					{
						goto NeedMoreData; // source ends in the middle of a multi-byte sequence
					}

					Debug.Assert(scalarValue == Rune.ReplacementChar); // DecodeFromUtfXX should've set replacement character on failure
					scalarValue = Rune.ReplacementChar;
					// goto MustEncodeNonAscii; // bad UTF-8 data seen
				}

				// All possible escape chars (all in _asciiPreescapedData) already handled at this point, no need to check IsScalarValueAllowed
				//if (IsScalarValueAllowed(scalarValue))
				{
					if (!scalarValue.TryEncodeToUtf8(destination.Slice(dstIdx), out int utf8CodeUnitCount))
					{
						goto DestTooSmall;
					}
					dstIdx += utf8CodeUnitCount;
					srcIdx += utf8CodeUnitCount;
					continue;
				}

				//MustEncodeNonAscii:

				//	// At this point, we know we need to encode.

				//	int bytesWrittenJustNow = _scalarEscaper.EncodeUtf8(scalarValue, destination.Slice(dstIdx));
				//	if (bytesWrittenJustNow < 0)
				//	{
				//		goto DestTooSmall;
				//	}

				//	dstIdx += bytesWrittenJustNow;
				//	srcIdx += bytesConsumedJustNow;
			}

			// And at this point, we're done!

			OperationStatus retVal = OperationStatus.Done;

		CommonReturn:
			bytesConsumed = srcIdx;
			bytesWritten = dstIdx;
			return retVal;

		DestTooSmall:
			retVal = OperationStatus.DestinationTooSmall;
			goto CommonReturn;

		NeedMoreData:
			retVal = OperationStatus.NeedMoreData;
			goto CommonReturn;
		}


#if NET8_0_OR_GREATER
		private SearchValues<byte> GetAllowedAsciiSvU8() 
			=> SearchValues.Create(GetAllowedAscii().Select(c => (byte)c).ToArray());

		private SearchValues<char> GetAllowedAsciiSv()
			=> SearchValues.Create(GetAllowedAscii());

		//		static readonly SearchValues<char> _sv_ascii_ok_subset = SearchValues.Create(" !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~");
		Lazy<SearchValues<char>> _sv_allowed_ascii = null!;
		Lazy<SearchValues<byte>> _sv_allowed_ascii_u8 = null!;

		//static string GetSearchValuesDisallowed()
		//{
		//	StringBuilder sb = new();

		//	// control chars
		//	for (int i = 0; i <= 0x1f; i++)
		//		sb.Append((char)i);

		//	sb.Append('"');
		//	sb.Append('\\');

		//	//	Surrogates: d800-dfff
		//	for (int i = 0xd800; i <= 0xdfff; i++)
		//		sb.Append((char)i);

		//	return sb.ToString();
		//}

		string GetAllowedAscii()
		{
			StringBuilder sb = new();

			for (int i = 0; i < 128; i++)
			{
				var c = (char)i;
				if (!MustEscapeChar(c))
					sb.Append(c);
			}

			return sb.ToString();
		}
#endif


		public override int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> data)
		{
			// To test the encoder fully, return 0 here to force encoding everything.
			//return 0;

			int dataOriginalLength = data.Length;

#if NET8_0_OR_GREATER
			// if we are lucky, we can skip a lot of valid ascii that don't need encoding, and we skip it faster with SearchValues
			int i = data.IndexOfAnyExcept(_sv_allowed_ascii_u8.Value);
			if (i == -1)
				return -1; // all data was allowed ascii

			// OPT: if the first disallowed char is ascii, we can return immediately (we know it is a char that needs escaping)
			if (UnicodeUtility.IsAsciiCodePoint(data[i]))
				return i;

			data = data.Slice(i);
			Debug.Assert(!data.IsEmpty);

#endif
			// If there's any leftover data, try consuming it now.
			while (!data.IsEmpty)
			{
				OperationStatus opStatus = Rune.DecodeFromUtf8(data, out Rune scalarValue, out int bytesConsumed);
				if (opStatus != OperationStatus.Done) { break; } // bad data found, must escape
																 //if (bytesConsumed >= 4) { break; } // found supplementary code point, must escape

				//UnicodeDebug.AssertIsBmpCodePoint((uint)scalarValue.Value);
				//if (!_allowedBmpCodePoints.IsCharAllowed((char)scalarValue.Value)) { break; } // disallowed code point
				if (MustEscapeChar((char)scalarValue.Value)) { break; } // disallowed code point
				data = data.Slice(bytesConsumed);
			}

			return (data.IsEmpty) ? -1 : dataOriginalLength - data.Length;
		}

		public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
			// To test the encoder fully, return 0 here to force encoding everything.
			//return 0;

			int i = 0;

#if NET8_0_OR_GREATER
			// if we are lucky, we can skip a lot of valid ascii that don't need encoding, and we skip it faster with SearchValues
			var data = new ReadOnlySpan<char>(text, textLength);
			i = data.IndexOfAnyExcept(_sv_allowed_ascii.Value);
			if (i == -1)
				return -1; // all data was allowed ascii

			// OPT: if the first disallowed char is ascii, we can return immediately (we know it is a char that needs escaping)
			if (UnicodeUtility.IsAsciiCodePoint(text[i]))
				return i;
#endif
			if (i >= 0)
			{
				_AssertThisNotNull(); // hoist "this != null" check out of hot loop below (what the dickens does this mean???)

				for (int index = i; index < textLength; ++index)
				{
					char value = text[index];
					if (MustEscapeChar(value))
						return index;

					if (char.IsSurrogate(value))
					{
						// do we have one more char?
						if (index + 1 < textLength && char.IsSurrogatePair(value, text[index + 1]))
						{
							// ok, skip next char (low surrogate)
							++index;
						}
						else
						{
							// no more chars (possibly unmatched surrogate) or invalid surrogate. encoder must handle it.
							return index;
						}
					}
				}
			}

			return -1; // all characters allowed (nothing to encode)
		}

		public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
        {
			Span<char> span = new(buffer, bufferLength);
			var res = _scalarEscaper.EncodeUtf16(new Rune(unicodeScalar), span, _lowerCaseHex);
			if (res >= 0)
			{
				numberOfCharactersWritten = res;
				return true;
			}
			else
			{
				numberOfCharactersWritten = 0;
				return false;
			}
        }

		public override bool WillEncode(int unicodeScalar)
		{
			if (unicodeScalar > char.MaxValue)
			{
				return false;
			}

			return MustEscapeChar((char)unicodeScalar);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _AssertThisNotNull()
		{
			// Used for hoisting "'this' is not null" assertions outside hot loops.
			if (GetType() == typeof(MinimalJsonEncoder)) { /* intentionally left blank */ }
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MustEscapeChar(char c)
		{
			return c < 128 && _mustEscapeAscii[c];
		}

		private static bool[] CreateEscapeMap(char[]? extraAsciiEscapeChars)
		{
			bool[] res = new bool[128];

			// https://datatracker.ietf.org/doc/html/rfc8259#section-7
			res['"'] = true;
			res['\\'] = true;

			// control chars
			for (int i = 0; i < 0x20; i++)
				res[i] = true;

			if (extraAsciiEscapeChars != null)
				for (int i = 0; i < extraAsciiEscapeChars.Length; i++)
					res[extraAsciiEscapeChars[i]] = true;

			return res;
		}
	}
}
