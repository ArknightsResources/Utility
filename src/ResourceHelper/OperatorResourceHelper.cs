#if NET6_0_OR_GREATER
#pragma warning disable IDE0090
#endif
#pragma warning disable IDE0019

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using ArknightsResources.Operators.Models;
using Org.Brotli.Dec;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 为ArknightsResources.Operators.Resources的资源访问提供帮助的类
    /// </summary>
#if NET7_0_OR_GREATER
    public class OperatorResourceHelper : CustomResourceHelpers.IOperatorResourceHelper
#else
    public class OperatorResourceHelper : CustomResourceHelpers.OperatorResourceHelper
#endif

    {
#if !NET7_0_OR_GREATER
        /// <summary>
        /// <seealso cref="OperatorResourceHelper"/>的实例
        /// </summary>
        public static readonly OperatorResourceHelper Instance = new OperatorResourceHelper();
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
        /// <summary>
        /// 意大利语的语言文化信息
        /// </summary>
        public static readonly CultureInfo ItalianCultureInfo = CultureInfo.ReadOnly(new CultureInfo("it-IT", false));
#else
        public ResourceManager ResourceManager { get; set; }
#endif

        /// <summary>
        /// 获取干员图片代号与干员代号的映射表
        /// </summary>
        /// <param name="cultureInfo">干员代号所用语言</param>
        /// <returns>Key为干员图片代号,Value为干员代号的Dictionary&lt;string, string&gt;</returns>
#if NET7_0_OR_GREATER
        public static Dictionary<string, string> GetOperatorImageCodenameMapping(CultureInfo cultureInfo)
#else
        public Dictionary<string, string> GetOperatorImageCodenameMapping(CultureInfo cultureInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            byte[] stringByteArray = (byte[])ResourceManager.GetObject("operator_image_codename_mapping", cultureInfo);
            string jsonString = Encoding.UTF8.GetString(stringByteArray);
#if NET6_0_OR_GREATER
            Dictionary<string, string> dict = JsonSerializer.Deserialize(jsonString, DictionarySourceGenerationContext.Default.DictionaryStringString);
#else
            Dictionary<string, string> dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
#endif
            return dict;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="MissingManifestResourceException"/>
        /// <exception cref="MissingSatelliteAssemblyException"/>
#if NET7_0_OR_GREATER
        public static byte[] GetOperatorImage(OperatorIllustrationInfo illustInfo)
#else
        public override byte[] GetOperatorImage(OperatorIllustrationInfo illustInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
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

            byte[] abPack = ResourceManager.GetObject(fileName) as byte[];
            if (abPack is null)
            {
                throw new ArgumentException($@"使用给定的参数""{illustInfo}""时找不到资源");
            }

            byte[] image = AssetBundleHelper.GetOperatorIllustration(abPack, illustInfo.ImageCodename, illustInfo.Type == OperatorType.Skin);
            return image;
        }

        /// <inheritdoc/>
#if NET7_0_OR_GREATER
        public static async Task<byte[]> GetOperatorImageAsync(OperatorIllustrationInfo illustInfo)
#else
        public override async Task<byte[]> GetOperatorImageAsync(OperatorIllustrationInfo illustInfo)
#endif
        {
            return await Task.Run(() => GetOperatorImage(illustInfo));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
#if NET7_0_OR_GREATER
        public static Operator GetOperator(string operatorName, CultureInfo cultureInfo)
#else
        public override Operator GetOperator(string operatorName, CultureInfo cultureInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            if (string.IsNullOrWhiteSpace(operatorName))
            {
                throw new ArgumentException($"“{nameof(operatorName)}”不能为 null 或空白。", nameof(operatorName));
            }

            try
            {
                return GetOperatorInternal(operatorName, cultureInfo, ResourceManager);
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException($"参数\"{operatorName}\"无效", ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException($"提供的语言文化\"{cultureInfo}\"无效", ex);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"提供的语言文化\"{cultureInfo}\"无效", ex);
            }
        }

        /// <inheritdoc />
#if NET7_0_OR_GREATER
        public static async Task<Operator> GetOperatorAsync(string operatorName, CultureInfo cultureInfo)
#else
        public override async Task<Operator> GetOperatorAsync(string operatorName, CultureInfo cultureInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            if (string.IsNullOrWhiteSpace(operatorName))
            {
                throw new ArgumentException($"“{nameof(operatorName)}”不能为 null 或空白。", nameof(operatorName));
            }
            
            try
            {
                return await Task.Run(() => GetOperatorInternal(operatorName, cultureInfo,ResourceManager));
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException($"参数\"{operatorName}\"无效", ex);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"提供的语言文化\"{cultureInfo}\"无效", ex);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
#if NET7_0_OR_GREATER
        public static Operator GetOperatorWithImageCodename(string imageCodename, CultureInfo cultureInfo)
#else
        public override Operator GetOperatorWithImageCodename(string imageCodename, CultureInfo cultureInfo)
#endif
        {
            if (string.IsNullOrWhiteSpace(imageCodename))
            {
                throw new ArgumentException($"“{nameof(imageCodename)}”不能为 null 或空白。", nameof(imageCodename));
            }

            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            try
            {
                return GetOperatorWithCodenameInternal(imageCodename, cultureInfo, ResourceManager);
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException($"参数\"{imageCodename}\"无效", ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException($"提供的语言文化\"{cultureInfo}\"无效", ex);
            }
        }

        /// <inheritdoc/>
#if NET7_0_OR_GREATER
        public static async Task<Operator> GetOperatorWithImageCodenameAsync(string imageCodename, CultureInfo cultureInfo)
#else
        public override async Task<Operator> GetOperatorWithImageCodenameAsync(string imageCodename, CultureInfo cultureInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            if (string.IsNullOrWhiteSpace(imageCodename))
            {
                throw new ArgumentException($"“{nameof(imageCodename)}”不能为 null 或空白。", nameof(imageCodename));
            }

            try
            {
                return await Task.Run(() => GetOperatorWithCodenameInternal(imageCodename, cultureInfo, ResourceManager));
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException($"参数\"{imageCodename}\"无效", ex);
            }
        }

        /// <inheritdoc/>
#if NET7_0_OR_GREATER
        public static OperatorsList GetAllOperators(CultureInfo cultureInfo)
#else
        public override OperatorsList GetAllOperators(CultureInfo cultureInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            return GetAllOperatorsInternal(cultureInfo, ResourceManager);
        }

        /// <inheritdoc/>
#if NET7_0_OR_GREATER
        public static async Task<OperatorsList> GetAllOperatorsAsync(CultureInfo cultureInfo)
#else
        public override async Task<OperatorsList> GetAllOperatorsAsync(CultureInfo cultureInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            return await Task.Run(() => GetAllOperatorsInternal(cultureInfo, ResourceManager));
        }

        /// <summary>
        /// 通过干员的立绘信息获取包含其立绘等的AssetBundle文件
        /// </summary>
        /// <param name="illustInfo">干员的立绘信息</param>
        /// <returns>一个byte数组,其中包含了AssetBundle文件的数据</returns>
        /// <exception cref="ArgumentException"/>
#if NET7_0_OR_GREATER
        public static byte[] GetAssetBundleFile(OperatorIllustrationInfo illustInfo)
#else
        public byte[] GetAssetBundleFile(OperatorIllustrationInfo illustInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            string name;
            string fileName = illustInfo.ImageCodename.Split('_')[0].Split('#')[0];
            if (illustInfo.Type == OperatorType.Skin)
            {
                name = $"operator_image_skin_{fileName}";
            }
            else
            {
                name = $"operator_image_{fileName}";
            }

            byte[] value = (byte[])ResourceManager.GetObject(name);
            if (value is null)
            {
                throw new ArgumentException($@"使用给定的参数""{illustInfo}""时找不到资源");
            }

            return value;
        }

        private static Operator GetOperatorInternal(string operatorName, CultureInfo cultureInfo, ResourceManager resourceManager)
        {
            byte[] opJson = (byte[])resourceManager.GetObject("operators", cultureInfo);
            Operator op = null;
            using (JsonDocument document = JsonDocument.Parse(opJson))
            {
                JsonElement operatorsElement = document.RootElement.GetProperty("Operators");
                foreach (JsonProperty item in operatorsElement.EnumerateObject())
                {
                    bool enumComplete = false;
                    foreach (JsonProperty item2 in from JsonProperty item2 in item.Value.EnumerateObject()
                                                    where item2.Name == "Name" && item2.Value.GetString() == operatorName
                                                    select item2)
                    {
                        JsonSerializerOptions options = new JsonSerializerOptions()
                        {
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            Converters = { new JsonStringEnumConverter() },
                        };
#if NET6_0_OR_GREATER
                        OperatorSourceGenerationContext context = new OperatorSourceGenerationContext(options);
                        op = item.Value.Deserialize(context.Operator);
#else
                        op = item.Value.Deserialize<Operator>(options);
#endif
                        enumComplete = true;
                        break;
                    }

                    if (enumComplete)
                    {
                        break;
                    }
                }
            }
            return op;
        }

        private static Operator GetOperatorWithCodenameInternal(string operatorImageCodename, CultureInfo cultureInfo,ResourceManager resourceManager)
        {
            byte[] opJson = (byte[])resourceManager.GetObject("operators", cultureInfo);
            Operator op = null;
            using (JsonDocument document = JsonDocument.Parse(opJson))
            {
                JsonElement operatorsElement = document.RootElement.GetProperty("Operators");
                foreach (JsonProperty item in operatorsElement.EnumerateObject())
                {
                    bool enumComplete = false;
                    foreach (JsonProperty item2 in from JsonProperty item2 in item.Value.EnumerateObject()
                                                    where item2.Name == "ImageCodename" && item2.Value.GetString() == operatorImageCodename
                                                    select item2)
                    {
                        JsonSerializerOptions options = new JsonSerializerOptions()
                        {
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            Converters = { new JsonStringEnumConverter() }
                        };
#if NET6_0_OR_GREATER
                        OperatorSourceGenerationContext context = new OperatorSourceGenerationContext(options);
                        op = item.Value.Deserialize(context.Operator);
#else
                        op = item.Value.Deserialize<Operator>(options);
#endif
                        enumComplete = true;
                        break;
                    }

                    if (enumComplete)
                    {
                        break;
                    }
                }
            }
            return op;
        }

        private static OperatorsList GetAllOperatorsInternal(CultureInfo cultureInfo, ResourceManager resourceManager)
        {
            byte[] operators = (byte[])resourceManager.GetObject("operators", cultureInfo);
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                Converters = { new JsonStringEnumConverter() }
            };
#if NET6_0_OR_GREATER
            OperatorsListSourceGenerationContext context = new OperatorsListSourceGenerationContext(options);
            OperatorsList operatorsList = JsonSerializer.Deserialize(operators, context.OperatorsList);
#else
            OperatorsList operatorsList = JsonSerializer.Deserialize<OperatorsList>(operators, options);
#endif
            return operatorsList;
        }
    }

#if NET6_0_OR_GREATER
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Operator))]
    internal partial class OperatorSourceGenerationContext : JsonSerializerContext
    {
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(OperatorsList))]
    internal partial class OperatorsListSourceGenerationContext : JsonSerializerContext
    {
    }
    
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class DictionarySourceGenerationContext : JsonSerializerContext
    {
    }
#endif
}
