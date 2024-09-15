using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExtremeJsonEncoders
{
	internal struct AllowAllExceptEscapeChars : IAllowedBmpCodePointsBitmap
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool IsCharAllowed(char value)
		{
			return !(value == '"' || value == '\\' || value <= '\u001f');
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool IsCodePointAllowed(uint value)
		{
			return value > char.MaxValue || IsCharAllowed((char)value);
		}
	}
}
