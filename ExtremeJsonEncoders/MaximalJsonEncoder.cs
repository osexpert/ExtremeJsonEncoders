using System;
using System.Collections.Generic;
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
    public class MaximalJsonEncoder : JavaScriptEncoder
    {
        public static readonly MaximalJsonEncoder Shared = new();

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

		public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
            if (textLength == 0)
            {
                return -1;
            }

            return 0;
        }

        public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength,
            out int numberOfCharactersWritten)
        {
			const int unicodeEscapeLength = 6; // "\\u0000".Length

			if (bufferLength < unicodeEscapeLength)
			{
				numberOfCharactersWritten = 0;
				return false;
			}

			Rune rune = new(unicodeScalar);
            int lengthNeeded = rune.Utf16SequenceLength * unicodeEscapeLength;

            if (bufferLength < lengthNeeded)
            {
                numberOfCharactersWritten = 0;
                return false;
            }

			//const int maxCharsPerScalar = 2;
			//char* utf16Buffer = stackalloc char[maxCharsPerScalar];
			//int utf16Length = rune.EncodeToUtf16(new Span<char>(utf16Buffer, maxCharsPerScalar));
			//Span<char> span = new(buffer, bufferLength);

			//rune.IsBmp

			//if (utf16Length > 1)
			//{
			//}

			//         for (int index = 0; index < utf16Length; ++index)
			//         {
			//             char toEncode = utf16Buffer[index];
			//             span[0] = '\\';
			//             span[1] = 'u';
			//             span[2] = ToHexDigit((toEncode & 0xf000) >> 12);
			//             span[3] = ToHexDigit((toEncode & 0xf00) >> 8);
			//             span[4] = ToHexDigit((toEncode & 0xf0) >> 4);
			//             span[5] = ToHexDigit(toEncode & 0xf);
			//             span = span.Slice(unicodeEscapeLength);
			//         }

			Span<char> span = new(buffer, bufferLength);

			if (rune.Utf16SequenceLength == 2)
			{
				const int maxCharsPerScalar = 2;
				char* utf16Buffer = stackalloc char[maxCharsPerScalar];
				if (rune.EncodeToUtf16(new Span<char>(utf16Buffer, maxCharsPerScalar)) != maxCharsPerScalar)
					throw new Exception("Not " + maxCharsPerScalar);

				char highSurrogate = utf16Buffer[0];
				char lowSurrogate = utf16Buffer[1];

				span[0] = '\\';
				span[1] = 'u';
				span[2] = ToHexDigit((highSurrogate & 0xf000) >> 12);
				span[3] = ToHexDigit((highSurrogate & 0xf00) >> 8);
				span[4] = ToHexDigit((highSurrogate & 0xf0) >> 4);
				span[5] = ToHexDigit(highSurrogate & 0xf);
				span[6] = '\\';
				span[7] = 'u';
				span[8] = ToHexDigit((lowSurrogate & 0xf000) >> 12);
				span[9] = ToHexDigit((lowSurrogate & 0xf00) >> 8);
				span[10] = ToHexDigit((lowSurrogate & 0xf0) >> 4);
				span[11] = ToHexDigit(lowSurrogate & 0xf);
			}
			else if (rune.Utf16SequenceLength == 1)
			{
				char toEncode = (char)unicodeScalar;
				span[0] = '\\';
				span[1] = 'u';
				span[2] = ToHexDigit((toEncode & 0xf000) >> 12);
				span[3] = ToHexDigit((toEncode & 0xf00) >> 8);
				span[4] = ToHexDigit((toEncode & 0xf0) >> 4);
				span[5] = ToHexDigit(toEncode & 0xf);
			}
			else
				throw new Exception("Impossible Utf16SequenceLength: " + rune.Utf16SequenceLength);

			numberOfCharactersWritten = lengthNeeded;
            return true;
        }

        public override bool WillEncode(int unicodeScalar)
        {
            return true;
        }

		static char ToHexDigit(int value)
        {
            if (value > 0xf)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (value < 10)
            {
                return (char)(value + '0');
            }
            else
            {
                return (char)(value - 0xa + 'a');
            }
        }
    }
}
