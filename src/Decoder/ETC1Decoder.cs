using System;
using System.Runtime.CompilerServices;

namespace ArknightsResources.Utility.Decoder
{
    internal static class ETC1Decoder
    {
        // 本类参考了AssetStudio项目来解码ETC1图片
        // AssetStudio项目地址:https://github.com/Perfare/AssetStudio
        // 下面附上AssetStudio项目的许可证原文
        #region LICENSE
        /*
        MIT License

        Copyright (c) 2016 Radu
        Copyright (c) 2016-2020 Perfare

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
         */
        #endregion

        private static readonly byte[][] Etc1SubblockTable = new byte[][]
        {
            new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 },
            new byte[] {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 }
        };

        private static readonly byte[][] Etc1ModifierTable = new byte[][]
        {
            new byte[] {2, 8},
            new byte[] {5, 17},
            new byte[] {9, 29},
            new byte[] {13, 42},
            new byte[] {18, 60},
            new byte[] {24, 80},
            new byte[] {33, 106},
            new byte[] {47, 183}
        };

        private static readonly byte[] WriteOrderTable = new byte[]
        {
            0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15
        };

        public unsafe static void DecodeETC1Block(ReadOnlySpan<byte> data, Span<uint> buffer)
        {
            byte* c0 = stackalloc byte[3];
            byte* c1 = stackalloc byte[3];
            byte** c = stackalloc byte*[] { c0, c1 };

            Span<byte> code = stackalloc byte[] { (byte)(data[3] >> 5), (byte)(data[3] >> 2 & 7) };  // Table codewords
            Span<byte> table = Etc1SubblockTable[data[3] & 1];

            if ((data[3] & 2) != 0)
            {
                // diff bit == 1
                c[0][0] = (byte)(data[0] & 0xf8);
                c[0][1] = (byte)(data[1] & 0xf8);
                c[0][2] = (byte)(data[2] & 0xf8);

                c[1][0] = (byte)(c[0][0] + (data[0] << 3 & 0x18) - (data[0] << 3 & 0x20));
                c[1][1] = (byte)(c[0][1] + (data[1] << 3 & 0x18) - (data[1] << 3 & 0x20));
                c[1][2] = (byte)(c[0][2] + (data[2] << 3 & 0x18) - (data[2] << 3 & 0x20));

                c[0][0] |= (byte)(c[0][0] >> 5);
                c[0][1] |= (byte)(c[0][1] >> 5);
                c[0][2] |= (byte)(c[0][2] >> 5);

                c[1][0] |= (byte)(c[1][0] >> 5);
                c[1][1] |= (byte)(c[1][1] >> 5);
                c[1][2] |= (byte)(c[1][2] >> 5);
            }
            else
            {
                // diff bit == 0
                c[0][0] = (byte)((data[0] & 0xf0) | data[0] >> 4);
                c[1][0] = (byte)((data[0] & 0x0f) | data[0] << 4);

                c[0][1] = (byte)((data[1] & 0xf0) | data[1] >> 4);
                c[1][1] = (byte)((data[1] & 0x0f) | data[1] << 4);

                c[0][2] = (byte)((data[2] & 0xf0) | data[2] >> 4);
                c[1][2] = (byte)((data[2] & 0x0f) | data[2] << 4);
            }

            int j = data[6] << 8 | data[7];  // less significant pixel index bits
            int k = data[4] << 8 | data[5];  // more significant pixel index bits
            for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
            {
                byte s = table[i];
                byte m = Etc1ModifierTable[code[s]][j & 1];
                buffer[WriteOrderTable[i]] = ApplicateColor(c[s], (k & 1) == 1 ? -m : m);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint ApplicateColor(byte* c, int m)
        {
            return Color(Clamp(c[0] + m), Clamp(c[1] + m), Clamp(c[2] + m), 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Clamp(int n)
        {
            return (byte)(n < 0 ? 0 : n > 255 ? 255 : n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Color(byte r, byte g, byte b, byte a)
        {
            return (uint)(b | (g << 8) | (r << 16) | (a << 24));
        }
    }
}
