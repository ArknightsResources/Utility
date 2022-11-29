using ArknightsResources.CustomResourceHelpers;
using System;
using System.Globalization;
using System.Resources;
using System.Text;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 为ArknightsResources.Stories.Resources的资源访问提供帮助的类
    /// </summary>
#if NET7_0_OR_GREATER
    public class StoryResourceHelper : IStoryResourceHelper
    {
#else
    public class StoryResourceHelper : CustomResourceHelpers.StoryResourceHelper
    {
        /// <summary>
        /// <seealso cref="StoryResourceHelper"/>的实例
        /// </summary>
        public static readonly StoryResourceHelper Instance = new StoryResourceHelper();
#endif

        /// <summary>
        /// 当前ResourceHelper使用的<see cref="System.Resources.ResourceManager"/>
        /// </summary>
#if NET7_0_OR_GREATER
        public static ResourceManager ResourceManager { get; set; }

        /// <summary>
        /// 简体中文的语言文化信息
        /// </summary>
        public static readonly CultureInfo ChineseSimplifiedCultureInfo = CultureInfo.ReadOnly(new CultureInfo("zh-CN", false));
        /// <summary>
        /// 繁体中文的语言文化信息
        /// </summary>
        public static readonly CultureInfo ChineseTraditionalCultureInfo = CultureInfo.ReadOnly(new CultureInfo("zh-TW", false));
        /// <summary>
        /// 英语的语言文化信息
        /// </summary>
        public static readonly CultureInfo EnglishCultureInfo = CultureInfo.ReadOnly(new CultureInfo("en-US", false));
        /// <summary>
        /// 日语的语言文化信息
        /// </summary>
        public static readonly CultureInfo JapaneseCultureInfo = CultureInfo.ReadOnly(new CultureInfo("ja-JP", false));
        /// <summary>
        /// 韩语的语言文化信息
        /// </summary>
        public static readonly CultureInfo KoreanCultureInfo = CultureInfo.ReadOnly(new CultureInfo("ko-KR", false));
#else
        public ResourceManager ResourceManager { get; set; }
#endif

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
#if NET7_0_OR_GREATER
        public static string GetStoryRawText(string codename, CultureInfo cultureInfo)
#else
        public override string GetStoryRawText(string codename, CultureInfo cultureInfo)
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
        public static byte[] GetVideo(string codename, CultureInfo cultureInfo = null)
#else
        public override byte[] GetVideo(string codename, CultureInfo cultureInfo = null)
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
    }
}
