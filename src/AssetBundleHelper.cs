#if NET6_0_OR_GREATER
#pragma warning disable IDE0090
#pragma warning disable IDE0063
#pragma warning disable IDE0066
#endif

using System;
using AssetStudio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ArknightsResources.Operators.Models;
using System.Runtime.CompilerServices;
using Fmod5Sharp.FmodTypes;
using Fmod5Sharp;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 用以读取AssetBundle文件的类
    /// </summary>
    public static class AssetBundleHelper
    {
        // 本类使用AssetStudio项目来读取AssetBundle文件
        // 为了本库的需要，我们修改了AssetStudio项目
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
            byte[] image = GetIllustFromAbPacksInternal(assetBundleFile, imageCodename, isSkin);
            return image;
        }

        /// <summary>
        /// 从指定的AssetBundle包中获取干员的立绘
        /// </summary>
        /// <param name="assetBundleFile">含有AssetBundle包内容的<seealso cref="byte"/>数组</param>
        /// <param name="imageCodename">干员立绘的图像代号</param>
        /// <param name="isSkin">指示干员立绘类型是否为皮肤</param>
        /// <returns>包含干员立绘的<seealso cref="Image{Bgra32}"/></returns>
        [Obsolete("不推荐使用此方法，在未来版本中，将删除此方法")]
        public static Image<Bgra32> GetOperatorIllustrationReturnImage(byte[] assetBundleFile, string imageCodename, bool isSkin)
        {
            byte[] image = GetIllustFromAbPacksInternal(assetBundleFile, imageCodename, isSkin);
            //非常低效...但为了新的实现，不得不这样做
            //因此我们弃用了这个方法
            return Image.Load<Bgra32>(image);
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

        /// <summary>
        /// 从指定的AssetBundle包中获取干员的语音
        /// </summary>
        /// <param name="assetBundleFile">含有AssetBundle包内容的<seealso cref="byte"/>数组</param>
        /// <param name="voiceItem">干员语音条目</param>
        /// <returns></returns>
        public static byte[] GetOperatorVoice(byte[] assetBundleFile, OperatorVoiceLine voiceItem)
        {
            return GetVoiceInternal(assetBundleFile, voiceItem);
        }

        private static unsafe byte[] GetVoiceInternal(byte[] assetBundleFile, OperatorVoiceLine voiceItem)
        {
            using (MemoryStream stream = new MemoryStream(assetBundleFile))
            {
                AssetsManager assetsManager = new AssetsManager();
                //We don't really need file path
                FileReader fileReader = new FileReader(".", stream);
                assetsManager.LoadFileModified(".", fileReader);
                List<AssetStudio.Object> objects = assetsManager.assetsFileList.FirstOrDefault().Objects;
                AudioClip audioClip = GetFromObjects<AudioClip>(objects, (obj) =>
                {
                    if (obj is AudioClip clip)
                    {
                        return clip.m_Name == voiceItem.VoiceId;
                    }
                    else
                    {
                        return false;
                    }
                });

                ResourceReader reader = audioClip.m_AudioData;
                byte[] data = reader.GetData();
                FmodSoundBank bank = FsbLoader.LoadFsbFromByteArray(data);
                FmodSample sample = bank.Samples.FirstOrDefault();
                bool success = sample.RebuildAsStandardFileFormat(out byte[] result, out string fileExtension);

                if (success != true)
                {
                    throw new ArgumentException("无效的语音文件");
                }

                fileReader.Dispose();
                assetsManager.Clear();

                return result;
            }
        }

        private static byte[] GetIllustFromAbPacksInternal(byte[] assetBundleFile, string imageCodename, bool isSkin)
        {
            using (MemoryStream abPackStream = new MemoryStream(assetBundleFile))
            {
                AssetsManager assetsManager = new AssetsManager();
                //We don't really need file path
                FileReader fileReader = new FileReader(".", abPackStream);
                assetsManager.LoadFileModified(".", fileReader);
                List<AssetStudio.Object> objects = assetsManager.assetsFileList.FirstOrDefault().Objects;
                Material material = GetFromObjects<Material>(objects, (obj) => IsMaterialMatchOperatorIllust(obj, imageCodename, isSkin), true);
                if (material is null)
                {
                    //找不到Material，说明此包已包含合并好的立绘文件，直接获取即可
                    Texture2D texture2D = GetFromObjects<Texture2D>(objects, obj => IsTexture2DMatchOperatorIllust(obj, imageCodename));

                    Image<Bgra32> image = texture2D.ConvertToImage();

                    using (MemoryStream stream = new MemoryStream(image.Height * image.Width * 4))
                    {
                        image.SaveAsPng(stream);

                        image.Dispose();

                        return stream.ToArray();
                    }
                }

                Image<Bgra32> rgb;
                Image<Bgra32> alpha;

                KeyValuePair<string, UnityTexEnv>[] m_TexEnvs = material.m_SavedProperties.m_TexEnvs;
                long rgbPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_MainTex");
                long alphaPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_AlphaTex");

                Texture2D rgbTexture2D = GetByPathID<Texture2D>(objects, rgbPath, true);
                Texture2D alphaTexture2D = GetByPathID<Texture2D>(objects, alphaPath, true);
                if (rgbTexture2D is null || alphaTexture2D is null)
                {
                    //有的包Material文件中指向Texture2D的PathID为0,这里使用回退方式
                    //回退方式的结果不一定准确
                    FallbackGetIllustFromAbPacksInternal(objects, imageCodename, isSkin, out rgb, out alpha);

                    if (rgb is null || alpha is null)
                    {
                        throw new ArgumentException("无法从包中解析出立绘文件");
                    }
                    return ImageHelper.ProcessImage(rgb, alpha);
                }
                rgb = rgbTexture2D.ConvertToImage();
                alpha = alphaTexture2D.ConvertToImage();

                fileReader.Dispose();
                assetsManager.Clear();

                return ImageHelper.ProcessImage(rgb, alpha);
            }
        }

        private static void GetSpineAnimationFromAbPacksInternal(byte[] assetBundleFile, OperatorSpineInfo spineInfo, out StreamReader atlasReader, out StreamReader skelReader, out byte[] image)
        {
            using (MemoryStream stream = new MemoryStream(assetBundleFile))
            {
                AssetsManager assetsManager = new AssetsManager();
                //We don't really need file path
                FileReader reader = new FileReader(".", stream);
                assetsManager.LoadFileModified(".", reader);
                List<AssetStudio.Object> objects = assetsManager.assetsFileList.FirstOrDefault().Objects;
                switch (spineInfo.ModelSet)
                {
                    case OperatorSpineModelSet.CombatFront:
                    case OperatorSpineModelSet.CombatBack:
                        (StreamReader, StreamReader, byte[]) animation = GetFrontOrBackSpineAnimation(objects, spineInfo);
                        atlasReader = animation.Item1;
                        skelReader = animation.Item2;
                        image = animation.Item3;
                        break;
                    case OperatorSpineModelSet.Build:
                    default:
                        (StreamReader, StreamReader, byte[]) buildAnimation = GetBuildSpineAnimation(objects, spineInfo);
                        atlasReader = buildAnimation.Item1;
                        skelReader = buildAnimation.Item2;
                        image = buildAnimation.Item3;
                        break;
                }
            }
        }

        private static bool IsMaterialMatchOperatorIllust(AssetStudio.Object obj, string imageCodename, bool isSkin)
        {
            if (obj is Material material)
            {
                if (imageCodename.Contains('+'))
                {
                    imageCodename = imageCodename.Replace("+", @"\+");
                }

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

        private static bool IsTexture2DMatchOperatorIllust(AssetStudio.Object obj, string imageCodename)
        {
            if (obj is Texture2D texture2D)
            {
                if (imageCodename.Contains('+'))
                {
                    imageCodename = imageCodename.Replace("+", @"\+");
                }

                string pattern = $@"char_[\d]*_({imageCodename})(?!b)";

                Match match = GetMatchByPattern(texture2D.m_Name, pattern);

                return match.Success && string.IsNullOrEmpty(match.Groups[2].Value);
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

        //三元组的第一项为包含atlas的StreamReader
        //第二项为包含二进制形式skel文件的StreamReader
        //第三项为Spine动画所需的PNG格式图片
        private static (StreamReader, StreamReader, byte[]) GetFrontOrBackSpineAnimation(IEnumerable<AssetStudio.Object> objects, OperatorSpineInfo spineInfo)
        {
            string type;

            switch (spineInfo.ModelSet)
            {
                case OperatorSpineModelSet.CombatFront:
                    type = "_front";
                    break;
                case OperatorSpineModelSet.CombatBack:
                    type = "_back";
                    break;
                case OperatorSpineModelSet.Build:
                default:
                    throw new InvalidOperationException($"参数{nameof(spineInfo.ModelSet)}的值为Build，这对该方法来说无效");
            }

            IEnumerable<MonoBehaviour> characterAnimators = from animator in objects where animator is MonoBehaviour behaviour
                       && behaviour.m_Script.TryGet(out var m_Script)
                       && (m_Script.m_ClassName == "CharacterAnimator" || m_Script.m_ClassName == "SingleSpineAnimator")
                       select (MonoBehaviour)animator;

            //一般情况下,下面的循环只会执行一次
            //但如果包内有多个Spine动画，那下面的循环会寻找满足条件的那个动画
            foreach (MonoBehaviour characterAnimator in characterAnimators)
            {
                dynamic charAnimatorDict = characterAnimator.ToType();
                dynamic frontOrSingleOrBackDict = charAnimatorDict[type];
                dynamic skeletonNode;
                if (frontOrSingleOrBackDict is null)
                {
                    //这是包没有战斗正面与战斗背面之分的情况(比如安洁莉娜)
                    //Animator为SingleSpineAnimator
                    skeletonNode = charAnimatorDict["_skeleton"];
                }
                else
                {
                    skeletonNode = frontOrSingleOrBackDict["skeleton"];
                }
                long skeletonAnimationPathID = (long)skeletonNode["m_PathID"];
                MonoBehaviour skeletonAnimation = GetByPathID<MonoBehaviour>(objects, skeletonAnimationPathID);

                dynamic skeletonAnimationDict = skeletonAnimation.ToType();
                dynamic skeletonAnimationRoot = skeletonAnimationDict["skeletonDataAsset"];
                long skeletonDataPathID = (long)skeletonAnimationRoot["m_PathID"];
                MonoBehaviour skeletonData = GetByPathID<MonoBehaviour>(objects, skeletonDataPathID);

                dynamic skeletonDataDict = skeletonData.ToType();
                dynamic atlasAssetsDict = skeletonDataDict["atlasAssets"];
                dynamic atlasAssetsDictTarget = atlasAssetsDict[0];
                long atlasAssetsPathID = (long)atlasAssetsDictTarget["m_PathID"];
                MonoBehaviour altasAssets = GetByPathID<MonoBehaviour>(objects, atlasAssetsPathID);

                dynamic atlasAssetsNode = altasAssets.ToType();
                dynamic atlasFile = atlasAssetsNode["atlasFile"];
                dynamic materials = atlasAssetsNode["materials"];
                dynamic materialsTarget = materials[0];
                #region Image
                long materialPathID = (long)materialsTarget["m_PathID"];
                Material material = GetByPathID<Material>(objects, materialPathID);
                KeyValuePair<string, UnityTexEnv>[] m_TexEnvs = material.m_SavedProperties.m_TexEnvs;
                long rgbPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_MainTex");
                long alphaPath = GetPathIDFromKeyValuePairs(m_TexEnvs, "_AlphaTex");

                Texture2D rgbTexture2D = GetByPathID<Texture2D>(objects, rgbPath);
                Match match = GetMatchByPattern(rgbTexture2D.m_Name, $@"char_[\d]*_({spineInfo.ImageCodename})(b?)(\[alpha\])?");
                if (!match.Success)
                {
                    //条件不满足,跳转到下一个循环迭代
                    continue;
                }
                Image<Bgra32> rgb = rgbTexture2D.ConvertToImage();
                Texture2D alphaTexture2D = GetByPathID<Texture2D>(objects, alphaPath);
                Image<Bgra32> alpha = alphaTexture2D.ConvertToImage();
                byte[] image = ImageHelper.ProcessImage(rgb, alpha);
                #endregion

                #region Skel
                //尽管这里节点名称有"skeletonJSON",但是其返回的是二进制skel文件
                dynamic skeletonJSON = skeletonDataDict["skeletonJSON"];
                long skelPathID = (long)skeletonJSON["m_PathID"];

                TextAsset skel = GetByPathID<TextAsset>(objects, skelPathID);
                MemoryStream skelStream = new MemoryStream(skel.m_Script);
                skelStream.Seek(0, SeekOrigin.Begin);
                StreamReader skelReader = new StreamReader(skelStream);
                #endregion

                #region Atlas
                long atlasPathID = (long)atlasFile["m_PathID"];
                TextAsset atlas = GetByPathID<TextAsset>(objects, atlasPathID);
                MemoryStream atlasStream = new MemoryStream(atlas.m_Script);
                atlasStream.Seek(0, SeekOrigin.Begin);
                StreamReader atlasReader = new StreamReader(atlasStream);
                #endregion
                return (atlasReader, skelReader, image);
            }

            throw new ArgumentException($"通过当前的{spineInfo.ImageCodename}找不到Spine动画");
        }

        private static (StreamReader, StreamReader, byte[]) GetBuildSpineAnimation(IEnumerable<AssetStudio.Object> objects, OperatorSpineInfo spineInfo)
        {
            byte[] image;
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
            MemoryStream atlasStream = new MemoryStream(atlas.m_Script);
            atlasStream.Seek(0, SeekOrigin.Begin);
            StreamReader atlasReader = new StreamReader(atlasStream);

            MemoryStream skelStream = new MemoryStream(skel.m_Script);
            skelStream.Seek(0, SeekOrigin.Begin);
            StreamReader skelReader = new StreamReader(skelStream);

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

        private static T GetByPathID<T>(IEnumerable<AssetStudio.Object> objects, long path, bool returnDefault = false)
        {
            foreach (AssetStudio.Object obj in objects)
            {
                if (obj.m_PathID == path && obj is T target)
                {
                    return target;
                }
            }

            if (returnDefault)
            {
                return default;
            }
            else
            {
                throw new ArgumentException($"在包中找不到特定的{typeof(T).Name}\n使用的PathID为:{path}");
            }
        }

        private static T GetFromObjects<T>(IEnumerable<AssetStudio.Object> objects, Predicate<AssetStudio.Object> predicate, bool returnDefault = false)
        {
            foreach (var obj1 in objects)
            {
                if (predicate(obj1) && obj1 is T target)
                {
                    return target;
                }
            }

            if (returnDefault)
            {
                return default;
            }
            else
            {
                throw new ArgumentException($"在包中找不到特定的{typeof(T).Name}");
            }
        }

        private static unsafe Image<Bgra32> ConvertToImage(this Texture2D m_Texture2D)
        {
            if (m_Texture2D is null)
            {
                throw new ArgumentNullException(nameof(m_Texture2D));
            }

            ResourceReader reader = m_Texture2D.image_data;
            void* memPtr = InternalNativeMemory.Alloc(reader.Size);
            Span<byte> originData = new Span<byte>(memPtr, reader.Size);
            reader.GetData(originData);

            int count = m_Texture2D.m_Height * m_Texture2D.m_Width * 4;
            void* memPtrData = InternalNativeMemory.Alloc(count);
            Span<byte> data = new Span<byte>(memPtrData, count);

            switch (m_Texture2D.m_TextureFormat)
            {
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC_RGB4_3DS:
                    ImageHelper.DecodeETC1(originData, memPtrData, m_Texture2D.m_Width, m_Texture2D.m_Height);
                    break;
                case TextureFormat.ASTC_RGB_4x4:
                case TextureFormat.ASTC_RGBA_4x4:
                case TextureFormat.ASTC_HDR_4x4:
                    ImageHelper.DecodeASTC(originData, memPtrData, m_Texture2D.m_Width, m_Texture2D.m_Height, 4);
                    break;
                case TextureFormat.ASTC_RGB_5x5:
                case TextureFormat.ASTC_RGBA_5x5:
                case TextureFormat.ASTC_HDR_5x5:
                    ImageHelper.DecodeASTC(originData, memPtrData, m_Texture2D.m_Width, m_Texture2D.m_Height, 5);
                    break;
                case TextureFormat.ASTC_RGB_6x6:
                case TextureFormat.ASTC_RGBA_6x6:
                case TextureFormat.ASTC_HDR_6x6:
                    ImageHelper.DecodeASTC(originData, memPtrData, m_Texture2D.m_Width, m_Texture2D.m_Height, 6);
                    break;
                case TextureFormat.ASTC_RGB_8x8:
                case TextureFormat.ASTC_RGBA_8x8:
                case TextureFormat.ASTC_HDR_8x8:
                    ImageHelper.DecodeASTC(originData, memPtrData, m_Texture2D.m_Width, m_Texture2D.m_Height, 8);
                    break;
                case TextureFormat.ASTC_RGB_10x10:
                case TextureFormat.ASTC_RGBA_10x10:
                case TextureFormat.ASTC_HDR_10x10:
                    ImageHelper.DecodeASTC(originData, memPtrData, m_Texture2D.m_Width, m_Texture2D.m_Height, 10);
                    break;
                case TextureFormat.ASTC_RGB_12x12:
                case TextureFormat.ASTC_RGBA_12x12:
                case TextureFormat.ASTC_HDR_12x12:
                    ImageHelper.DecodeASTC(originData, memPtrData, m_Texture2D.m_Width, m_Texture2D.m_Height, 12);
                    break;
                default:
                    throw new NotImplementedException($"无法解码图像，因为未实现 {m_Texture2D.m_TextureFormat} 的解码器");
            }

            try
            {
                Image<Bgra32> image = Image.LoadPixelData<Bgra32>(data, m_Texture2D.m_Width, m_Texture2D.m_Height);
                image.Mutate(x => x.Flip(FlipMode.Vertical));
                return image;
            }
            finally
            {
                InternalNativeMemory.Free(memPtr);
                InternalNativeMemory.Free(memPtrData);
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

        #region Fallback
        //有的包Material文件中指向Texture2D的PathID为0,这里提供回退方式
        //回退方式的结果不一定准确

        private static void FallbackGetIllustFromAbPacksInternal(IEnumerable<AssetStudio.Object> objects, string imageCodename, bool isSkin, out Image<Bgra32> rgb, out Image<Bgra32> alpha)
        {
            alpha = null;
            rgb = null;

            IEnumerable<Texture2D> targets = from asset
                                             in objects
                                             where FallbackIsTexture2DMatchOperatorImage(asset, imageCodename, isSkin)
                                             select (asset as Texture2D);

            foreach (var item in targets)
            {
                if (item.m_Name.Contains("[alpha]"))
                {
                    alpha = item.ConvertToImage();
                }
                else
                {
                    rgb = item.ConvertToImage();
                }
            }
        }

        private static bool FallbackIsTexture2DMatchOperatorImage(AssetStudio.Object asset, string imageCodename, bool isSkin)
        {
            if (asset.type == ClassIDType.Texture2D)
            {
                Texture2D texture2D = (Texture2D)asset;
                if (texture2D.m_Width <= 512 || texture2D.m_Height <= 512)
                {
                    return false;
                }

                Match match;
                if (isSkin)
                {
                    if (imageCodename.Contains('#'))
                    {
                        //有的皮肤(如阿),具有两个皮肤,但只能以后面的'#(数字)'区分,所以这里进行了特殊处理
                        match = Regex.Match(texture2D.m_Name, $@"char_[\d]*_({imageCodename})(b?)(\[alpha\])?",
                                          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
                        {
                            //如果match匹配成功,那么说明这个文件不符合要求,返回false
                            return false;
                        }
                    }
                    else
                    {
                        //如果match匹配成功,那么说明这个文件不符合要求,返回false
                        match = Regex.Match(texture2D.m_Name, $@"char_[\d]*_({imageCodename})#([\d]*)(b?)(\[alpha\])?",
                                          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[3].Value))
                        {
                            //如果match匹配成功,那么说明这个文件不符合要求,返回false
                            return false;
                        }
                    }
                }
                else
                {
                    match = Regex.Match(texture2D.m_Name, $@"char_[\d]*_({imageCodename}\+?)(?!b)(\[alpha\])?");
                }

                return match.Success && string.Equals(match.Groups[1].Value, imageCodename);
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
