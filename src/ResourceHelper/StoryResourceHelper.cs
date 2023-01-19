using ArknightsResources.CustomResourceHelpers;
using System;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 为ArknightsResources.Stories.Resources的资源访问提供帮助的结构
    /// </summary>
    public readonly struct StoryResourceHelper : IStoryResourceGetter
    {
        /// <summary>
        /// 当前ResourceHelper使用的<see cref="System.Resources.ResourceManager"/>
        /// </summary>
        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// 使用指定的参数构造<seealso cref="StoryResourceHelper"/>的新实例
        /// </summary>
        /// <param name="resourceManager">该ResourcesHelper使用的<see cref="System.Resources.ResourceManager"/></param>
        public StoryResourceHelper(ResourceManager resourceManager)
        {
            ResourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        public string GetStoryRawText(string codename, CultureInfo cultureInfo)
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            if (string.IsNullOrWhiteSpace(codename))
            {
                throw new ArgumentException($"“{nameof(codename)}”不能为 null 或空白。", nameof(codename));
            }

            try
            {
                string text = ResourceManager.GetString(codename, cultureInfo);
                return text;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"使用给定的参数\"{codename}\"查找资源时出错", ex);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        public async Task<string> GetStoryRawTextAsync(string codename, CultureInfo cultureInfo)
        {
            StoryResourceHelper self = this;
            return await Task.Run(() => self.GetStoryRawText(codename, cultureInfo));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        public byte[] GetVideo(string codename, CultureInfo cultureInfo = null)
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            if (string.IsNullOrWhiteSpace(codename))
            {
                throw new ArgumentException($"“{nameof(codename)}”不能为 null 或空白。", nameof(codename));
            }

            try
            {
                byte[] videoArray;
                if (cultureInfo is null)
                {
                    videoArray = ResourceManager.GetObject(codename) as byte[];
                }
                else
                {
                    videoArray = ResourceManager.GetObject(codename, cultureInfo) as byte[];
                }

                if (videoArray is null)
                {
                    throw new ArgumentException("找不到资源");
                }

                return videoArray;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"使用给定的参数\"{codename}\"查找资源时出错", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetVideoAsync(string codename, CultureInfo cultureInfo = null)
        {
            StoryResourceHelper self = this;
            return await Task.Run(() => self.GetVideo(codename, cultureInfo));
        }
    }
}
