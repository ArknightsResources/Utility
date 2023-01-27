#pragma warning disable IDE0019

using System;
using System.Resources;
using System.Threading.Tasks;
using ArknightsResources.CustomResourceHelpers;
using ArknightsResources.Operators.Models;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.IO;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 为ArknightsResources.Operators.IllustResources的资源访问提供帮助的结构
    /// </summary>
    public readonly struct OperatorIllustResourceHelper : IOperatorIllustrationGetter, IOperatorSpineAnimationGetter
    {
        /// <summary>
        /// 当前ResourceHelper使用的<see cref="System.Resources.ResourceManager"/>
        /// </summary>
        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// 通过指定的参数构造<see cref="OperatorIllustResourceHelper"/>的新实例
        /// </summary>
        /// <param name="resourceManager">该ResourcesHelper使用的<see cref="System.Resources.ResourceManager"/></param>
        /// <exception cref="ArgumentNullException"/>
        public OperatorIllustResourceHelper(ResourceManager resourceManager)
        {
            ResourceManager= resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="MissingManifestResourceException"/>
        /// <exception cref="MissingSatelliteAssemblyException"/>
        public byte[] GetOperatorIllustration(OperatorIllustrationInfo illustInfo)
        {
            byte[] abPack = GetIllustAssetBundleFile(illustInfo);
            if (abPack is null)
            {
                throw new ArgumentException($@"使用给定的参数""{illustInfo}""时找不到资源");
            }

            byte[] image = AssetBundleHelper.GetOperatorIllustration(abPack, illustInfo.ImageCodename, illustInfo.Type == OperatorType.Skin);
            return image;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetOperatorIllustrationAsync(OperatorIllustrationInfo illustInfo)
        {
            OperatorIllustResourceHelper self = this;
            return await Task.Run(() => self.GetOperatorIllustration(illustInfo));
        }

        /// <summary>
        /// 通过干员的立绘信息获取其立绘
        /// </summary>
        /// <param name="illustInfo">干员的立绘信息</param>
        /// <returns>一个<see cref="Image{Bgra32}"/>,其中包含了干员的立绘</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="MissingManifestResourceException"/>
        /// <exception cref="MissingSatelliteAssemblyException"/>
        public Image<Bgra32> GetOperatorIllustrationReturnImage(OperatorIllustrationInfo illustInfo)
        {
            byte[] abPack = GetIllustAssetBundleFile(illustInfo);
            if (abPack is null)
            {
                throw new ArgumentException($@"使用给定的参数""{illustInfo}""时找不到资源");
            }

            Image<Bgra32> image = AssetBundleHelper.GetOperatorIllustrationReturnImage(abPack, illustInfo.ImageCodename, illustInfo.Type == OperatorType.Skin);
            return image;
        }

        /// <summary>
        /// 通过干员的立绘信息获取其立绘
        /// </summary>
        /// <param name="illustInfo">干员的立绘信息</param>
        /// <returns>一个<see cref="Image{Bgra32}"/>,其中包含了干员的立绘</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="MissingManifestResourceException"/>
        /// <exception cref="MissingSatelliteAssemblyException"/>
        public async Task<Image<Bgra32>> GetOperatorIllustrationReturnImageAsync(OperatorIllustrationInfo illustInfo)
        {
            OperatorIllustResourceHelper self = this;
            return await Task.Run(() => self.GetOperatorIllustrationReturnImage(illustInfo));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="MissingManifestResourceException"/>
        /// <exception cref="MissingSatelliteAssemblyException"/>
        public (TextReader, TextReader, byte[]) GetOperatorSpineAnimation(OperatorSpineInfo spineInfo)
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此对象的属性 {nameof(ResourceManager)} 不可为空");
            }

            string fileName;
            string imageCodename = spineInfo.ImageCodename.Split('_')[0].Split('#')[0];
            if (spineInfo.IsSkin)
            {
                fileName = $"operator_image_skin_{imageCodename}";
            }
            else
            {
                if (spineInfo.ModelSet == OperatorSpineModelSet.Build)
                {
                    //升变形式阿米娅的基建小人与普通阿米娅相同
                    fileName = imageCodename == "amiya2" ? $"operator_image_amiya" : $"operator_image_{imageCodename}";
                }
                else
                {
                    fileName = $"operator_image_{imageCodename}";
                }
            }

            byte[] abPack = ResourceManager.GetObject(fileName) as byte[];
            if (abPack is null)
            {
                throw new ArgumentException($@"使用给定的参数""{spineInfo}""时找不到资源");
            }

            (TextReader, TextReader, byte[]) result;
            if (spineInfo.ImageCodename == "amiya2")
            {
                result = AssetBundleHelper.GetOperatorSpineAnimation(abPack, new OperatorSpineInfo(spineInfo.ModelSet, "amiya", false));
            }
            else
            {
                result = AssetBundleHelper.GetOperatorSpineAnimation(abPack, spineInfo);
            }
            return result;
        }

        /// <inheritdoc/>
        public async Task<(TextReader, TextReader, byte[])> GetOperatorSpineAnimationAsync(OperatorSpineInfo spineInfo)
        {
            OperatorIllustResourceHelper self = this;
            return await Task.Run(() => self.GetOperatorSpineAnimation(spineInfo));
        }

        /// <summary>
        /// 通过干员的立绘信息获取包含其立绘等的AssetBundle文件
        /// </summary>
        /// <param name="illustInfo">干员的立绘信息</param>
        /// <returns>一个byte数组,其中包含了AssetBundle文件的数据</returns>
        /// <exception cref="ArgumentException"/>
        public byte[] GetIllustAssetBundleFile(OperatorIllustrationInfo illustInfo)
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此对象的属性 {nameof(ResourceManager)} 不可为空");
            }

            string fileName;
            string imageCodename = illustInfo.ImageCodename.Split('_')[0].Split('#')[0];
            if (illustInfo.Type == OperatorType.Skin)
            {
                fileName = $"operator_image_skin_{imageCodename}";
            }
            else
            {
                fileName = $"operator_image_{imageCodename}";
            }

            byte[] value = (byte[])ResourceManager.GetObject(fileName);
            if (value is null)
            {
                throw new ArgumentException($@"使用给定的参数""{illustInfo}""时找不到资源");
            }

            return value;
        }
    }
}
