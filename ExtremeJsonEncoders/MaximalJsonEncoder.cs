using System;
using System.Collections.Generic;
using System.Linq;
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

        public override int MaxOutputCharactersPerInputCharacter => 12;

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
            Rune rune = new Rune(unicodeScalar);
            const int unicodeEscapeLength = 6; // "\\u0000".Length
            int lengthNeeded = rune.Utf16SequenceLength * unicodeEscapeLength;

            if (bufferLength < lengthNeeded)
            {
                numberOfCharactersWritten = 0;
                return false;
            }

            const int maxCharsPerScalar = 2;
            char* utf16Buffer = stackalloc char[maxCharsPerScalar];
            int utf16Length = rune.EncodeToUtf16(new Span<char>(utf16Buffer, maxCharsPerScalar));
            Span<char> span = new Span<char>(buffer, bufferLength);

            for (int index = 0; index < utf16Length; ++index)
            {
                char toEncode = utf16Buffer[index];
                span[0] = '\\';
                span[1] = 'u';
                span[2] = ToHexDigit((toEncode & 0xf000) >> 12);
                span[3] = ToHexDigit((toEncode & 0xf00) >> 8);
                span[4] = ToHexDigit((toEncode & 0xf0) >> 4);
                span[5] = ToHexDigit(toEncode & 0xf);
                span = span.Slice(unicodeEscapeLength);
            }

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

            if (value <= 10)
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
