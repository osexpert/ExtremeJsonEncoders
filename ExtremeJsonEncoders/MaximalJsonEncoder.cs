using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ExtremeJsonEncoders
{
    /// <summary>
    /// Escape every char as \u####
    /// 
    /// Just for fun/to prove a point:-)
    /// Default JSON escaping is biased against other languages #86805 
    /// https://github.com/dotnet/runtime/issues/86805
    /// </summary>
    public class MaximalJsonEncoder : JavaScriptEncoder, IMustEscapeChar
	{
        public static readonly MaximalJsonEncoder Shared = new();

		private readonly AsciiPreescapedData _asciiPreescapedData;

		private readonly ScalarEscaperBase _scalarEscaper;

		private readonly bool _lowerCaseHex;

		public MaximalJsonEncoder(bool shortEscapes = false, bool lowerCaseHex = false)
		{
			_lowerCaseHex = lowerCaseHex;
			_scalarEscaper = shortEscapes ? EscaperImplementation.SingletonPreescape : EscaperImplementation.SingletonNoPreescape;
			_asciiPreescapedData.PopulatePreescapedData(this, _scalarEscaper, lowerCaseHex);
		}

		/*
		was:
		public override int MaxOutputCharactersPerInputCharacter => 12;

		But even the runtime uses 6:
		https://github.com/dotnet/runtime/blob/c5340807568ba33edc74f61ed05dafe8e030362e/src/libraries/System.Text.Encodings.Web/src/System/Text/Encodings/Web/DefaultJavaScriptEncoder.cs#L39
		public override int MaxOutputCharactersPerInputCharacter => 6; // "\uXXXX" for a single char ("\uXXXX\uYYYY" [12 chars] for supplementary scalar value)
		Also MaxOutputCharactersPerInputCharacter does not seem to be used or called from anywhere.
		But changing to 6 here too:
		*/
		public override int MaxOutputCharactersPerInputCharacter => 6;


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
				//{
				//	if (!scalarValue.TryEncodeToUtf16(destination.Slice(dstIdx), out int utf16CodeUnitCount))
				//	{
				//		goto DestTooSmall;
				//	}

				//	dstIdx += utf16CodeUnitCount;
				//	srcIdx += utf16CodeUnitCount;
				//	continue;
				//}

				//MustEncodeNonAscii:

				//	// At this point, we know we need to encode.
				int charsWrittenJustNow = _scalarEscaper.EncodeUtf16(scalarValue, destination.Slice(dstIdx), _lowerCaseHex);
				if (charsWrittenJustNow < 0)
				{
					goto DestTooSmall;
				}

				dstIdx += charsWrittenJustNow;
				srcIdx += scalarValue.Utf16SequenceLength;
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

				//uint
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
				//{
				//	if (!scalarValue.TryEncodeToUtf8(destination.Slice(dstIdx), out int utf8CodeUnitCount))
				//	{
				//		goto DestTooSmall;
				//	}
				//	dstIdx += utf8CodeUnitCount;
				//	srcIdx += utf8CodeUnitCount;
				//	continue;
				//}

				//MustEncodeNonAscii:

				//	// At this point, we know we need to encode.

				int bytesWrittenJustNow = _scalarEscaper.EncodeUtf8(scalarValue, destination.Slice(dstIdx), _lowerCaseHex);
				if (bytesWrittenJustNow < 0)
				{
					goto DestTooSmall;
				}

				dstIdx += bytesWrittenJustNow;
				srcIdx += bytesConsumedJustNow;
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

		public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
            if (textLength == 0)
            {
                return -1;
            }

            return 0;
        }

		public bool MustEscapeChar(char c)
		{
			return true;
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
            return true;
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _AssertThisNotNull()
		{
			// Used for hoisting "'this' is not null" assertions outside hot loops.
			if (GetType() == typeof(MaximalJsonEncoder)) { /* intentionally left blank */ }
		}
	}
}
