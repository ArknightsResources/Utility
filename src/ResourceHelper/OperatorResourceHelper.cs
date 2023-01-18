#if NET6_0_OR_GREATER
#pragma warning disable IDE0090
#endif
#pragma warning disable IDE0019

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;
using ArknightsResources.CustomResourceHelpers;
using ArknightsResources.Operators.Models;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 为ArknightsResources.Operators.Resources的资源访问提供帮助的类
    /// </summary>
    public class OperatorResourceHelper : IOperatorInfoGetter, IOperatorIllustrationGetter, IOperatorSpineAnimationGetter
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
#else
        public ResourceManager ResourceManager { get; set; }
#endif

        /// <summary>
        /// 获取干员内部代号与干员名称的映射表
        /// </summary>
        /// <param name="cultureInfo">干员名称所用语言</param>
        /// <returns>Key为干员内部代号,Value为干员名称的Dictionary&lt;string, string&gt;</returns>
#if NET7_0_OR_GREATER
        public static Dictionary<string, string> GetOperatorCodenameMapping(CultureInfo cultureInfo)
#else
        public Dictionary<string, string> GetOperatorCodenameMapping(CultureInfo cultureInfo)
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

        /// <summary>
        /// 获取干员内部代号与干员皮肤列表的映射表
        /// </summary>
        /// <returns>Key为干员内部代号,Value为干员皮肤列表的Dictionary&lt;string, IList&lt;string&gt;&gt;</returns>
#if NET7_0_OR_GREATER
        public static Dictionary<string, IList<string>> GetOperatorSkinMapping()
#else
        public Dictionary<string, IList<string>> GetOperatorSkinMapping()
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            string skinList = ResourceManager.GetString("operator_skin_codename");
            Dictionary<string, IList<string>> dict = new Dictionary<string, IList<string>>(250);
            StringReader reader = new StringReader(skinList);
            while (reader.Peek() != -1)
            {
                string line = reader.ReadLine();
                string[] val = line.Split('_');
                if (!dict.ContainsKey(val[0]))
                {
                    dict[val[0]] = new List<string>(5) { line };
                }
                else
                {
                    dict[val[0]].Add(line);
                }
            }
            return dict;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="MissingManifestResourceException"/>
        /// <exception cref="MissingSatelliteAssemblyException"/>
#if NET7_0_OR_GREATER
        public static byte[] GetOperatorIllustration(OperatorIllustrationInfo illustInfo)
#else
        public byte[] GetOperatorIllustration(OperatorIllustrationInfo illustInfo)
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

        /// <summary>
        /// 通过干员的立绘信息获取其立绘
        /// </summary>
        /// <param name="illustInfo">干员的立绘信息</param>
        /// <returns>一个<see cref="Image{Bgra32}"/>,其中包含了干员的立绘</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="MissingManifestResourceException"/>
        /// <exception cref="MissingSatelliteAssemblyException"/>
#if NET7_0_OR_GREATER
        public static Image<Bgra32> GetOperatorIllustrationReturnImage(OperatorIllustrationInfo illustInfo)
#else
        public Image<Bgra32> GetOperatorIllustrationReturnImage(OperatorIllustrationInfo illustInfo)
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
#if NET7_0_OR_GREATER
        public static async Task<Image<Bgra32>> GetOperatorIllustrationReturnImageAsync(OperatorIllustrationInfo illustInfo)
#else
        public async Task<Image<Bgra32>> GetOperatorIllustrationReturnImageAsync(OperatorIllustrationInfo illustInfo)
#endif
        {
            return await Task.Run(() => GetOperatorIllustrationReturnImage(illustInfo));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="MissingManifestResourceException"/>
        /// <exception cref="MissingSatelliteAssemblyException"/>
#if NET7_0_OR_GREATER
        public static (TextReader, TextReader, byte[]) GetOperatorSpineAnimation(OperatorSpineInfo spineInfo)
#else
        public (TextReader, TextReader, byte[]) GetOperatorSpineAnimation(OperatorSpineInfo spineInfo)
#endif
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
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
#if NET7_0_OR_GREATER
        public static async Task<(TextReader, TextReader, byte[])> GetOperatorSpineAnimationAsync(OperatorSpineInfo spineInfo)
#else
        public async Task<(TextReader, TextReader, byte[])> GetOperatorSpineAnimationAsync(OperatorSpineInfo spineInfo)
#endif
        {
            return await Task.Run(() => GetOperatorSpineAnimation(spineInfo));
        }

        /// <inheritdoc/>
#if NET7_0_OR_GREATER
        public static async Task<byte[]> GetOperatorIllustrationAsync(OperatorIllustrationInfo illustInfo)
#else
        public async Task<byte[]> GetOperatorIllustrationAsync(OperatorIllustrationInfo illustInfo)
#endif
        {
            return await Task.Run(() => GetOperatorIllustration(illustInfo));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
#if NET7_0_OR_GREATER
        public static Operator GetOperator(string operatorName, CultureInfo cultureInfo)
#else
        public Operator GetOperator(string operatorName, CultureInfo cultureInfo)
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
        public async Task<Operator> GetOperatorAsync(string operatorName, CultureInfo cultureInfo)
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
        public static Operator GetOperatorWithCodename(string codename, CultureInfo cultureInfo)
#else
        public Operator GetOperatorWithCodename(string codename, CultureInfo cultureInfo)
#endif
        {
            if (string.IsNullOrWhiteSpace(codename))
            {
                throw new ArgumentException($"“{nameof(codename)}”不能为 null 或空白。", nameof(codename));
            }

            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            try
            {
                return GetOperatorWithCodenameInternal(codename, cultureInfo, ResourceManager);
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException($"参数\"{codename}\"无效", ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException($"提供的语言文化\"{cultureInfo}\"无效", ex);
            }
        }

        /// <inheritdoc/>
#if NET7_0_OR_GREATER
        public static async Task<Operator> GetOperatorWithCodenameAsync(string codename, CultureInfo cultureInfo)
#else
        public async Task<Operator> GetOperatorWithCodenameAsync(string codename, CultureInfo cultureInfo)
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
                return await Task.Run(() => GetOperatorWithCodenameInternal(codename, cultureInfo, ResourceManager));
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException($"参数\"{codename}\"无效", ex);
            }
        }

        /// <inheritdoc/>
#if NET7_0_OR_GREATER
        public static OperatorsList GetAllOperators(CultureInfo cultureInfo)
#else
        public OperatorsList GetAllOperators(CultureInfo cultureInfo)
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
        public async Task<OperatorsList> GetAllOperatorsAsync(CultureInfo cultureInfo)
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
                        //支持IL裁剪
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

        private static Operator GetOperatorWithCodenameInternal(string codename, CultureInfo cultureInfo,ResourceManager resourceManager)
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
                                                    where item2.Name == nameof(Operator.Codename) && item2.Value.GetString() == codename
                                                    select item2)
                    {
                        JsonSerializerOptions options = new JsonSerializerOptions()
                        {
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            Converters = { new JsonStringEnumConverter() }
                        };
#if NET6_0_OR_GREATER
                        //支持IL裁剪
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
            //支持IL裁剪
            OperatorsListSourceGenerationContext context = new OperatorsListSourceGenerationContext(options);
            OperatorsList operatorsList = JsonSerializer.Deserialize(operators, context.OperatorsList);
#else
            OperatorsList operatorsList = JsonSerializer.Deserialize<OperatorsList>(operators, options);
#endif
            return operatorsList;
        }
    }

#if NET6_0_OR_GREATER
    //这些类提供System.Text.Json源代码生成器的支持

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
