using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 一个处理图片的类
    /// </summary>
    public static class ImageHelper
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

        /// <summary>
        /// 处理两张图片(一张RGB,一张Alpha),并将它们合并为一张具有透明度的图片
        /// </summary>
        /// <param name="rgb">包含图片RGB通道信息的<seealso cref="Image{Bgra32}"/></param>
        /// <param name="alpha">包含图片Alpha通道信息的<seealso cref="Image{Bgra32}"/></param>
        /// <returns>包含图片信息的<seealso cref="byte"/>数组</returns>
        public static byte[] ProcessImage(Image<Bgra32> rgb, Image<Bgra32> alpha)
        {
            HandleImages(rgb, alpha);
            var stream = new MemoryStream();
            rgb.SaveAsPng(stream);

            rgb.Dispose();
            alpha.Dispose();

            return stream.ToArray();
        }

        /// <summary>
        /// 处理两张图片(一张RGB,一张Alpha),并将它们合并为一张具有透明度的图片(以<seealso cref="Image{Bgra32}"/>对象返回)
        /// </summary>
        /// <param name="rgb">包含图片RGB通道信息的<seealso cref="Image{Bgra32}"/></param>
        /// <param name="alpha">包含图片Alpha通道信息的<seealso cref="Image{Bgra32}"/></param>
        /// <returns>包含图片信息的<seealso cref="Image{Bgra32}"/>对象</returns>
        public static Image<Bgra32> ProcessImageReturnImage(Image<Bgra32> rgb, Image<Bgra32> alpha)
        {
            HandleImages(rgb, alpha);
            alpha.Dispose();
            return rgb;
        }

        //TODO: Find a better way to process image
        private static void HandleImages(Image<Bgra32> rgb, Image<Bgra32> alpha)
        {
            //Bad implementation
            Bgra32 transparent = SixLabors.ImageSharp.Color.Transparent;
            alpha.Mutate(x => x.Resize(rgb.Width, rgb.Height));
            alpha.ProcessPixelRows(accessorA =>
            {
                Bgra32 black = new Bgra32(0, 0, 0, 255);
                for (int yA = 0; yA < accessorA.Height; yA++)
                {
                    Span<Bgra32> pixelRowA = accessorA.GetRowSpan(yA);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int x = 0; x < pixelRowA.Length; x++)
                    {
                        ref Bgra32 pixel = ref pixelRowA[x];
                        if (pixel == black)
                        {
                            pixel = transparent;
                        }
                    }
                }
            });

            rgb.ProcessPixelRows(accessor =>
            {

                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Bgra32> pixelRow = accessor.GetRowSpan(y);

                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        ref Bgra32 pixel = ref pixelRow[x];

                        Bgra32 alphaPixel = alpha[x, y];
                        if (alphaPixel == transparent || (alphaPixel.B < 60 && alphaPixel.R < 60 && alphaPixel.G < 60))
                        {
                            pixel = transparent;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 解码ETC1图片
        /// </summary>
        /// <param name="originData">包含图片原始数据的数组</param>
        /// <param name="w">图片的宽度</param>
        /// <param name="h">图片的高度</param>
        /// <returns>包含解码图片的<seealso cref="byte"/>数组</returns>
        public static byte[] DecodeETC1(byte[] originData, int w, int h)
        {
            var imageData = InternalArrayPools.ByteArrayPool.Rent(w * h * 4);

            int num_blocks_x = (w + 3) / 4;
            int num_blocks_y = (h + 3) / 4;
            int[] buffer = InternalArrayPools.Int32ArrayPool.Rent(16);
            Span<byte> originDataSpan = originData.AsSpan();
            int index = 0;
            for (int by = 0; by < num_blocks_y; by++)
            {
                for (int bx = 0; bx < num_blocks_x; bx++, index += 8)
                {
                    Span<byte> slice = originDataSpan.Slice(index, 8);
                    DecodeETC1Block(slice, buffer);

                    CopyBlockBuffer(bx, by, w, h, 4, 4, buffer, imageData);
                }
            }

            return imageData;
        }

        private static void DecodeETC1Block(Span<byte> data, int[] buffer)
        {
            byte[] code = new byte[] { (byte)(data[3] >> 5), (byte)(data[3] >> 2 & 7) };  // Table codewords
            byte[] table = Etc1SubblockTable[data[3] & 1];

            byte[][] c = InternalArrayPools.ByteArrayArrayPool.Rent(2);
            c[0] = new byte[3];
            c[1] = new byte[3];

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
            InternalArrayPools.ByteArrayArrayPool.Return(c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CopyBlockBuffer(int bx, int by, int w, int h, int bw, int bh, int[] buffer, byte[] imageData)
        {
            Span<int> buf = buffer.AsSpan();
            int x = bw * bx;
            int xl = (bw * (bx + 1) > w ? w - (bw * bx) : bw) * 4;
            int index = 0;
            for (int y = by * bh; index < buf.Length && y < h; index += bw, y++)
            {
                var slice = buf.Slice(index, bw);
                int[] sliceArray = InternalArrayPools.Int32ArrayPool.Rent(slice.Length);
                for (int i = 0; i < slice.Length; i++)
                {
                    sliceArray[i] = slice[i];
                }
                Buffer.BlockCopy(sliceArray, 0, imageData, (y * w + x) * 4, xl);
                InternalArrayPools.Int32ArrayPool.Return(sliceArray);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ApplicateColor(byte[] c, int m)
        {
            return Color(Clamp(c[0] + m), Clamp(c[1] + m), Clamp(c[2] + m), 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Clamp(int n)
        {
            return (byte)(n < 0 ? 0 : n > 255 ? 255 : n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Color(byte r, byte g, byte b, byte a)
        {
            return b | (g << 8) | (r << 16) | (a << 24);
        }
    }
}
