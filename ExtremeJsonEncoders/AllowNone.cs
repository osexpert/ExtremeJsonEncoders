using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExtremeJsonEncoders
{
	internal struct AllowNone : IAllowedBmpCodePointsBitmap
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool IsCharAllowed(char value)
		{
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool IsCodePointAllowed(uint value)
		{
			return false;
		}
	}
	
}
