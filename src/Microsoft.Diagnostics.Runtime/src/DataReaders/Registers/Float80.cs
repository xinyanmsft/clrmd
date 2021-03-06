// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace Microsoft.Diagnostics.Runtime
{
    /// <summary>
    /// Float in X86-specific windows thread context.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Float80
    {
        [FieldOffset(0x0)]
        public ulong Mantissa;

        [FieldOffset(0x8)]
        public ushort Exponent;
    }
}
