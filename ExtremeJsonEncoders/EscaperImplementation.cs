using System;
using System.Collections.Generic;
using System.Text;
using ExtremeJsonEncoders;

namespace ExtremeJsonEncoders
{
	sealed class EscaperImplementation : ScalarEscaperBase
	{
		internal static readonly EscaperImplementation SingletonPreescape = new EscaperImplementation(true, true);

		// do not add 2 char escapes
		internal static readonly EscaperImplementation SingletonNoPreescape = new EscaperImplementation(false, false);

		// Map stores the second byte for any ASCII input that can be escaped as the two-element sequence
		// REVERSE SOLIDUS followed by a single character. For example, <LF> maps to the two chars "\n".
		// The map does not contain an entry for chars which cannot be escaped in this manner.
		private readonly AsciiByteMap _preescapedMap;

		private EscaperImplementation(bool addPreescapedMap, bool addPreescapedMap_allowMinimalEscaping)
		{
			if (addPreescapedMap)
			{
				_preescapedMap.InsertAsciiChar('\b', (byte)'b');
				_preescapedMap.InsertAsciiChar('\t', (byte)'t');
				_preescapedMap.InsertAsciiChar('\n', (byte)'n');
				_preescapedMap.InsertAsciiChar('\f', (byte)'f');
				_preescapedMap.InsertAsciiChar('\r', (byte)'r');
				_preescapedMap.InsertAsciiChar('\\', (byte)'\\');

				if (addPreescapedMap_allowMinimalEscaping)
				{
					_preescapedMap.InsertAsciiChar('\"', (byte)'\"');
				}
			}
		}

		// Writes a scalar value as a JavaScript-escaped character (or sequence of characters).
		// See ECMA-262, Sec. 7.8.4, and ECMA-404, Sec. 9
		// https://www.ecma-international.org/ecma-262/5.1/#sec-7.8.4
		// https://www.ecma-international.org/publications/files/ECMA-ST/ECMA-404.pdf
		//
		// ECMA-262 allows encoding U+000B as "\v", but ECMA-404 does not.
		// Both ECMA-262 and ECMA-404 allow encoding U+002F SOLIDUS as "\/"
		// (in ECMA-262 this character is a NonEscape character); however, we
		// don't encode SOLIDUS by default unless the caller has provided an
		// explicit bitmap which does not contain it. In this case we'll assume
		// that the caller didn't want a SOLIDUS written to the output at all,
		// so it should be written using "\u002F" encoding.
		// HTML-specific characters (including apostrophe and quotes) will
		// be written out as numeric entities for defense-in-depth.

		internal override int EncodeUtf8(Rune value, Span<byte> destination, bool lowerCaseHex)
		{
			if (_preescapedMap.TryLookup(value, out byte preescapedForm))
			{
				if (!SpanUtility.IsValidIndex(destination, 1)) { goto OutOfSpace; }
				destination[0] = (byte)'\\';
				destination[1] = preescapedForm;
				return 2;

			OutOfSpace:
				return -1;
			}

			return TryEncodeScalarAsHex(this, value, destination, lowerCaseHex);

#pragma warning disable IDE0060 // 'this' taken explicitly to avoid argument shuffling by caller
			static int TryEncodeScalarAsHex(object @this, Rune value, Span<byte> destination, bool lowerCaseHex)
#pragma warning restore IDE0060
			{
				HexConverter.Casing casing = lowerCaseHex ? HexConverter.Casing.Lower : HexConverter.Casing.Upper;

				if (value.IsBmp)
				{
					// Write 6 bytes: "\uXXXX"
					if (!SpanUtility.IsValidIndex(destination, 5)) { goto OutOfSpaceInner; }
					destination[0] = (byte)'\\';
					destination[1] = (byte)'u';
					HexConverter.ToBytesBuffer((byte)value.Value, destination, 4, casing);
					HexConverter.ToBytesBuffer((byte)((uint)value.Value >> 8), destination, 2, casing);
					return 6;
				}
				else
				{
					// Write 12 bytes: "\uXXXX\uYYYY"
					UnicodeHelpers.GetUtf16SurrogatePairFromAstralScalarValue((uint)value.Value, out char highSurrogate, out char lowSurrogate);
					if (!SpanUtility.IsValidIndex(destination, 11)) { goto OutOfSpaceInner; }
					destination[0] = (byte)'\\';
					destination[1] = (byte)'u';
					HexConverter.ToBytesBuffer((byte)highSurrogate, destination, 4, casing);
					HexConverter.ToBytesBuffer((byte)((uint)highSurrogate >> 8), destination, 2, casing);
					destination[6] = (byte)'\\';
					destination[7] = (byte)'u';
					HexConverter.ToBytesBuffer((byte)lowSurrogate, destination, 10, casing);
					HexConverter.ToBytesBuffer((byte)((uint)lowSurrogate >> 8), destination, 8, casing);
					return 12;
				}

			OutOfSpaceInner:

				return -1;
			}
		}

		internal override int EncodeUtf16(Rune value, Span<char> destination, bool lowerCaseHex)
		{
			if (_preescapedMap.TryLookup(value, out byte preescapedForm))
			{
				if (!SpanUtility.IsValidIndex(destination, 1)) { goto OutOfSpace; }
				destination[0] = '\\';
				destination[1] = (char)preescapedForm;
				return 2;

			OutOfSpace:
				return -1;
			}

			return TryEncodeScalarAsHex(this, value, destination, lowerCaseHex);

#pragma warning disable IDE0060 // 'this' taken explicitly to avoid argument shuffling by caller
			static int TryEncodeScalarAsHex(object @this, Rune value, Span<char> destination, bool lowerCaseHex)
#pragma warning restore IDE0060
			{
				HexConverter.Casing casing = lowerCaseHex ? HexConverter.Casing.Lower : HexConverter.Casing.Upper;

				if (value.IsBmp)
				{
					// Write 6 chars: "\uXXXX"
					if (!SpanUtility.IsValidIndex(destination, 5)) { goto OutOfSpaceInner; }
					destination[0] = '\\';
					destination[1] = 'u';
					HexConverter.ToCharsBuffer((byte)value.Value, destination, 4, casing);
					HexConverter.ToCharsBuffer((byte)((uint)value.Value >> 8), destination, 2, casing);
					return 6;
				}
				else
				{
					// Write 12 chars: "\uXXXX\uYYYY"
					UnicodeHelpers.GetUtf16SurrogatePairFromAstralScalarValue((uint)value.Value, out char highSurrogate, out char lowSurrogate);
					if (!SpanUtility.IsValidIndex(destination, 11)) { goto OutOfSpaceInner; }
					destination[0] = '\\';
					destination[1] = 'u';
					HexConverter.ToCharsBuffer((byte)highSurrogate, destination, 4, casing);
					HexConverter.ToCharsBuffer((byte)((uint)highSurrogate >> 8), destination, 2, casing);
					destination[6] = '\\';
					destination[7] = 'u';
					HexConverter.ToCharsBuffer((byte)lowSurrogate, destination, 10, casing);
					HexConverter.ToCharsBuffer((byte)((uint)lowSurrogate >> 8), destination, 8, casing);
					return 12;
				}

			OutOfSpaceInner:

				return -1;
			}
		}
	}
}
