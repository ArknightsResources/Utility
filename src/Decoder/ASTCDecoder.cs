using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ArknightsResources.Utility.Decoder
{
    internal static class ASTCDecoder
    {
        // 本类参考了AssetStudio项目来解码ASTC图片
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

        #region Long and long chaos
        static readonly int[] BitReverseTable =
        {
            0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0, 0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0, 0x08, 0x88,
            0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8, 0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8, 0x04, 0x84, 0x44, 0xC4,
            0x24, 0xA4, 0x64, 0xE4, 0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4, 0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC,
            0x6C, 0xEC, 0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC, 0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2,
            0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2, 0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA, 0x1A, 0x9A,
            0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA, 0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6, 0x16, 0x96, 0x56, 0xD6,
            0x36, 0xB6, 0x76, 0xF6, 0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE, 0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE,
            0x7E, 0xFE, 0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1, 0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
            0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9, 0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9, 0x05, 0x85,
            0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5, 0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5, 0x0D, 0x8D, 0x4D, 0xCD,
            0x2D, 0xAD, 0x6D, 0xED, 0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD, 0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3,
            0x63, 0xE3, 0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3, 0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB,
            0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB, 0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7, 0x17, 0x97,
            0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7, 0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF, 0x1F, 0x9F, 0x5F, 0xDF,
            0x3F, 0xBF, 0x7F, 0xFF
        };

        static readonly int[] WeightPrecTableA = { 0, 0, 0, 3, 0, 5, 3, 0, 0, 0, 5, 3, 0, 5, 3, 0 };
        static readonly int[] WeightPrecTableB = { 0, 0, 1, 0, 2, 0, 1, 3, 0, 0, 1, 2, 4, 2, 3, 5 };

        static readonly int[] CemTableA = { 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 0, 0 };
        static readonly int[] CemTableB = { 8, 6, 5, 7, 5, 4, 6, 4, 3, 5, 3, 2, 4, 2, 1, 3, 1, 2, 1 };

        static readonly int[,] TritsTable =
        {
            { 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 1, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 1, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2 },
            { 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 1 },
            { 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 2, 2, 2, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 2, 2, 2, 2 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }
        };

        static readonly int[,] QuintsTable = {
        {
                0, 1, 2, 3, 4, 0, 4, 4, 0, 1, 2, 3, 4, 1, 4, 4, 0, 1, 2, 3, 4, 2, 4, 4, 0, 1, 2, 3, 4, 3, 4, 4, 0, 1, 2,
                3, 4, 0, 4, 0, 0, 1, 2, 3, 4, 1, 4, 1, 0, 1, 2, 3, 4, 2, 4, 2, 0, 1, 2, 3, 4, 3, 4, 3, 0, 1, 2, 3, 4, 0,
                2, 3, 0, 1, 2, 3, 4, 1, 2, 3, 0, 1, 2, 3, 4, 2, 2, 3, 0, 1, 2, 3, 4, 3, 2, 3, 0, 1, 2, 3, 4, 0, 0, 1, 0,
                1, 2, 3, 4, 1, 0, 1, 0, 1, 2, 3, 4, 2, 0, 1, 0, 1, 2, 3, 4, 3, 0, 1
        },
        {
                0, 0, 0, 0, 0, 4, 4, 4, 1, 1, 1, 1, 1, 4, 4, 4, 2, 2, 2, 2, 2, 4, 4, 4, 3, 3, 3, 3, 3, 4, 4, 4, 0, 0, 0,
                0, 0, 4, 0, 4, 1, 1, 1, 1, 1, 4, 1, 4, 2, 2, 2, 2, 2, 4, 2, 4, 3, 3, 3, 3, 3, 4, 3, 4, 0, 0, 0, 0, 0, 4,
                0, 0, 1, 1, 1, 1, 1, 4, 1, 1, 2, 2, 2, 2, 2, 4, 2, 2, 3, 3, 3, 3, 3, 4, 3, 3, 0, 0, 0, 0, 0, 4, 0, 0, 1,
                1, 1, 1, 1, 4, 1, 1, 2, 2, 2, 2, 2, 4, 2, 2, 3, 3, 3, 3, 3, 4, 3, 3
        },
        {
                0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 4, 0, 0, 0, 0, 0, 0, 2, 4, 0, 0, 0, 0, 0, 0, 3, 4, 1, 1, 1,
                1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 2, 2, 2, 2, 2, 2,
                4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4, 3,
                3, 3, 3, 3, 3, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4
        }};
        #endregion

        public static unsafe void DecodeASTCBlock(ReadOnlySpan<byte> data, Span<uint> buffer, int blockSize)
        {
            if (data[0] == 0xfc && (data[1] & 1) == 1)
            {
                uint c;
                if ((data[1] & 2) != 0)
                {
                    c = Color(data[8], data[10], data[12], data[14]);
                }
                else
                {
                    c = Color(data[9], data[11], data[13], data[15]);
                }

                for (int i = 0; i < blockSize * blockSize; i++)
                {
                    buffer[i] = c;
                }
            }
            else if (((data[0] & 0xc3) == 0xc0 && (data[1] & 1) == 1) || (data[0] & 0xf) == 0)
            {
                uint c = Color(255, 0, 255, 255);
                for (int i = 0; i < blockSize * blockSize; i++)
                {
                    buffer[i] = c;
                }
            }
            else
            {
                BlockData blockData = new BlockData
                {
                    bw = blockSize,
                    bh = blockSize
                };

                DecodeBlockParams(data, ref blockData);
                DecodeEndpoints(data, ref blockData);
                DecodeWeights(data, ref blockData);
                if (blockData.part_num > 1)
                {
                    SelectPartition(data, ref blockData);
                }
                ApplicateColor(ref blockData, buffer);
            }
        }

        private static unsafe void ApplicateColor(ref BlockData data, Span<uint> outbuf)
        {
            delegate*<int, int, int, byte>* FuncTableC = stackalloc delegate*<int, int, int, byte>[]
            {
                &SelectColor, &SelectColor, &SelectColorHdr, &SelectColorHdr, &SelectColor,
                &SelectColor, &SelectColor, &SelectColorHdr, &SelectColor, &SelectColor, 
                &SelectColor, &SelectColorHdr, &SelectColor, &SelectColor, &SelectColorHdr, &SelectColorHdr 
            };

            delegate*<int, int, int, byte>* FuncTableA = stackalloc delegate*<int, int, int, byte>[]
            {
                &SelectColor, &SelectColor, &SelectColorHdr, &SelectColorHdr, &SelectColor,
                &SelectColor, &SelectColor, &SelectColorHdr, &SelectColor, &SelectColor,
                &SelectColor, &SelectColorHdr, &SelectColor, &SelectColor, &SelectColor, &SelectColorHdr
            };

            if (data.dual_plane != 0)
            {
                Span<int> ps = stackalloc int[] { 0, 0, 0, 0 };
                ps[data.plane_selector] = 1;
                if (data.part_num > 1)
                {
                    for (int i = 0; i < data.bw * data.bh; i++)
                    {
                        int p = data.partition[i];
                        byte r =
                          FuncTableC[data.cem[p]](data.endpoints[p][0], data.endpoints[p][4], data.weights[i][ps[0]]);
                        byte g =
                          FuncTableC[data.cem[p]](data.endpoints[p][1], data.endpoints[p][5], data.weights[i][ps[1]]);
                        byte b =
                          FuncTableC[data.cem[p]](data.endpoints[p][2], data.endpoints[p][6], data.weights[i][ps[2]]);
                        byte a =
                          FuncTableA[data.cem[p]](data.endpoints[p][3], data.endpoints[p][7], data.weights[i][ps[3]]);
                        outbuf[i] = Color(r, g, b, a);
                    }
                }
                else
                {
                    for (int i = 0; i < data.bw * data.bh; i++)
                    {
                        byte r =
                          FuncTableC[data.cem[0]](data.endpoints[0][0], data.endpoints[0][4], data.weights[i][ps[0]]);
                        byte g =
                          FuncTableC[data.cem[0]](data.endpoints[0][1], data.endpoints[0][5], data.weights[i][ps[1]]);
                        byte b =
                          FuncTableC[data.cem[0]](data.endpoints[0][2], data.endpoints[0][6], data.weights[i][ps[2]]);
                        byte a =
                          FuncTableA[data.cem[0]](data.endpoints[0][3], data.endpoints[0][7], data.weights[i][ps[3]]);
                        outbuf[i] = Color(r, g, b, a);
                    }
                }
            }
            else if (data.part_num > 1)
            {
                for (int i = 0; i < data.bw * data.bh; i++)
                {
                    int p = data.partition[i];
                    byte r =
                      FuncTableC[data.cem[p]](data.endpoints[p][0], data.endpoints[p][4], data.weights[i][0]);
                    byte g =
                      FuncTableC[data.cem[p]](data.endpoints[p][1], data.endpoints[p][5], data.weights[i][0]);
                    byte b =
                      FuncTableC[data.cem[p]](data.endpoints[p][2], data.endpoints[p][6], data.weights[i][0]);
                    byte a =
                      FuncTableA[data.cem[p]](data.endpoints[p][3], data.endpoints[p][7], data.weights[i][0]);
                    outbuf[i] = Color(r, g, b, a);
                }
            }
            else
            {
                for (int i = 0; i < data.bw * data.bh; i++)
                {
                    byte r =
                      FuncTableC[data.cem[0]](data.endpoints[0][0], data.endpoints[0][4], data.weights[i][0]);
                    byte g =
                      FuncTableC[data.cem[0]](data.endpoints[0][1], data.endpoints[0][5], data.weights[i][0]);
                    byte b =
                      FuncTableC[data.cem[0]](data.endpoints[0][2], data.endpoints[0][6], data.weights[i][0]);
                    byte a =
                      FuncTableA[data.cem[0]](data.endpoints[0][3], data.endpoints[0][7], data.weights[i][0]);
                    outbuf[i] = Color(r, g, b, a);
                }
            }
        }

        private static byte SelectColor(int v0, int v1, int weight)
        {
            return (byte)(((((v0 << 8 | v0) * (64 - weight) + (v1 << 8 | v1) * weight + 32) >> 6) * 255 + 32768) / 65536);
        }

        private static byte SelectColorHdr(int v0, int v1, int weight)
        {
            ushort c = (ushort)(((v0 << 4) * (64 - weight) + (v1 << 4) * weight + 32) >> 6);
            ushort m = (ushort)(c & 0x7ff);
            if (m < 512)
                m *= 3;
            else if (m < 1536)
                m = (ushort)((4 * m) - 512);
            else
                m = (ushort)((5 * m) - 2048);
            float f = (c >> 1 & 0x7c00) | m >> 3;
            return (byte)(IsFinite(f) ? Clamp((int)Math.Round(f * 255)) : 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFinite(float f)
        {
#if NET
            return float.IsFinite(f);
#else
            return !float.IsInfinity(f) && !float.IsNaN(f);
#endif
        }

        private static void SelectPartition(ReadOnlySpan<byte> buf, ref BlockData data)
        {
            bool small_block = (data.bw * data.bh) < 31;
            int seed = (buf[0] >> 13 & 0x3ff) | (data.part_num - 1) << 10;

            uint rnum = (uint)seed;
            rnum ^= rnum >> 15;
            rnum -= rnum << 17;
            rnum += rnum << 7;
            rnum += rnum << 4;
            rnum ^= rnum >> 5;
            rnum += rnum << 16;
            rnum ^= rnum >> 7;
            rnum ^= rnum >> 3;
            rnum ^= rnum << 6;
            rnum ^= rnum >> 17;

            Span<int> seeds = stackalloc int[8];
            for (int i = 0; i < 8; i++)
            {
                seeds[i] = (int)((rnum >> (i * 4)) & 0xF);
                seeds[i] *= seeds[i];
            }

            Span<int> sh = stackalloc int[2] { (seed & 2) != 0 ? 4 : 5, data.part_num == 3 ? 6 : 5 };

            if ((seed & 1) != 0)
                for (int i = 0; i < 8; i++)
                    seeds[i] >>= sh[i % 2];
            else
                for (int i = 0; i < 8; i++)
                    seeds[i] >>= sh[1 - i % 2];

            if (small_block)
            {
                for (int t = 0, i = 0; t < data.bh; t++)
                {
                    for (int s = 0; s < data.bw; s++, i++)
                    {
                        int x = s << 1;
                        int y = t << 1;
                        int a = (int)((seeds[0] * x + seeds[1] * y + (rnum >> 14)) & 0x3f);
                        int b = (int)((seeds[2] * x + seeds[3] * y + (rnum >> 10)) & 0x3f);
                        int c = (int)(data.part_num < 3 ? 0 : (seeds[4] * x + seeds[5] * y + (rnum >> 6)) & 0x3f);
                        int d = (int)(data.part_num < 4 ? 0 : (seeds[6] * x + seeds[7] * y + (rnum >> 2)) & 0x3f);
                        data.partition[i] = (a >= b && a >= c && a >= d) ? 0 : (b >= c && b >= d) ? 1 : (c >= d) ? 2 : 3;
                    }
                }
            }
            else
            {
                for (int y = 0, i = 0; y < data.bh; y++)
                {
                    for (int x = 0; x < data.bw; x++, i++)
                    {
                        int a = (int)((seeds[0] * x + seeds[1] * y + (rnum >> 14)) & 0x3f);
                        int b = (int)((seeds[2] * x + seeds[3] * y + (rnum >> 10)) & 0x3f);
                        int c = (int)(data.part_num < 3 ? 0 : (seeds[4] * x + seeds[5] * y + (rnum >> 6)) & 0x3f);
                        int d = (int)(data.part_num < 4 ? 0 : (seeds[6] * x + seeds[7] * y + (rnum >> 2)) & 0x3f);
                        data.partition[i] = (a >= b && a >= c && a >= d) ? 0 : (b >= c && b >= d) ? 1 : (c >= d) ? 2 : 3;
                    }
                }
            }
        }

        private static void DecodeBlockParams(ReadOnlySpan<byte> buf, ref BlockData data)
        {
            data.dual_plane = ((buf[1] & 4) != 0) ? 1 : 0;
            data.weight_range = ((buf[0] >> 4) & 1) | ((buf[1] << 2) & 8);

            if ((buf[0] & 3) != 0)
            {
                data.weight_range |= (buf[0] << 1) & 6;

                switch (buf[0] & 0xc)
                {
                    case 0:
                        data.width = (buf[0] >> 7 & 3) + 4;
                        data.height = (buf[0] >> 5 & 3) + 2;
                        break;
                    case 4:
                        data.width = (buf[0] >> 7 & 3) + 8;
                        data.height = (buf[0] >> 5 & 3) + 2;
                        break;
                    case 8:
                        data.width = (buf[0] >> 5 & 3) + 2;
                        data.height = (buf[0] >> 7 & 3) + 8;
                        break;
                    case 12:
                        if ((buf[1] & 1) != 0)
                        {
                            data.width = (buf[0] >> 7 & 1) + 2;
                            data.height = (buf[0] >> 5 & 3) + 2;
                        }
                        else
                        {
                            data.width = (buf[0] >> 5 & 3) + 2;
                            data.height = (buf[0] >> 7 & 1) + 6;
                        }
                        break;
                }
            }
            else
            {
                data.weight_range |= buf[0] >> 1 & 6;
                switch (buf[0] & 0x180)
                {
                    case 0:
                        data.width = 12;
                        data.height = (buf[0] >> 5 & 3) + 2;
                        break;
                    case 0x80:
                        data.width = (buf[0] >> 5 & 3) + 2;
                        data.height = 12;
                        break;
                    case 0x100:
                        data.width = (buf[0] >> 5 & 3) + 6;
                        data.height = (buf[1] >> 1 & 3) + 6;
                        data.dual_plane = 0;
                        data.weight_range &= 7;
                        break;
                    case 0x180:
                        data.width = (buf[0] & 0x20) != 0 ? 10 : 6;
                        data.height = (buf[0] & 0x20) != 0 ? 6 : 10;
                        break;
                }
            }

            data.part_num = (buf[1] >> 3 & 3) + 1;
            data.weight_num = data.width * data.height;

            if (data.dual_plane != 0)
            {
                data.weight_num *= 2;
            }

            int cem_base = 0;
            int weight_bits = WeightPrecTableA[data.weight_range] switch
            {
                3 => data.weight_num * WeightPrecTableB[data.weight_range] + (data.weight_num * 8 + 4) / 5,
                5 => data.weight_num * WeightPrecTableB[data.weight_range] + (data.weight_num * 7 + 2) / 3,
                _ => data.weight_num * WeightPrecTableB[data.weight_range],
            };

            int config_bits;
            if (data.part_num == 1)
            {
                data.cem[0] = (buf[1] >> 5) & 0xf;
                config_bits = 17;
            }
            else
            {
                cem_base = (buf[2] >> 7) & 3;
                if (cem_base == 0)
                {
                    int cem = (buf[3] >> 1) & 0xf;
                    for (int i = 0; i < data.part_num; i++)
                    {
                        data.cem[i] = cem;
                    }

                    config_bits = 29;
                }
                else
                {
                    for (int i = 0; i < data.part_num; i++)
                    {
                        data.cem[i] = ((buf[3] >> (i + 1) & 1) + cem_base - 1) << 2;
                    }

                    switch (data.part_num)
                    {
                        case 2:
                            data.cem[0] |= buf[3] >> 3 & 3;
                            data.cem[1] |= GetBits(buf, 126 - weight_bits, 2);
                            break;
                        case 3:
                            data.cem[0] |= buf[3] >> 4 & 1;
                            data.cem[0] |= GetBits(buf, 122 - weight_bits, 2) & 2;
                            data.cem[1] |= GetBits(buf, 124 - weight_bits, 2);
                            data.cem[2] |= GetBits(buf, 126 - weight_bits, 2);
                            break;
                        case 4:
                            for (int i = 0; i < 4; i++)
                                data.cem[i] |= GetBits(buf, 120 + i * 2 - weight_bits, 2);
                            break;
                    }
                    config_bits = 25 + data.part_num * 3;
                }
            }

            if (data.dual_plane != 0)
            {
                config_bits += 2;
                data.plane_selector =
                    GetBits(buf, (cem_base != 0) ? 130 - weight_bits - (data.part_num * 3) : 126 - weight_bits, 2);
            }

            int remain_bits = 128 - config_bits - weight_bits;

            data.endpoint_value_num = 0;

            for (int i = 0; i < data.part_num; i++)
            {
                data.endpoint_value_num += (data.cem[i] >> 1 & 6) + 2;
            }

            for (int i = 0, endpoint_bits; i < CemTableA.Length; i++)
            {
                endpoint_bits = CemTableA[i] switch
                {
                    3 => data.endpoint_value_num * CemTableB[i] + (data.endpoint_value_num * 8 + 4) / 5,
                    5 => data.endpoint_value_num * CemTableB[i] + (data.endpoint_value_num * 7 + 2) / 3,
                    _ => data.endpoint_value_num * CemTableB[i],
                };

                if (endpoint_bits <= remain_bits)
                {
                    data.cem_range = i;
                    break;
                }
            }
        }

        private static void DecodeEndpoints(ReadOnlySpan<byte> buf, ref BlockData data)
        {
            Span<int> TritsTable = stackalloc int[] { 0, 204, 93, 44, 22, 11, 5 };
            Span<int> QuintsTable = stackalloc int[] { 0, 113, 54, 26, 13, 6 };
            Span<IntSeqData> seq = stackalloc IntSeqData[32];
            Span<int> ev = stackalloc int[32];

            DecodeIntSeq(buf, data.part_num == 1 ? 17 : 29, CemTableA[data.cem_range], CemTableB[data.cem_range],
                  data.endpoint_value_num, 0, ref seq);

            switch (CemTableA[data.cem_range])
            {
                case 3:
                    for (int i = 0, b = 0, c = TritsTable[CemTableB[data.cem_range]]; i < data.endpoint_value_num; i++)
                    {
                        int a = (seq[i].bits & 1) * 0x1ff;
                        int x = seq[i].bits >> 1;
                        switch (CemTableB[data.cem_range])
                        {
                            case 1:
                                b = 0;
                                break;
                            case 2:
                                b = 0b100010110 * x;
                                break;
                            case 3:
                                b = x << 7 | x << 2 | x;
                                break;
                            case 4:
                                b = x << 6 | x;
                                break;
                            case 5:
                                b = x << 5 | x >> 2;
                                break;
                            case 6:
                                b = x << 4 | x >> 4;
                                break;
                        }
                        ev[i] = (a & 0x80) | ((seq[i].nonbits * c + b) ^ a) >> 2;
                    }
                    break;
                case 5:
                    for (int i = 0, b = 0, c = QuintsTable[CemTableB[data.cem_range]]; i < data.endpoint_value_num; i++)
                    {
                        int a = (seq[i].bits & 1) * 0x1ff;
                        int x = seq[i].bits >> 1;
                        switch (CemTableB[data.cem_range])
                        {
                            case 1:
                                b = 0;
                                break;
                            case 2:
                                b = 0b100001100 * x;
                                break;
                            case 3:
                                b = x << 7 | x << 1 | x >> 1;
                                break;
                            case 4:
                                b = x << 6 | x >> 1;
                                break;
                            case 5:
                                b = x << 5 | x >> 3;
                                break;
                        }
                        ev[i] = (a & 0x80) | ((seq[i].nonbits * c + b) ^ a) >> 2;
                    }
                    break;
                default:
                    switch (CemTableB[data.cem_range])
                    {
                        case 1:
                            for (int i = 0; i < data.endpoint_value_num; i++)
                                ev[i] = seq[i].bits * 0xff;
                            break;
                        case 2:
                            for (int i = 0; i < data.endpoint_value_num; i++)
                                ev[i] = seq[i].bits * 0x55;
                            break;
                        case 3:
                            for (int i = 0; i < data.endpoint_value_num; i++)
                                ev[i] = seq[i].bits << 5 | seq[i].bits << 2 | seq[i].bits >> 1;
                            break;
                        case 4:
                            for (int i = 0; i < data.endpoint_value_num; i++)
                                ev[i] = seq[i].bits << 4 | seq[i].bits;
                            break;
                        case 5:
                            for (int i = 0; i < data.endpoint_value_num; i++)
                                ev[i] = seq[i].bits << 3 | seq[i].bits >> 2;
                            break;
                        case 6:
                            for (int i = 0; i < data.endpoint_value_num; i++)
                                ev[i] = seq[i].bits << 2 | seq[i].bits >> 4;
                            break;
                        case 7:
                            for (int i = 0; i < data.endpoint_value_num; i++)
                                ev[i] = seq[i].bits << 1 | seq[i].bits >> 6;
                            break;
                        case 8:
                            for (int i = 0; i < data.endpoint_value_num; i++)
                                ev[i] = seq[i].bits;
                            break;
                    }
                    break;
            }

            int vIndex = 0;

            for (int cem = 0; cem < data.part_num; vIndex += (data.cem[cem] / 4 + 1) * 2, cem++)
            {
                Span<int> v = ev[vIndex..];
                switch (data.cem[cem])
                {
                    case 0:
                        SetEndpoint(data.endpoints[cem], v[0], v[0], v[0], 255, v[1], v[1], v[1], 255);
                        break;
                    case 1:
                        {
                            int l0 = (v[0] >> 2) | (v[1] & 0xc0);
                            int l1 = Clamp(l0 + (v[1] & 0x3f));
                            SetEndpoint(data.endpoints[cem], l0, l0, l0, 255, l1, l1, l1, 255);
                            break;
                        }
                    case 2:
                        {
                            int y0, y1;
                            if (v[0] <= v[1])
                            {
                                y0 = v[0] << 4;
                                y1 = v[1] << 4;
                            }
                            else
                            {
                                y0 = (v[1] << 4) + 8;
                                y1 = (v[0] << 4) - 8;
                            }
                            SetEndpointHdr(data.endpoints[cem], y0, y0, y0, 0x780, y1, y1, y1, 0x780);
                            break;
                        }
                    case 3:
                        {
                            int y0, d;
                            if ((v[0] & 0x80) != 0)
                            {
                                y0 = (v[1] & 0xe0) << 4 | (v[0] & 0x7f) << 2;
                                d = (v[1] & 0x1f) << 2;
                            }
                            else
                            {
                                y0 = (v[1] & 0xf0) << 4 | (v[0] & 0x7f) << 1;
                                d = (v[1] & 0x0f) << 1;
                            }
                            int y1 = ClampHdr(y0 + d);
                            SetEndpointHdr(data.endpoints[cem], y0, y0, y0, 0x780, y1, y1, y1, 0x780);
                            break;
                        }
                    case 4:
                        SetEndpoint(data.endpoints[cem], v[0], v[0], v[0], v[2], v[1], v[1], v[1], v[3]);
                        break;
                    case 5:
                        BitTransferSigned(ref v[1], ref v[0]);
                        BitTransferSigned(ref v[3], ref v[2]);
                        v[1] += v[0];
                        SetEndpointClamp(data.endpoints[cem], v[0], v[0], v[0], v[2], v[1], v[1], v[1], v[2] + v[3]);
                        break;
                    case 6:
                        SetEndpoint(data.endpoints[cem], v[0] * v[3] >> 8, v[1] * v[3] >> 8, v[2] * v[3] >> 8, 255, v[0], v[1],
                         v[2], 255);
                        break;
                    case 7:
                        DecodeEndpointsHdr7(data.endpoints[cem], v);
                        break;
                    case 8:
                        if (v[0] + v[2] + v[4] <= v[1] + v[3] + v[5])
                        {
                            SetEndpoint(data.endpoints[cem], v[0], v[2], v[4], 255, v[1], v[3], v[5], 255);
                        }
                        else
                        {
                            SetEndpointBlue(data.endpoints[cem], v[1], v[3], v[5], 255, v[0], v[2], v[4], 255);
                        }

                        break;
                    case 9:
                        BitTransferSigned(ref v[1],ref v[0]);
                        BitTransferSigned(ref v[3], ref v[2]);
                        BitTransferSigned(ref v[5], ref v[4]);
                        if (v[1] + v[3] + v[5] >= 0)
                        {
                            SetEndpointClamp(data.endpoints[cem], v[0], v[2], v[4], 255, v[0] + v[1], v[2] + v[3], v[4] + v[5],
                                               255);
                        }
                        else
                        {
                            SetEndpointBlueClamp(data.endpoints[cem], v[0] + v[1], v[2] + v[3], v[4] + v[5], 255, v[0], v[2],
                                                    v[4], 255);
                        }
                        break;
                    case 10:
                        SetEndpoint(data.endpoints[cem], v[0] * v[3] >> 8, v[1] * v[3] >> 8, v[2] * v[3] >> 8, v[4], v[0], v[1],
                                     v[2], v[5]);
                        break;
                    case 11:
                        DecodeEndpointsHdr11(data.endpoints[cem], v, 0x780, 0x780);
                        break;
                    case 12:
                        if (v[0] + v[2] + v[4] <= v[1] + v[3] + v[5])
                            SetEndpoint(data.endpoints[cem], v[0], v[2], v[4], v[6], v[1], v[3], v[5], v[7]);
                        else
                            SetEndpointBlue(data.endpoints[cem], v[1], v[3], v[5], v[7], v[0], v[2], v[4], v[6]);
                        break;
                    case 13:
                        BitTransferSigned(ref v[1], ref v[0]);
                        BitTransferSigned(ref v[3], ref v[2]);
                        BitTransferSigned(ref v[5], ref v[4]);
                        BitTransferSigned(ref v[7], ref v[6]);
                        if (v[1] + v[3] + v[5] >= 0)
                            SetEndpointClamp(data.endpoints[cem], v[0], v[2], v[4], v[6], v[0] + v[1], v[2] + v[3], v[4] + v[5],
                                               v[6] + v[7]);
                        else
                            SetEndpointBlueClamp(data.endpoints[cem], v[0] + v[1], v[2] + v[3], v[4] + v[5], v[6] + v[7], v[0],
                                                    v[2], v[4], v[6]);
                        break;
                    case 14:
                        DecodeEndpointsHdr11(data.endpoints[cem], v, v[6], v[7]);
                        break;
                    case 15:
                        {
                            int mode = ((v[6] >> 7) & 1) | ((v[7] >> 6) & 2);
                            v[6] &= 0x7f;
                            v[7] &= 0x7f;
                            if (mode == 3)
                            {
                                DecodeEndpointsHdr11(data.endpoints[cem], v, v[6] << 5, v[7] << 5);
                            }
                            else
                            {
                                v[6] |= (v[7] << (mode + 1)) & 0x780;
                                v[7] = ((v[7] & (0x3f >> mode)) ^ (0x20 >> mode)) - (0x20 >> mode);
                                v[6] <<= 4 - mode;
                                v[7] <<= 4 - mode;
                                DecodeEndpointsHdr11(data.endpoints[cem], v, v[6], ClampHdr(v[6] + v[7]));
                            }
                        }
                        break;
                }
            }
        }

        private static void DecodeWeights(ReadOnlySpan<byte> buf, ref BlockData data)
        {
            Span<IntSeqData> seq = stackalloc IntSeqData[128];
            Span<int> wv = stackalloc int[128];
            DecodeIntSeq(buf, 128, WeightPrecTableA[data.weight_range], WeightPrecTableB[data.weight_range],
                  data.weight_num, 1,ref seq);
            if (WeightPrecTableA[data.weight_range] == 0)
            {
                switch (WeightPrecTableB[data.weight_range])
                {
                    case 1:
                        for (int i = 0; i < data.weight_num; i++)
                            wv[i] = seq[i].bits != 0 ? 63 : 0;
                        break;
                    case 2:
                        for (int i = 0; i < data.weight_num; i++)
                            wv[i] = seq[i].bits << 4 | seq[i].bits << 2 | seq[i].bits;
                        break;
                    case 3:
                        for (int i = 0; i < data.weight_num; i++)
                            wv[i] = seq[i].bits << 3 | seq[i].bits;
                        break;
                    case 4:
                        for (int i = 0; i < data.weight_num; i++)
                            wv[i] = seq[i].bits << 2 | seq[i].bits >> 2;
                        break;
                    case 5:
                        for (int i = 0; i < data.weight_num; i++)
                            wv[i] = seq[i].bits << 1 | seq[i].bits >> 4;
                        break;
                }
                for (int i = 0; i < data.weight_num; i++)
                    if (wv[i] > 32)
                        ++wv[i];
            }
            else if (WeightPrecTableB[data.weight_range] == 0)
            {
                int s = WeightPrecTableA[data.weight_range] == 3 ? 32 : 16;
                for (int i = 0; i < data.weight_num; i++)
                    wv[i] = seq[i].nonbits * s;
            }
            else
            {
                if (WeightPrecTableA[data.weight_range] == 3)
                {
                    switch (WeightPrecTableB[data.weight_range])
                    {
                        case 1:
                            for (int i = 0; i < data.weight_num; i++)
                                wv[i] = seq[i].nonbits * 50;
                            break;
                        case 2:
                            for (int i = 0; i < data.weight_num; i++)
                            {
                                wv[i] = seq[i].nonbits * 23;
                                if ((seq[i].bits & 2) != 0)
                                    wv[i] += 0b1000101;
                            }
                            break;
                        case 3:
                            for (int i = 0; i < data.weight_num; i++)
                                wv[i] = seq[i].nonbits * 11 + ((seq[i].bits << 4 | seq[i].bits >> 1) & 0b1100011);
                            break;
                    }
                }
                else if (WeightPrecTableA[data.weight_range] == 5)
                {
                    switch (WeightPrecTableB[data.weight_range])
                    {
                        case 1:
                            for (int i = 0; i < data.weight_num; i++)
                                wv[i] = seq[i].nonbits * 28;
                            break;
                        case 2:
                            for (int i = 0; i < data.weight_num; i++)
                            {
                                wv[i] = seq[i].nonbits * 13;
                                if ((seq[i].bits & 2) != 0)
                                    wv[i] += 0b1000010;
                            }
                            break;
                    }
                }
                for (int i = 0; i < data.weight_num; i++)
                {
                    int a = (seq[i].bits & 1) * 0x7f;
                    wv[i] = (a & 0x20) | ((wv[i] ^ a) >> 2);
                    if (wv[i] > 32)
                        ++wv[i];
                }
            }

            int ds = (1024 + data.bw / 2) / (data.bw - 1);
            int dt = (1024 + data.bh / 2) / (data.bh - 1);
            int pn = data.dual_plane != 0 ? 2 : 1;

            for (int t = 0, i = 0; t < data.bh; t++)
            {
                for (int s = 0; s < data.bw; s++, i++)
                {
                    int gs = (ds * s * (data.width - 1) + 32) >> 6;
                    int gt = (dt * t * (data.height - 1) + 32) >> 6;
                    int fs = gs & 0xf;
                    int ft = gt & 0xf;
                    int v = (gs >> 4) + (gt >> 4) * data.width;
                    int w11 = (fs * ft + 8) >> 4;
                    int w10 = ft - w11;
                    int w01 = fs - w11;
                    int w00 = 16 - fs - ft + w11;

                    for (int p = 0; p < pn; p++)
                    {
                        int p00 = wv[v * pn + p];
                        int p01 = wv[(v + 1) * pn + p];
                        int p10 = wv[(v + data.width) * pn + p];
                        int p11 = wv[(v + data.width + 1) * pn + p];
                        data.weights[i][p] = (p00 * w00 + p01 * w01 + p10 * w10 + p11 * w11 + 8) >> 4;
                    }
                }
            }
        }

        private static unsafe void DecodeIntSeq(ReadOnlySpan<byte> buf, int offset, int a, int b , int count, int reverse,
                   ref Span<IntSeqData> result)
        {
            Span<int> mt = stackalloc int[] { 0, 2, 4, 5, 7 };
            Span<int> mq = stackalloc int[] { 0, 3, 5 };

            if (count <= 0)
            {
                return;
            }

            int n = 0;

            if (a == 3)
            {
                int mask = (1 << b) - 1;
                int block_count = (count + 4) / 5;
                int last_block_count = (count + 4) % 5 + 1;
                int block_size = 8 + 5 * b;
                int last_block_size = (block_size * last_block_count + 4) / 5;

                if (reverse != 0)
                {
                    for (int i = 0, p = offset; i < block_count; i++, p -= block_size)
                    {
                        int now_size = (i < block_count - 1) ? block_size : last_block_size;
                        ulong d = BitReverseUInt64(GetBits64(buf, p - now_size, now_size), now_size);
                        int x =
                          (int)((d >> b & 3) | (d >> b * 2 & 0xc) | (d >> b * 3 & 0x10) | (d >> b * 4 & 0x60) | (d >> b * 5 & 0x80));
                        for (int j = 0; j < 5 && n < count; j++, n++)
                        {
                            IntSeqData data = new IntSeqData
                            {
                                bits = (int)(d >> (mt[j] + b * j)) & mask,
                                nonbits = TritsTable[j, x]
                            };
                            result[n] = data;
                        }
                    }
                }
                else
                {
                    for (int i = 0, p = offset; i < block_count; i++, p += block_size)
                    {
                        ulong d = GetBits64(buf, p, (i < block_count - 1) ? block_size : last_block_size);
                        int x =
                          (int)((d >> b & 3) | (d >> b * 2 & 0xc) | (d >> b * 3 & 0x10) | (d >> b * 4 & 0x60) | (d >> b * 5 & 0x80));
                        for (int j = 0; j < 5 && n < count; j++, n++)
                        {
                            IntSeqData data = new IntSeqData
                            {
                                bits = (int)(d >> (mt[j] + b * j)) & mask,
                                nonbits = TritsTable[j, x]
                            };
                            result[n] = data;
                        }
                    }
                }
            }
            else if (a == 5)
            {
                int mask = (1 << b) - 1;
                int block_count = (count + 2) / 3;
                int last_block_count = (count + 2) % 3 + 1;
                int block_size = 7 + 3 * b;
                int last_block_size = (block_size * last_block_count + 2) / 3;

                if (reverse != 0)
                {
                    for (int i = 0, p = offset; i < block_count; i++, p -= block_size)
                    {
                        int now_size = (i < block_count - 1) ? block_size : last_block_size;
                        ulong d = BitReverseUInt64(GetBits64(buf, p - now_size, now_size), now_size);
                        int x = (int)((d >> b & 7) | (d >> b * 2 & 0x18) | (d >> b * 3 & 0x60));
                        for (int j = 0; j < 3 && n < count; j++, n++)
                        {
                            IntSeqData data = new IntSeqData()
                            {
                                bits = (int)(d >> (mq[j] + b * j)) & mask,
                                nonbits = QuintsTable[j, x]
                            };
                            result[n] = data;
                        }
                    }
                }
                else
                {
                    for (int i = 0, p = offset; i < block_count; i++, p += block_size)
                    {
                        ulong d = GetBits64(buf, p, (i < block_count - 1) ? block_size : last_block_size);
                        int x = (int)((d >> b & 7) | (d >> b * 2 & 0x18) | (d >> b * 3 & 0x60));
                        for (int j = 0; j < 3 && n < count; j++, n++)
                        {
                            IntSeqData data = new()
                            {
                                bits = (int)(d >> (mq[j] + b * j)) & mask,
                                nonbits = QuintsTable[j, x]
                            };
                            result[n] = data;
                        }
                    }
                }
            }
            else
            {
                if (reverse != 0)
                {
                    for (int p = offset - b; n < count; n++, p -= b)
                    {
                        IntSeqData data = new()
                        {
                            bits = BitReverseByte((byte)GetBits(buf, p, b), b),
                            nonbits = 0
                        };
                        result[n] = data;
                    }
                }
                else
                {
                    for (int p = offset; n < count; n++, p += b)
                    {
                        IntSeqData data = new()
                        {
                            bits = GetBits(buf, p, b),
                            nonbits = 0
                        };

                        result[n] = data;
                    }
                }
            }
        }

        private static void DecodeEndpointsHdr11(int[] endpoints, Span<int> v, int alpha1, int alpha2)
        {
            int major_component = (v[4] >> 7) | (v[5] >> 6 & 2);
            if (major_component == 3)
            {
                SetEndpointHdr(endpoints, v[0] << 4, v[2] << 4, v[4] << 5 & 0xfe0, alpha1, v[1] << 4, v[3] << 4,
                                 v[5] << 5 & 0xfe0, alpha2);
                return;
            }
            int mode = (v[1] >> 7) | (v[2] >> 6 & 2) | (v[3] >> 5 & 4);
            int va = v[0] | (v[1] << 2 & 0x100);
            int vb0 = v[2] & 0x3f, vb1 = v[3] & 0x3f;
            int vc = v[1] & 0x3f;
            int vd0, vd1;

            switch (mode)
            {
                case 0:
                case 2:
                    vd0 = v[4] & 0x7f;
                    if ((vd0 & 0x40) != 0)
                        vd0 |= 0xff80;
                    vd1 = v[5] & 0x7f;
                    if ((vd1 & 0x40) != 0)
                        vd1 |= 0xff80;
                    break;
                case 1:
                case 3:
                case 5:
                case 7:
                    vd0 = v[4] & 0x3f;
                    if ((vd0 & 0x20) != 0)
                        vd0 |= 0xffc0;
                    vd1 = v[5] & 0x3f;
                    if ((vd1 & 0x20) != 0)
                        vd1 |= 0xffc0;
                    break;
                default:
                    vd0 = v[4] & 0x1f;
                    if ((vd0 & 0x10) != 0)
                        vd0 |= 0xffe0;
                    vd1 = v[5] & 0x1f;
                    if ((vd1 & 0x10) != 0)
                        vd1 |= 0xffe0;
                    break;
            }

            switch (mode)
            {
                case 0:
                    vb0 |= v[2] & 0x40;
                    vb1 |= v[3] & 0x40;
                    break;
                case 1:
                    vb0 |= v[2] & 0x40;
                    vb1 |= v[3] & 0x40;
                    vb0 |= v[4] << 1 & 0x80;
                    vb1 |= v[5] << 1 & 0x80;
                    break;
                case 2:
                    va |= v[2] << 3 & 0x200;
                    vc |= v[3] & 0x40;
                    break;
                case 3:
                    va |= v[4] << 3 & 0x200;
                    vc |= v[5] & 0x40;
                    vb0 |= v[2] & 0x40;
                    vb1 |= v[3] & 0x40;
                    break;
                case 4:
                    va |= v[4] << 4 & 0x200;
                    va |= v[5] << 5 & 0x400;
                    vb0 |= v[2] & 0x40;
                    vb1 |= v[3] & 0x40;
                    vb0 |= v[4] << 1 & 0x80;
                    vb1 |= v[5] << 1 & 0x80;
                    break;
                case 5:
                    va |= v[2] << 3 & 0x200;
                    va |= v[3] << 4 & 0x400;
                    vc |= v[5] & 0x40;
                    vc |= v[4] << 1 & 0x80;
                    break;
                case 6:
                    va |= v[4] << 4 & 0x200;
                    va |= v[5] << 5 & 0x400;
                    va |= v[4] << 5 & 0x800;
                    vc |= v[5] & 0x40;
                    vb0 |= v[2] & 0x40;
                    vb1 |= v[3] & 0x40;
                    break;
                case 7:
                    va |= v[2] << 3 & 0x200;
                    va |= v[3] << 4 & 0x400;
                    va |= v[4] << 5 & 0x800;
                    vc |= v[5] & 0x40;
                    break;
            }

            int shamt = (mode >> 1) ^ 3;
            va <<= shamt;
            vb0 <<= shamt;
            vb1 <<= shamt;
            vc <<= shamt;
            int mult = 1 << shamt;
            vd0 *= mult;
            vd1 *= mult;

            if (major_component == 1)
                SetEndpointHdrClamp(endpoints, va - vb0 - vc - vd0, va - vc, va - vb1 - vc - vd1, alpha1, va - vb0, va,
                                       va - vb1, alpha2);
            else if (major_component == 2)
                SetEndpointHdrClamp(endpoints, va - vb1 - vc - vd1, va - vb0 - vc - vd0, va - vc, alpha1, va - vb1, va - vb0,
                                       va, alpha2);
            else
                SetEndpointHdrClamp(endpoints, va - vc, va - vb0 - vc - vd0, va - vb1 - vc - vd1, alpha1, va, va - vb0,
                                       va - vb1, alpha2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetEndpointBlueClamp(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2,
                                           int a2)
        {
            endpoint[0] = Clamp((r1 + b1) >> 1);
            endpoint[1] = Clamp((g1 + b1) >> 1);
            endpoint[2] = Clamp(b1);
            endpoint[3] = Clamp(a1);
            endpoint[4] = Clamp((r2 + b2) >> 1);
            endpoint[5] = Clamp((g2 + b2) >> 1);
            endpoint[6] = Clamp(b2);
            endpoint[7] = Clamp(a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetEndpointBlue(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
        {
            endpoint[0] = (r1 + b1) >> 1;
            endpoint[1] = (g1 + b1) >> 1;
            endpoint[2] = b1;
            endpoint[3] = a1;
            endpoint[4] = (r2 + b2) >> 1;
            endpoint[5] = (g2 + b2) >> 1;
            endpoint[6] = b2;
            endpoint[7] = a2;
        }

        private static void DecodeEndpointsHdr7(int[] endpoints, ReadOnlySpan<int> v)
        {
            int modeval = (v[2] >> 4 & 0x8) | (v[1] >> 5 & 0x4) | (v[0] >> 6);
            int major_component, mode;
            if ((modeval & 0xc) != 0xc)
            {
                major_component = modeval >> 2;
                mode = modeval & 3;
            }
            else if (modeval != 0xf)
            {
                major_component = modeval & 3;
                mode = 4;
            }
            else
            {
                major_component = 0;
                mode = 5;
            }
            int[] c = { v[0] & 0x3f, v[1] & 0x1f, v[2] & 0x1f, v[3] & 0x1f };

            switch (mode)
            {
                case 0:
                    c[3] |= v[3] & 0x60;
                    c[0] |= v[3] >> 1 & 0x40;
                    c[0] |= v[2] << 1 & 0x80;
                    c[0] |= v[1] << 3 & 0x300;
                    c[0] |= v[2] << 5 & 0x400;
                    c[0] <<= 1;
                    c[1] <<= 1;
                    c[2] <<= 1;
                    c[3] <<= 1;
                    break;
                case 1:
                    c[1] |= v[1] & 0x20;
                    c[2] |= v[2] & 0x20;
                    c[0] |= v[3] >> 1 & 0x40;
                    c[0] |= v[2] << 1 & 0x80;
                    c[0] |= v[1] << 2 & 0x100;
                    c[0] |= v[3] << 4 & 0x600;
                    c[0] <<= 1;
                    c[1] <<= 1;
                    c[2] <<= 1;
                    c[3] <<= 1;
                    break;
                case 2:
                    c[3] |= v[3] & 0xe0;
                    c[0] |= v[2] << 1 & 0xc0;
                    c[0] |= v[1] << 3 & 0x300;
                    c[0] <<= 2;
                    c[1] <<= 2;
                    c[2] <<= 2;
                    c[3] <<= 2;
                    break;
                case 3:
                    c[1] |= v[1] & 0x20;
                    c[2] |= v[2] & 0x20;
                    c[3] |= v[3] & 0x60;
                    c[0] |= v[3] >> 1 & 0x40;
                    c[0] |= v[2] << 1 & 0x80;
                    c[0] |= v[1] << 2 & 0x100;
                    c[0] <<= 3;
                    c[1] <<= 3;
                    c[2] <<= 3;
                    c[3] <<= 3;
                    break;
                case 4:
                    c[1] |= v[1] & 0x60;
                    c[2] |= v[2] & 0x60;
                    c[3] |= v[3] & 0x20;
                    c[0] |= v[3] >> 1 & 0x40;
                    c[0] |= v[3] << 1 & 0x80;
                    c[0] <<= 4;
                    c[1] <<= 4;
                    c[2] <<= 4;
                    c[3] <<= 4;
                    break;
                case 5:
                    c[1] |= v[1] & 0x60;
                    c[2] |= v[2] & 0x60;
                    c[3] |= v[3] & 0x60;
                    c[0] |= v[3] >> 1 & 0x40;
                    c[0] <<= 5;
                    c[1] <<= 5;
                    c[2] <<= 5;
                    c[3] <<= 5;
                    break;
            }
            if (mode != 5)
            {
                c[1] = c[0] - c[1];
                c[2] = c[0] - c[2];
            }

            switch (major_component)
            {
                case 1:
                    SetEndpointHdrClamp(endpoints, c[1] - c[3], c[0] - c[3], c[2] - c[3], 0x780, c[1], c[0], c[2], 0x780);
                    break;
                case 2:
                    SetEndpointHdrClamp(endpoints, c[2] - c[3], c[1] - c[3], c[0] - c[3], 0x780, c[2], c[1], c[0], 0x780);
                    break;
                default:
                    SetEndpointHdrClamp(endpoints, c[0] - c[3], c[1] - c[3], c[2] - c[3], 0x780, c[0], c[1], c[2], 0x780);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetEndpointHdrClamp(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2,
                                          int a2)
        {
            endpoint[0] = ClampHdr(r1);
            endpoint[1] = ClampHdr(g1);
            endpoint[2] = ClampHdr(b1);
            endpoint[3] = ClampHdr(a1);
            endpoint[4] = ClampHdr(r2);
            endpoint[5] = ClampHdr(g2);
            endpoint[6] = ClampHdr(b2);
            endpoint[7] = ClampHdr(a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetEndpoint(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
        {
            endpoint[0] = r1;
            endpoint[1] = g1;
            endpoint[2] = b1;
            endpoint[3] = a1;
            endpoint[4] = r2;
            endpoint[5] = g2;
            endpoint[6] = b2;
            endpoint[7] = a2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetEndpointHdr(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
        {
            endpoint[0] = r1;
            endpoint[1] = g1;
            endpoint[2] = b1;
            endpoint[3] = a1;
            endpoint[4] = r2;
            endpoint[5] = g2;
            endpoint[6] = b2;
            endpoint[7] = a2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetEndpointClamp(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
        {
            endpoint[0] = Clamp(r1);
            endpoint[1] = Clamp(g1);
            endpoint[2] = Clamp(b1);
            endpoint[3] = Clamp(a1);
            endpoint[4] = Clamp(r2);
            endpoint[5] = Clamp(g2);
            endpoint[6] = Clamp(b2);
            endpoint[7] = Clamp(a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BitTransferSigned(ref int a, ref int b)
        {
            b = (b >> 1) | (a & 0x80);
            a = (a >> 1) & 0x3f;
            if ((a & 0x20) != 0)
            {
                a -= 0x40;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Clamp(int n)
        {
            return (byte)(n < 0 ? 0 : n > 255 ? 255 : n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampHdr(int n)
        {
            return n < 0 ? 0 : n > 0xfff ? 0xfff : n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong BitReverseUInt64(ulong d, int bits)
        {
            ulong ret = (ulong)BitReverseTable[d & 0xff] << 56
                        | (ulong)BitReverseTable[d >> 8 & 0xff] << 48
                        | (ulong)BitReverseTable[d >> 16 & 0xff] << 40
                        | (ulong)BitReverseTable[d >> 24 & 0xff] << 32
                        | (ulong)BitReverseTable[d >> 32 & 0xff] << 24
                        | (ulong)BitReverseTable[d >> 40 & 0xff] << 16
                        | (ulong)BitReverseTable[d >> 48 & 0xff] << 8
                        | (ushort)BitReverseTable[d >> 56 & 0xff];
            return ret >> (64 - bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte BitReverseByte(byte c, int bits)
        {
            return (byte)(BitReverseTable[c] >> (8 - bits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBits(ReadOnlySpan<byte> bytes, int bit, int length)
        {
            return (bytes[bit / 8] >> (bit % 8)) & ((1 << length) - 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong GetBits64(ReadOnlySpan<byte> bytes, int bit, int length)
        {
            ulong mask = length == 64 ? 0xffffffffffffffff : (ulong)(1 << length) -1;
            if (length < 1)
            {
                return 0;
            }
            else if (bit >= 64)
            {
                return (ulong)(bytes[8] >> (bit - 64)) & mask;
            }
            else if (bit <= 0)
            {
                return ((ulong)bytes[0] << -bit) & mask;
            }
            else if (bit + length <= 64)
            {
                return ((ulong)bytes[0] >> bit) & mask;
            }
            else
            {
                return ((((ulong)bytes[0]) >> bit) | ((ulong)bytes[8] << (64 - bit))) & mask;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Color(byte r, byte g, byte b, byte a)
        {
            return (uint)(b | (g << 8) | (r << 16) | (a << 24));
        }

        private ref struct BlockData
        {
            public int bw = default;
            public int bh = default;
            public int width = default;
            public int height = default;
            public int part_num = default;
            public int dual_plane = default;
            public int plane_selector = default;
            public int weight_range = default;
            public int weight_num = default;
            //int cem[4];
            public int[] cem = new int[4];
            public int cem_range = default;
            public int endpoint_value_num = default;
            //int endpoints[4][8];
            //int weights[144][2];
            //int partition[144];
            public int[][] endpoints;
            public int[][] weights;
            public int[] partition = new int[144];

            public BlockData()
            {
                endpoints = Enumerable.Repeat(new int[8], 4).ToArray(); ;
                weights = Enumerable.Repeat(new int[2], 144).ToArray();
            }
        }

        private struct IntSeqData
        {
            public int bits;
            public int nonbits;
        }
    }
}
