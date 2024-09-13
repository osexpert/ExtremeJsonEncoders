// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET
using System.Runtime.Intrinsics;

#endif
using System.Text;

//namespace System.Text.Encodings.Web
namespace ExtremeJsonEncoders
{
	internal sealed partial class OptimizedInboxTextEncoder
	{
		/// <summary>
		/// A bitmap which represents allowed ASCII code points.
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		private unsafe partial struct AllowedAsciiCodePoints
		{
			[FieldOffset(0)] // ensure same offset with AsVector field
			private fixed byte AsBytes[16];

#if NET
#if !TARGET_BROWSER
			[FieldOffset(0)] // ensure same offset with AsBytes field
			internal Vector128<byte> AsVector;
#else
            // This member shouldn't be accessed from browser-based code paths.
            // All call sites should be trimmed away, which will also trim this member
            // and the type hierarchy it links to.
#pragma warning disable CA1822
            internal Vector128<byte> AsVector => throw new PlatformNotSupportedException();
#pragma warning restore CA1822
#endif
#endif

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal readonly bool IsAllowedAsciiCodePoint(uint codePoint)
			{
				if (codePoint > 0x7F)
				{
					return false; // non-ASCII
				}

				uint mask = AsBytes[codePoint & 0xF];
				if ((mask & (0x1u << (int)(codePoint >> 4))) == 0)
				{
					return false; // ASCII but disallowed
				}

				return true;
			}

			internal void PopulateAllowedCodePoints(in AllowedBmpCodePointsBitmap allowedBmpCodePoints)
			{
				this = default; // clear all existing data

				// we only care about ASCII non-control chars; all control chars and non-ASCII chars are disallowed
				for (int i = 0x20; i < 0x7F; i++)
				{
					if (allowedBmpCodePoints.IsCharAllowed((char)i))
					{
						AsBytes[i & 0xF] |= (byte)(1 << (i >> 4));
					}
				}
			}
		}

	
	}

	
}
