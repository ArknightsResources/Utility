#if NET6_0_OR_GREATER
#pragma warning disable IDE0090
#endif

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using ArknightsResources.CustomResourceHelpers;
using ArknightsResources.Operators.Models;

namespace ArknightsResources.Utility
{
    /// <summary>
    /// 为ArknightsResources.Operators.TextResources的资源访问提供帮助的结构
    /// </summary>
    public readonly struct OperatorTextResourceHelper : IOperatorInfoGetter
    {
        /// <summary>
        /// 当前ResourceHelper使用的<see cref="System.Resources.ResourceManager"/>
        /// </summary>
        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// 使用指定的参数构造<seealso cref="OperatorTextResourceHelper"/>的新实例
        /// </summary>
        /// <param name="resourceManager">该ResourcesHelper使用的<see cref="System.Resources.ResourceManager"/></param>
        public OperatorTextResourceHelper(ResourceManager resourceManager)
        {
            ResourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        }

        /// <summary>
        /// 获取干员内部代号与干员名称的映射表
        /// </summary>
        /// <param name="cultureInfo">干员名称所用语言</param>
        /// <returns>Key为干员内部代号,Value为干员名称的Dictionary&lt;string, string&gt;</returns>
        public ImmutableDictionary<string, string> GetOperatorCodenameMapping(CultureInfo cultureInfo)
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            byte[] stringByteArray = (byte[])ResourceManager.GetObject("operator_image_codename_mapping", cultureInfo);
#if NET6_0_OR_GREATER
            ImmutableDictionary<string, string> dict = JsonSerializer.Deserialize(stringByteArray, ImmutableDictionaryStrStrSourceGenerationContext.Default.ImmutableDictionaryStringString);
#else
            ImmutableDictionary<string, string> dict = JsonSerializer.Deserialize<ImmutableDictionary<string, string>>(stringByteArray);
#endif
            return dict;
        }

        /// <summary>
        /// 获取干员内部代号与干员皮肤列表的映射表
        /// </summary>
        /// <returns>Key为干员内部代号,Value为干员皮肤列表的Dictionary&lt;string, IList&lt;string&gt;&gt;</returns>
        public ImmutableDictionary<string, string[]> GetOperatorSkinMapping()
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            byte[] skinDict = (byte[])ResourceManager.GetObject("operator_skin_codename");
#if NET6_0_OR_GREATER
            ImmutableDictionary<string, string[]> dict = JsonSerializer.Deserialize(skinDict, ImmutableDictionaryStrStrArraySourceGenerationContext.Default.ImmutableDictionaryStringStringArray);
#else
            ImmutableDictionary<string, string[]> dict = JsonSerializer.Deserialize<ImmutableDictionary<string, string[]>>(skinDict);
#endif
            return dict;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        public Operator GetOperator(string operatorName, CultureInfo cultureInfo)
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
        public async Task<Operator> GetOperatorAsync(string operatorName, CultureInfo cultureInfo)
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
                OperatorTextResourceHelper self = this;
                return await Task.Run(() => GetOperatorInternal(operatorName, cultureInfo, self.ResourceManager));
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
        public Operator GetOperatorWithCodename(string codename, CultureInfo cultureInfo)
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
        public async Task<Operator> GetOperatorWithCodenameAsync(string codename, CultureInfo cultureInfo)
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
                OperatorTextResourceHelper self = this;
                return await Task.Run(() => GetOperatorWithCodenameInternal(codename, cultureInfo, self.ResourceManager));
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException($"参数\"{codename}\"无效", ex);
            }
        }

        /// <inheritdoc/>
        public ImmutableDictionary<string, Operator> GetAllOperators(CultureInfo cultureInfo)
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }

            return GetAllOperatorsInternal(cultureInfo, ResourceManager);
        }

        /// <inheritdoc/>
        public async Task<ImmutableDictionary<string, Operator>> GetAllOperatorsAsync(CultureInfo cultureInfo)
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此类的属性 {nameof(ResourceManager)} 不可为空");
            }
            OperatorTextResourceHelper self = this;
            return await Task.Run(() => GetAllOperatorsInternal(cultureInfo, self.ResourceManager));
        }

        private static Operator GetOperatorInternal(string operatorName, CultureInfo cultureInfo, ResourceManager resourceManager)
        {
            byte[] opJson = (byte[])resourceManager.GetObject("operators", cultureInfo);

            using (JsonDocument document = JsonDocument.Parse(opJson))
            {
                foreach (JsonProperty item in document.RootElement.EnumerateObject())
                {
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
                        return item.Value.Deserialize(context.Operator);
#else
                        return item.Value.Deserialize<Operator>(options);
#endif
                    }
                }
            }

            throw new ArgumentException($"使用指定的参数{operatorName},{cultureInfo}时找不到指定的干员");
        }

        private static Operator GetOperatorWithCodenameInternal(string codename, CultureInfo cultureInfo, ResourceManager resourceManager)
        {
            byte[] opJson = (byte[])resourceManager.GetObject("operators", cultureInfo);
            using (JsonDocument document = JsonDocument.Parse(opJson))
            {
                foreach (JsonProperty item in document.RootElement.EnumerateObject())
                {
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
                        Operator op = item.Value.Deserialize(context.Operator);
#else
                        Operator op = item.Value.Deserialize<Operator>(options);
#endif
                        return op;
                    }
                }
            }

            throw new ArgumentException($"使用指定的参数{codename},{cultureInfo}时找不到指定的干员");
        }

        private static ImmutableDictionary<string, Operator> GetAllOperatorsInternal(CultureInfo cultureInfo, ResourceManager resourceManager)
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
            OperatorsDictSourceGenerationContext context = new OperatorsDictSourceGenerationContext(options);
            ImmutableDictionary<string, Operator> operatorsDict = JsonSerializer.Deserialize(operators, context.ImmutableDictionaryStringOperator);
#else
            ImmutableDictionary<string, Operator> operatorsDict = JsonSerializer.Deserialize<ImmutableDictionary<string, Operator>>(operators, options);
#endif
            return operatorsDict;
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
    [JsonSerializable(typeof(ImmutableDictionary<string, Operator>))]
    internal partial class OperatorsDictSourceGenerationContext : JsonSerializerContext
    {
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ImmutableDictionary<string, string>))]
    internal partial class ImmutableDictionaryStrStrSourceGenerationContext : JsonSerializerContext
    {
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ImmutableDictionary<string, string[]>))]
    internal partial class ImmutableDictionaryStrStrArraySourceGenerationContext : JsonSerializerContext
    {
    }
#endif
}
