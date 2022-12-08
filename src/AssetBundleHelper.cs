#if NET6_0_OR_GREATER
#pragma warning disable IDE0090
#pragma warning disable IDE0063
#pragma warning disable IDE0066
#endif
#pragma warning disable IDE0017

using System;
using AssetStudio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.ColorSpaces;
using ArknightsResources.Operators.Models;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Specialized;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 用以读取AssetBundle文件的类
    /// </summary>
    public static class AssetBundleHelper
    {
        // 本类使用AssetStudio项目来读取AssetBundle文件
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
        /// 从指定的AssetBundle包中获取干员的立绘
        /// </summary>
        /// <param name="assetBundleFile">含有AssetBundle包内容的<seealso cref="byte"/>数组</param>
        /// <param name="imageCodename">干员立绘的图像代号</param>
        /// <param name="isSkin">指示干员立绘类型是否为皮肤</param>
        /// <returns>包含干员立绘的<seealso cref="byte"/>数组</returns>
        public static byte[] GetOperatorIllustration(byte[] assetBundleFile, string imageCodename, bool isSkin)
        {
            GetIllustFromAbPacksInternal(assetBundleFile, imageCodename, isSkin, out Image<Bgra32> rgb, out Image<Bgra32> alpha);
            return ImageHelper.ProcessImage(rgb, alpha);
        }

        /// <summary>
        /// 从指定的AssetBundle包中获取干员的Spine动画
        /// </summary>
        /// <param name="assetBundleFile">含有AssetBundle包内容的<seealso cref="byte"/>数组</param>
        /// <param name="spineInfo">干员Spine动画的信息</param>
        /// <returns>一个三元组,第一项为包含atlas的<see cref="TextReader"/>,第二项为包含二进制形式skel文件的<see cref="TextReader"/>,第三项为Spine动画所需的PNG格式图片</returns>
        public static (TextReader, TextReader, byte[]) GetOperatorSpineAnimation(byte[] assetBundleFile, OperatorSpineInfo spineInfo)
        {
            GetSpineAnimationFromAbPacksInternal(assetBundleFile, spineInfo, out StreamReader atlas, out StreamReader skel, out byte[] image);
            return (atlas, skel, image);
        }

        private static void GetIllustFromAbPacksInternal(byte[] assetBundleFile, string imageCodename, bool isSkin, out Image<Bgra32> rgb, out Image<Bgra32> alpha)
        {
            using (MemoryStream stream = new MemoryStream(assetBundleFile))
            {
                AssetsManager assetsManager = new AssetsManager();
                //We don't really need file path
                FileReader reader = new FileReader(".", stream);
                assetsManager.LoadFile(".", reader);
                List<AssetStudio.Object> objects = assetsManager.assetsFileList.FirstOrDefault().Objects;
                Material material = GetFromObjects<Material>(objects, (obj) => IsMaterialMatchOperatorIllust(obj, imageCodename, isSkin));

                KeyValuePair<string, UnityTexEnv>[] m_TexEnvs = material.m_SavedProperties.m_TexEnvs;
                long rgbPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_MainTex");
                long alphaPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_AlphaTex");

                rgb = GetByPathID<Texture2D>(objects, rgbPath).ConvertToImage();
                alpha = GetByPathID<Texture2D>(objects, alphaPath).ConvertToImage();
            }
        }

        private static void GetSpineAnimationFromAbPacksInternal(byte[] assetBundleFile, OperatorSpineInfo spineInfo, out StreamReader atlasReader, out StreamReader skelReader, out byte[] image)
        {
            using (MemoryStream stream = new MemoryStream(assetBundleFile))
            {
                AssetsManager assetsManager = new AssetsManager();
                //We don't really need file path
                FileReader reader = new FileReader(".", stream);
                assetsManager.LoadFile(".", reader);
                List<AssetStudio.Object> objects = assetsManager.assetsFileList.FirstOrDefault().Objects;
                switch (spineInfo.ModelSet)
                {
                    case OperatorSpineModelSet.CombatFront:
                    case OperatorSpineModelSet.CombatBack:
                        (StreamReader, StreamReader, byte[]) animation = GetFrontOrBackSpineAnimation(objects, spineInfo.ModelSet);
                        atlasReader = animation.Item1;
                        skelReader = animation.Item2;
                        image = animation.Item3;
                        break;
                    case OperatorSpineModelSet.Build:
                    default:
                        #region Build
                        {
                            //防止变量外溢
                            Material material = GetFromObjects<Material>(objects, (obj) =>
                                IsMaterialMatchBuildSpineAnimation(obj, spineInfo));
                            KeyValuePair<string, UnityTexEnv>[] m_TexEnvs = material.m_SavedProperties.m_TexEnvs;
                            long rgbPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_MainTex");
                            long alphaPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_AlphaTex");
                            Image<Bgra32> rgb = GetByPathID<Texture2D>(objects, rgbPath).ConvertToImage();
                            Image<Bgra32> alpha = GetByPathID<Texture2D>(objects, alphaPath).ConvertToImage();
                            image = ImageHelper.ProcessImage(rgb, alpha);
                        }
                        TextAsset atlas = GetFromObjects<TextAsset>(objects, (obj) =>
                        {
                            if (obj is TextAsset text)
                            {
                                //有的皮肤(如阿),具有两个皮肤,但只能以后面的'#(数字)'区分,这种情况不按皮肤方式处理
                                string pattern = spineInfo.IsSkin && !spineInfo.ImageCodename.Contains('#')
                                ? $@"build_char_[\d]*_({spineInfo.ImageCodename})#([\d]*).atlas"
                                : $@"build_char_[\d]*_({spineInfo.ImageCodename}).atlas";

                                Match match = GetMatchByPattern(text.m_Name, pattern);
                                return match.Success;
                            }
                            else
                            {
                                return false;
                            }
                        });

                        TextAsset skel = GetFromObjects<TextAsset>(objects, (obj) =>
                        {
                            if (obj is TextAsset text)
                            {
                                //有的皮肤(如阿),具有两个皮肤,但只能以后面的'#(数字)'区分,这种情况不按皮肤方式处理
                                string pattern = spineInfo.IsSkin && !spineInfo.ImageCodename.Contains('#')
                                ? $@"build_char_[\d]*_({spineInfo.ImageCodename})#([\d]*).skel"
                                : $@"build_char_[\d]*_({spineInfo.ImageCodename}).skel";

                                Match match = GetMatchByPattern(text.m_Name, pattern);
                                return match.Success;
                            }
                            else
                            {
                                return false;
                            }
                        });
                        MemoryStream atlasStream = new MemoryStream(atlas.m_Script)
                        {
                            Position = 0
                        };
                        atlasReader = new StreamReader(atlasStream);

                        MemoryStream skelStream = new MemoryStream(skel.m_Script)
                        {
                            Position = 0
                        };
                        skelReader = new StreamReader(skelStream);
                        #endregion
                        break;
                }
            }
        }

        private static bool IsMaterialMatchOperatorIllust(AssetStudio.Object obj, string imageCodename, bool isSkin)
        {
            if (obj is Material material)
            {
                //有的皮肤(如阿),具有两个皮肤,但只能以后面的'#(数字)'区分,这种情况不按皮肤方式处理
                string pattern = isSkin && !imageCodename.Contains('#')
                    ? $@"illust_char_[\d]*_({imageCodename})#([\d]*)(?!b)_material"
                    : $@"illust_char_[\d]*_({imageCodename})(?!b)_material";
                Match match = GetMatchByPattern(material.m_Name, pattern);
                return match.Success;
            }
            else
            {
                return false;
            }
        }

        private static bool IsMaterialMatchBuildSpineAnimation(AssetStudio.Object obj, OperatorSpineInfo spineInfo)
        {
            if (obj is Material material)
            {
                switch (spineInfo.ModelSet)
                {
                    case OperatorSpineModelSet.Build:
                        string pattern;
                        //有的皮肤(如阿),具有两个皮肤,但只能以后面的'#(数字)'区分,这种情况不按皮肤方式处理
                        pattern = spineInfo.IsSkin && !spineInfo.ImageCodename.Contains('#')
                            ? $@"build_char_[\d]*_({spineInfo.ImageCodename})#([\d]*)_Material"
                            : $@"build_char_[\d]*_({spineInfo.ImageCodename})_Material";

                        Match match = GetMatchByPattern(material.m_Name, pattern);
                        return match.Success;
                    case OperatorSpineModelSet.CombatFront:
                    case OperatorSpineModelSet.CombatBack:
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static (StreamReader, StreamReader, byte[]) GetFrontOrBackSpineAnimation(IEnumerable<AssetStudio.Object> objects, OperatorSpineModelSet modelSet)
        {
            string type;

            switch (modelSet)
            {
                case OperatorSpineModelSet.CombatFront:
                    type = "_front";
                    break;
                case OperatorSpineModelSet.CombatBack:
                    type = "_back";
                    break;
                case OperatorSpineModelSet.Build:
                default:
                    throw new InvalidOperationException($"参数{nameof(modelSet)}的值为Build，这对该方法来说无效");
            }

            MonoBehaviour characterAnimator = GetFromObjects<MonoBehaviour>(objects, (obj) =>
            {
                return obj is MonoBehaviour behaviour
                       && behaviour.m_Script.TryGet(out var m_Script)
                       && m_Script.m_ClassName == "CharacterAnimator";
            });
            OrderedDictionary charAnimatorDict = characterAnimator.ToType();
            OrderedDictionary frontOrBackDict = (OrderedDictionary)charAnimatorDict[type];
            OrderedDictionary skeletonNode = (OrderedDictionary)frontOrBackDict["skeleton"];
            long skeletonAnimationPathID = (long)skeletonNode["m_PathID"];
            MonoBehaviour skeletonAnimation = GetByPathID<MonoBehaviour>(objects, skeletonAnimationPathID);

            OrderedDictionary skeletonAnimationDict = skeletonAnimation.ToType();
            OrderedDictionary skeletonAnimationRoot = (OrderedDictionary)skeletonAnimationDict["skeletonDataAsset"];
            long skeletonDataPathID = (long)skeletonAnimationRoot["m_PathID"];
            MonoBehaviour skeletonData = GetByPathID<MonoBehaviour>(objects, skeletonDataPathID);

            OrderedDictionary skeletonDataDict = skeletonData.ToType();
            #region Skel
            //尽管这里节点名称有"skeletonJSON",但是其返回的是二进制skel文件
            OrderedDictionary skeletonJSON = (OrderedDictionary)skeletonDataDict["skeletonJSON"];
            long skelPathID = (long)skeletonJSON["m_PathID"];

            TextAsset skel = GetByPathID<TextAsset>(objects, skelPathID);
            MemoryStream skelStream = new MemoryStream(skel.m_Script);
            skelStream.Position = 0;
            StreamReader skelReader = new StreamReader(skelStream);
            #endregion

            IList<object> atlasAssetsDict = (IList<object>)skeletonDataDict["atlasAssets"];
            OrderedDictionary atlasAssetsDictTarget = (OrderedDictionary)atlasAssetsDict[0];
            long atlasAssetsPathID = (long)atlasAssetsDictTarget["m_PathID"];
            MonoBehaviour altasAssets = GetByPathID<MonoBehaviour>(objects, atlasAssetsPathID);

            OrderedDictionary atlasAssetsNode = altasAssets.ToType();
            OrderedDictionary atlasFile = (OrderedDictionary)atlasAssetsNode["atlasFile"];
            #region Atlas
            long atlasPathID = (long)atlasFile["m_PathID"];
            TextAsset atlas = GetByPathID<TextAsset>(objects, atlasPathID);
            MemoryStream atlasStream = new MemoryStream(atlas.m_Script);
            atlasStream.Position = 0;
            StreamReader atlasReader = new StreamReader(atlasStream);
            #endregion

            IList<object> materials = (IList<object>)atlasAssetsNode["materials"];
            OrderedDictionary materialsTarget = (OrderedDictionary)materials[0];
            #region Image
            long materialPathID = (long)materialsTarget["m_PathID"];
            Material material = GetByPathID<Material>(objects, materialPathID);
            KeyValuePair<string, UnityTexEnv>[] m_TexEnvs = material.m_SavedProperties.m_TexEnvs;
            long rgbPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_MainTex");
            long alphaPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_AlphaTex");
            Image<Bgra32> rgb = GetByPathID<Texture2D>(objects, rgbPath).ConvertToImage();
            Image<Bgra32> alpha = GetByPathID<Texture2D>(objects, alphaPath).ConvertToImage();
            byte[] image = ImageHelper.ProcessImage(rgb, alpha);
            #endregion

            return (atlasReader, skelReader, image);
        }

        private static long GetPathIDFromKeyValuePairs(KeyValuePair<string, UnityTexEnv>[] m_TexEnvs, string key)
        {
            foreach (KeyValuePair<string, UnityTexEnv> pair in m_TexEnvs)
            {
                if (pair.Key == key)
                {
                    return pair.Value.m_Texture.m_PathID;
                }
            }

            throw new ArgumentException($"在Material中找不到特定的资源\n使用的Key为:{key}");
        }

        private static T GetByPathID<T>(IEnumerable<AssetStudio.Object> objects, long path)
        {
            foreach (AssetStudio.Object obj in objects)
            {
                if (obj.m_PathID == path && obj is T target)
                {
                    return target;
                }
            }

            throw new ArgumentException($"在包中找不到特定的{typeof(T).Name}\n使用的PathID为:{path}");
        }

        private static T GetFromObjects<T>(IEnumerable<AssetStudio.Object> objects, Predicate<AssetStudio.Object> predicate)
        {
            foreach (var obj1 in objects)
            {
                if (predicate(obj1) && obj1 is T target)
                {
                    return target;
                }
            }

            throw new ArgumentException($"在包中找不到特定的{typeof(T).Name}");
        }

        private static unsafe Image<Bgra32> ConvertToImage(this Texture2D m_Texture2D)
        {
            if (m_Texture2D is null)
            {
                throw new ArgumentNullException(nameof(m_Texture2D));
            }

            ResourceReader reader = m_Texture2D.image_data;
#if NET6_0_OR_GREATER
            void* memPtr = NativeMemory.AllocZeroed((nuint)reader.Size);
            Span<byte> originData = new Span<byte>(memPtr, reader.Size);
#else
            byte[] originData = InternalArrayPools.ByteArrayPool.Rent(reader.Size);
#endif
            reader.GetData(originData);

            byte[] data = ImageHelper.DecodeETC1(originData, m_Texture2D.m_Width, m_Texture2D.m_Height);

            try
            {
                var image = Image.LoadPixelData<Bgra32>(data, m_Texture2D.m_Width, m_Texture2D.m_Height);
                image.Mutate(x => x.Flip(FlipMode.Vertical));
                return image;
            }
            finally
            {
#if NET6_0_OR_GREATER
                NativeMemory.Free(memPtr);
#else
                InternalArrayPools.ByteArrayPool.Return(originData);
#endif
            }
        }

        /// <summary>
        /// 通过要分析的字符串与正则表达式字符串获取<seealso cref="Match"/>对象
        /// </summary>
        /// <param name="strToAnalyse">要分析的字符串</param>
        /// <param name="pattern">正则表达式字符串</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Match GetMatchByPattern(string strToAnalyse, string pattern)
        {
            return Regex.Match(strToAnalyse, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
    }
}
