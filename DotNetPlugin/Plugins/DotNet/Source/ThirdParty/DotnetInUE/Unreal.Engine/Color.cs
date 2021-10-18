// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Unreal
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public struct Color
    {
        public byte B, G, R, A;
        public Color(byte r, byte g, byte b, byte a = 255)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        public readonly unsafe uint GetValue()
        {
            fixed (byte* b = &B)
                return *(uint*) b;
        }

        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color Transparent = new Color(0, 0, 0, 0);
        public static readonly Color Red = new Color(255, 0, 0);
        public static readonly Color Green = new Color(0, 255, 0);
        public static readonly Color Blue = new Color(0, 0, 255);
        public static readonly Color Yellow = new Color(255, 255, 0);
        public static readonly Color Cyan = new Color(0, 255, 255);
        public static readonly Color Magenta = new Color(255, 0, 255);
        public static readonly Color Orange = new Color(243, 156, 18);
        public static readonly Color Purple = new Color(169, 7, 228);
        public static readonly Color Turquoise = new Color(26, 188, 156);
        public static readonly Color Silver = new Color(189, 195, 199);
        public static readonly Color Emerald = new Color(46, 204, 113);
    }
}