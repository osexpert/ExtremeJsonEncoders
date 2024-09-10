using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MinimalJsonEncoder : JavaScriptEncoder
    {
        public static readonly MinimalJsonEncoder Shared = new();

        public override int MaxOutputCharactersPerInputCharacter => 6;

        public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
            for (int index = 0; index < textLength; ++index)
            {
                char value = text[index];

                if (MustBeEscaped(value))
                {
                    return index;
                }
            }

            return -1;
        }

        public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength,
            out int numberOfCharactersWritten)
        {
			bool escape = WillEncode(unicodeScalar);

            if (!escape)
            {
                Span<char> span = new(buffer, bufferLength);
				bool succeeded = new Rune(unicodeScalar).TryEncodeToUtf16(span, out int spanWritten);
				numberOfCharactersWritten = spanWritten;
                return succeeded;
            }

            if (unicodeScalar <= char.MaxValue && HasTwoCharacterEscape((char)unicodeScalar, out var suffix))// unicodeScalar == '"' || unicodeScalar == '\\')
            {
				if (bufferLength < 2)
				{
					numberOfCharactersWritten = 0;
					return false;
				}

				buffer[0] = '\\';
                buffer[1] = suffix;
                numberOfCharactersWritten = 2;
                return true;
            }
            else
            {
                if (bufferLength < 6)
                {
                    numberOfCharactersWritten = 0;
                    return false;
                }

                buffer[0] = '\\';
                buffer[1] = 'u';
                buffer[2] = '0';
                buffer[3] = '0';
                buffer[4] = ToHexDigit((unicodeScalar & 0xf0) >> 4);
                buffer[5] = ToHexDigit(unicodeScalar & 0xf);
                numberOfCharactersWritten = 6;
                return true;
            }
        }

		private bool HasTwoCharacterEscape(char value, out char suffix)
		{
			// RFC 8259, Section 7, "char = " BNF
			switch (value)
			{
				case '"':
					suffix = '"';
					return true;
				case '\\':
					suffix = '\\';
					return true;
				case '/':
					suffix = '/';
					return true;
				case '\b':
					suffix = 'b';
					return true;
				case '\f':
					suffix = 'f';
					return true;
				case '\n':
					suffix = 'n';
					return true;
				case '\r':
					suffix = 'r';
					return true;
				case '\t':
					suffix = 't';
					return true;
				default:
					suffix = '\0';
					return false;
			}
		}

		public override bool WillEncode(int unicodeScalar)
        {
            if (unicodeScalar > char.MaxValue)
            {
                return false;
            }

            return MustBeEscaped((char)unicodeScalar);
        }

        static bool MustBeEscaped(char value)
        {
			// https://datatracker.ietf.org/doc/html/rfc8259#section-7
			return value == '"' || value == '\\' || value <= '\u001f';
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
