#if NET5_0_OR_GREATER
#pragma warning disable IDE0066
#pragma warning disable IDE0090
#endif

using System;
using System.Collections.Immutable;
using System.Globalization;
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
    /// 为ArknightsResources.Operators.VoiceResources的资源访问提供帮助的结构
    /// </summary>
    public readonly struct OperatorVoiceResourceHelper : IOperatorVoiceGetter
    {
        /// <summary>
        /// 当前ResourceHelper使用的<see cref="System.Resources.ResourceManager"/>
        /// </summary>
        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// 使用指定的参数构造<seealso cref="OperatorVoiceResourceHelper"/>的新实例
        /// </summary>
        /// <param name="resourceManager">该ResourcesHelper使用的<see cref="System.Resources.ResourceManager"/></param>
        /// <exception cref="ArgumentNullException"/>
        public OperatorVoiceResourceHelper(ResourceManager resourceManager)
        {
            ResourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        }

        /// <inheritdoc/>
        public byte[] GetOperatorVoice(OperatorVoiceItem voiceItem)
        {
            byte[] value = GetVoiceAssetBundleFile(voiceItem);
            if (value is null)
            {
                throw new ArgumentException($@"使用给定的参数""{voiceItem}""时找不到资源");
            }

            byte[] voice = AssetBundleHelper.GetOperatorVoice(value, voiceItem);
            return voice;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetOperatorVoiceAsync(OperatorVoiceItem voiceItem)
        {
            OperatorVoiceResourceHelper self = this;
            return await Task.Run(() => self.GetOperatorVoice(voiceItem));
        }

        /// <summary>
        /// 通过干员的语音信息获取包含其语音的AssetBundle文件
        /// </summary>
        /// <param name="voiceItem">干员的语音信息</param>
        /// <returns>一个byte数组,其中包含了AssetBundle文件的数据</returns>
        /// <exception cref="ArgumentException"/>
        public byte[] GetVoiceAssetBundleFile(OperatorVoiceItem voiceItem)
        {
            if (ResourceManager is null)
            {
                throw new InvalidOperationException($"此对象的属性 {nameof(ResourceManager)} 不可为空");
            }

            string name;
            switch (voiceItem.VoiceType)
            {
                default:
                case OperatorVoiceType.ChineseMandarin:
                    name = $"operator_voice_cn_{voiceItem.CharactorCodename}";
                    break;
                case OperatorVoiceType.ChineseRegional:
                    name = $"operator_voice_cn_topolect_{voiceItem.CharactorCodename}";
                    break;
                case OperatorVoiceType.Japanese:
                    name = $"operator_voice_ja_{voiceItem.CharactorCodename}";
                    break;
                case OperatorVoiceType.English:
                    name = $"operator_voice_en_{voiceItem.CharactorCodename}";
                    break;
                case OperatorVoiceType.Korean:
                    name = $"operator_voice_kr_{voiceItem.CharactorCodename}";
                    break;
                case OperatorVoiceType.Italian:
                    name = $"operator_voice_ita_{voiceItem.CharactorCodename}";
                    break;
            }

            byte[] value = (byte[])ResourceManager.GetObject(name);
            if (value is null)
            {
                throw new ArgumentException($@"使用给定的参数""{voiceItem}""时找不到资源");
            }

            return value;
        }
    }
}
