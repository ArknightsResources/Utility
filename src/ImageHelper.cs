#if NET7_0_OR_GREATER
#pragma warning disable IDE0230
#endif

#if NET6_0_OR_GREATER
#pragma warning disable IDE0090
#pragma warning disable IDE0063
#endif

using ArknightsResources.Utility.Decoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 一个处理图片的类
    /// </summary>
    public static class ImageHelper
    {
        // 本类参考了AssetStudio项目来解码图片
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

        /// <summary>
        /// 处理两张图片(一张RGB,一张Alpha),并将它们合并为一张具有透明度的图片
        /// </summary>
        /// <param name="rgb">包含图片RGB通道信息的<seealso cref="Image{Bgra32}"/></param>
        /// <param name="alpha">包含图片Alpha通道信息的<seealso cref="Image{Bgra32}"/></param>
        /// <returns>包含图片信息的<seealso cref="byte"/>数组</returns>
        public static byte[] ProcessImage(Image<Bgra32> rgb, Image<Bgra32> alpha)
        {
            MergeImages(rgb, alpha);
            using (MemoryStream stream = new MemoryStream(rgb.Height * rgb.Width * 4))
            {
                rgb.SaveAsPng(stream);

                rgb.Dispose();
                alpha.Dispose();

                return stream.ToArray();
            }
        }

        /// <summary>
        /// 处理两张图片(一张RGB,一张Alpha),并将它们合并为一张具有透明度的图片
        /// </summary>
        /// <param name="rgb">包含图片RGB通道信息的<seealso cref="Image{Bgra32}"/></param>
        /// <param name="alpha">包含图片Alpha通道信息的<seealso cref="Image{Bgra32}"/></param>
        /// <returns>包含图片信息的<see cref="Image"/></returns>
        public static Image<Bgra32> ProcessImageReturnImage(Image<Bgra32> rgb, Image<Bgra32> alpha)
        {
            MergeImages(rgb, alpha);
            alpha.Dispose();
            return rgb;
        }

        private static void MergeImages(Image<Bgra32> rgb, Image<Bgra32> alpha)
        {
            alpha.Mutate(x => x.Resize(rgb.Width, rgb.Height));

            rgb.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Bgra32> pixelRow = accessor.GetRowSpan(y);

                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        ref Bgra32 pixel = ref pixelRow[x];

                        Bgra32 alphaPixel = alpha[x, y];
                        pixel.A = alphaPixel.R;
                    }
                }
            });
        }

        /// <summary>
        /// 解码 ETC1 图片
        /// </summary>
        /// <param name="originData">包含图片原始数据的Span&lt;byte&gt;</param>
        /// <param name="w">图片的宽度</param>
        /// <param name="h">图片的高度</param>
        /// <returns>包含解码图片的 byte 数组</returns>
        public static unsafe byte[] DecodeETC1(ReadOnlySpan<byte> originData, int w, int h)
        {
            byte[] imageData = new byte[w * h * 4];

            int num_blocks_x = (w + 3) / 4;
            int num_blocks_y = (h + 3) / 4;
            Span<uint> buffer = stackalloc uint[16];

            int index = 0;
            for (int by = 0; by < num_blocks_y; by++)
            {
                for (int bx = 0; bx < num_blocks_x; bx++, index += 8)
                {
                    ReadOnlySpan<byte> slice = originData.Slice(index, 8);
                    ETC1Decoder.DecodeETC1Block(slice, buffer);
                    CopyBlockBuffer(bx, by, w, h, 4, 4, buffer, imageData);
                }
            }
            return imageData;
        }

        /// <summary>
        /// 解码 ETC1 图片
        /// </summary>
        /// <param name="originData">包含图片原始数据的Span&lt;byte&gt;</param>
        /// <param name="imageData">一个指向byte数组的指针,其大小应为w * h * 4,方法返回后该数组将被填充解码后图片数据</param>
        /// <param name="w">图片的宽度</param>
        /// <param name="h">图片的高度</param>
        public static unsafe void DecodeETC1(ReadOnlySpan<byte> originData, void* imageData, int w, int h)
        {
            int num_blocks_x = (w + 3) / 4;
            int num_blocks_y = (h + 3) / 4;
            Span<uint> buffer = stackalloc uint[16];

            int index = 0;
            for (int by = 0; by < num_blocks_y; by++)
            {
                for (int bx = 0; bx < num_blocks_x; bx++, index += 8)
                {
                    ReadOnlySpan<byte> slice = originData.Slice(index, 8);
                    ETC1Decoder.DecodeETC1Block(slice, buffer);
                    CopyBlockBuffer(bx, by, w, h, 4, 4, buffer, imageData);
                }
            }
        }

        /// <summary>
        /// 解码 ASTC 图像
        /// </summary>
        /// <param name="originData">包含图片原始数据的Span&lt;byte&gt;</param>
        /// <param name="imageData">一个指向byte数组的指针,其大小应为w * h * 4,方法返回后该数组将被填充解码后图片数据</param>
        /// <param name="w">图片的宽度</param>
        /// <param name="h">图片的高度</param>
        /// <param name="blockSize">图像块大小</param>
        public static unsafe void DecodeASTC(ReadOnlySpan<byte> originData, void* imageData, int w, int h, int blockSize)
        {
            int num_blocks_x = (w + blockSize - 1) / blockSize;
            int num_blocks_y = (h + blockSize - 1) / blockSize;
            Span<uint> buffer = stackalloc uint[114];
            int index = 0;
            for (int by = 0; by < num_blocks_y; by++)
            {
                for (int bx = 0; bx < num_blocks_x; bx++, index += 16)
                {
                    ReadOnlySpan<byte> slice = originData.Slice(index, 16);
                    ASTCDecoder.DecodeASTCBlock(slice, buffer, blockSize);
                    CopyBlockBuffer(bx, by, w, h, blockSize, blockSize, buffer, imageData);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CopyBlockBuffer(int bx, int by, int w, int h, int bw, int bh, ReadOnlySpan<uint> buf, byte[] imageData)
        {
            int x = bw * bx;
            int xl;

            if (bw * (bx + 1) > w)
            {
                xl = (w - (bw * bx)) * 4;
            }
            else
            {
                xl = (bw) * 4;
            }

            int index = 0;
            for (int y = by * bh; index < buf.Length && y < h; index += bw, y++)
            {
                ReadOnlySpan<uint> slice = buf.Slice(index, bw);
                RuntimeHelpers.EnsureSufficientExecutionStack();
#pragma warning disable CA2014
                int* slicePtr = stackalloc int[slice.Length];
#pragma warning restore CA2014
                IntPtr ptr = new IntPtr(slicePtr);
                Span<uint> sliceSpan = new Span<uint>(slicePtr, slice.Length);
                slice.CopyTo(sliceSpan);
                Marshal.Copy(ptr, imageData, (y * w + x) * 4, xl);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CopyBlockBuffer(int bx, int by, int w, int h, int bw, int bh, ReadOnlySpan<uint> buf, void* imageData)
        {
            int x = bw * bx;
            int xl;

            if (bw * (bx + 1) > w)
            {
                xl = (w - (bw * bx)) * 4;
            }
            else
            {
                xl = (bw) * 4;
            }

            int index = 0;
            for (int y = by * bh; index < buf.Length && y < h; index += bw, y++)
            {
                ReadOnlySpan<uint> slice = buf.Slice(index, bw);
                RuntimeHelpers.EnsureSufficientExecutionStack();
#pragma warning disable CA2014
                int* slicePtr = stackalloc int[slice.Length];
#pragma warning restore CA2014
                Span<uint> sliceSpan = new Span<uint>(slicePtr, slice.Length);
                slice.CopyTo(sliceSpan);

                void* data = Unsafe.Add<byte>(imageData, (y * w + x) * 4);
                Buffer.MemoryCopy(slicePtr, data, w * h * 4, xl);
            }
        }
    }
}
