using ArknightsResources.CustomResourceHelpers;
using System;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 为ArknightsResources.Stories.Resources的资源访问提供帮助的类
    /// </summary>
    public class StoryResourceHelper : IStoryResourceGetter
    {
        /// <summary>
        /// 当前ResourceHelper使用的<see cref="System.Resources.ResourceManager"/>
        /// </summary>
#if NET7_0_OR_GREATER
        public static ResourceManager ResourceManager { get; set; }
#else
        public ResourceManager ResourceManager { get; set; }

        /// <summary>
        /// <seealso cref="StoryResourceHelper"/>的实例
        /// </summary>
        public static readonly StoryResourceHelper Instance = new StoryResourceHelper();
#endif

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
#if NET7_0_OR_GREATER
        public static string GetStoryRawText(string codename, CultureInfo cultureInfo)
#else
        public string GetStoryRawText(string codename, CultureInfo cultureInfo)
#endif
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
#if NET7_0_OR_GREATER
        public static async Task<string> GetStoryRawTextAsync(string codename, CultureInfo cultureInfo)
#else
        public async Task<string> GetStoryRawTextAsync(string codename, CultureInfo cultureInfo)
#endif
        {
            return await Task.Run(() => GetStoryRawText(codename, cultureInfo));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
#if NET7_0_OR_GREATER
        public static byte[] GetVideo(string codename, CultureInfo cultureInfo = null)
#else
        public byte[] GetVideo(string codename, CultureInfo cultureInfo = null)
#endif
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
#if NET7_0_OR_GREATER
        public static async Task<byte[]> GetVideoAsync(string codename, CultureInfo cultureInfo = null)
#else
        public async Task<byte[]> GetVideoAsync(string codename, CultureInfo cultureInfo = null)
#endif
        {
            return await Task.Run(() => GetVideo(codename, cultureInfo));
        }
    }
}
