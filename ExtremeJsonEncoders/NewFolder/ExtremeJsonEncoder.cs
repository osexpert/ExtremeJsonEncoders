#if false
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

//namespace System.Text.Encodings.Web
namespace ExtremeJsonEncoders
{
	public sealed class ExtremeJsonEncoder : JavaScriptEncoder
	{
		//internal static readonly DefaultJavaScriptEncoder BasicLatinSingleton = new DefaultJavaScriptEncoder(new TextEncoderSettings(UnicodeRanges.BasicLatin));
		//internal static readonly DefaultJavaScriptEncoder UnsafeRelaxedEscapingSingleton = new DefaultJavaScriptEncoder(new TextEncoderSettings(UnicodeRanges.All), allowMinimalJsonEscaping: true);
		public static readonly ExtremeJsonEncoder MinimalEscapingSingleton = new ExtremeJsonEncoder(/*new TextEncoderSettings(UnicodeRanges.All),*/ allowMinimalJsonEscaping: true);


		private readonly OptimizedInboxTextEncoder _innerEncoder;

		public ExtremeJsonEncoder()//TextEncoderSettings settings)
			: this(/*settings,*/ allowMinimalJsonEscaping: false)
		{
		}

		public ExtremeJsonEncoder(/*TextEncoderSettings settings, */bool allowMinimalJsonEscaping)
		{
			//if (settings is null)
			//{
			//	//ThrowHelper.ThrowArgumentNullException(ExceptionArgument.settings);
			//	throw new ArgumentNullException(nameof(settings));
			//}

			// '\' (U+005C REVERSE SOLIDUS) must always be escaped in Javascript / ECMAScript / JSON.
			// '/' (U+002F SOLIDUS) is not Javascript / ECMAScript / JSON-sensitive so doesn't need to be escaped.
			// '`' (U+0060 GRAVE ACCENT) is ECMAScript-sensitive (see ECMA-262).

			if ('\"' != '"')
			{
			}
			else
			{
			}

			AllowedBmpCodePointsBitmap all = new AllowedBmpCodePointsBitmap();
			//for (char c = char.MinValue; c < char.MaxValue; c++)
			//	all.AllowChar(c);

			//all.AllowChar(char.MaxValue);

			//for (char c = '\0'; c <= '\u001f'; c++)
			//	all.ForbidChar(c);

			//all.ForbidChar('\\');
			//all.ForbidChar('"');

			_innerEncoder = new OptimizedInboxTextEncoder(EscaperImplementation.SingletonMinimallyEscaped, all);
		}

		public override int MaxOutputCharactersPerInputCharacter => 6; // "\uXXXX" for a single char ("\uXXXX\uYYYY" [12 chars] for supplementary scalar value)

		/*
         * These overrides should be copied to all other subclasses that are backed
         * by the fast inbox escaping mechanism.
         */

#pragma warning disable CS0618 // some of the adapters are intentionally marked [Obsolete]
		//private protected override OperationStatus EncodeCore(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock)
		//	=> _innerEncoder.Encode(source, destination, out charsConsumed, out charsWritten, isFinalBlock);

		//private protected override OperationStatus EncodeUtf8Core(ReadOnlySpan<byte> utf8Source, Span<byte> utf8Destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
		//	=> _innerEncoder.EncodeUtf8(utf8Source, utf8Destination, out bytesConsumed, out bytesWritten, isFinalBlock);

		//private protected override int FindFirstCharacterToEncode(ReadOnlySpan<char> text)
		//	=> _innerEncoder.GetIndexOfFirstCharToEncode(text);


		public override OperationStatus Encode(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock = true)
			=> _innerEncoder.Encode(source, destination, out charsConsumed, out charsWritten, isFinalBlock);

		public override OperationStatus EncodeUtf8(ReadOnlySpan<byte> utf8Source, Span<byte> utf8Destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
			=> _innerEncoder.EncodeUtf8(utf8Source, utf8Destination, out bytesConsumed, out bytesWritten, isFinalBlock);

		

		public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
			=> _innerEncoder.FindFirstCharacterToEncode(text, textLength);

		public override int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> utf8Text)
			=> _innerEncoder.GetIndexOfFirstByteToEncode(utf8Text);

		public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
			=> _innerEncoder.TryEncodeUnicodeScalar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten);

		public override bool WillEncode(int unicodeScalar)
			=> !_innerEncoder.IsScalarValueAllowed(new Rune(unicodeScalar));
#pragma warning restore CS0618

		/*
         * End overrides section.
         */


	}


	
}
#endif