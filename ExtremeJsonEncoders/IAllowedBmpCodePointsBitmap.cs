using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExtremeJsonEncoders
{
	internal interface IAllowedBmpCodePointsBitmap
	{
		bool IsCharAllowed(char c);
	}
}