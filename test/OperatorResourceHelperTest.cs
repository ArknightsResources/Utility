using OperatorIllustResources = ArknightsResources.Operators.IllustResources.Properties.Resources;
using OperatorTextResources = ArknightsResources.Operators.TextResources.Properties.Resources;
using OperatorCustomVoiceResources = ArknightsResources.Operators.VoiceResources.Custom.Properties.Resources;
using System.Collections.Immutable;

namespace ArknightsResources.Utility.Test
{
    public class OperatorResourceHelperTest
    {
        private readonly CultureInfo ChineseSimplifiedCultureInfo = new("zh-CN", false);

        [Fact]
        public async void GetAllOperatorTest()
        {
            OperatorTextResourceHelper operatorResourceHelper = new(OperatorTextResources.ResourceManager);
            var value = operatorResourceHelper.GetAllOperators(ChineseSimplifiedCultureInfo);
            var valueAsync = await operatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            Assert.NotEmpty(value);
            Assert.NotEmpty(valueAsync);
        }

        [Fact]
        public async void GetOperatorIllustrationTest()
        {
            OperatorTextResourceHelper textResourceHelper = new(OperatorTextResources.ResourceManager);
            OperatorIllustResourceHelper illustResourceHelper = new(OperatorIllustResources.ResourceManager);
            var list = await textResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    byte[] value = illustResourceHelper.GetOperatorIllustration(illustInfo);
                    Assert.NotEmpty(value);
                }
            }
        }

        [Fact]
        public async void GetOperatorIllustrationReturnImageTest()
        {
            OperatorTextResourceHelper textResourceHelper = new(OperatorTextResources.ResourceManager);
            OperatorIllustResourceHelper illustResourceHelper = new(OperatorIllustResources.ResourceManager);
            var list = await textResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    var value = illustResourceHelper.GetOperatorIllustrationReturnImage(illustInfo);
                    Assert.NotNull(value);
                    Assert.NotEqual(0, value.Height);
                    Assert.NotEqual(0, value.Width);
                }
            }
        }
        
        [Fact]
        public async void GetOperatorSpineAnimationTest()
        {
            OperatorIllustResourceHelper illustResourceHelper = new(OperatorIllustResources.ResourceManager);
            OperatorTextResourceHelper textResourceHelper = new(OperatorTextResources.ResourceManager);
            var list = await textResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    bool isSkin = illustInfo.Type == OperatorType.Skin;
                    string imageCodename = isSkin
                        ? illustInfo.ImageCodename
                        : pair.Value.Codename;

                    OperatorSpineInfo spineInfo = new(OperatorSpineModelSet.Build, imageCodename, isSkin);
                    var value = illustResourceHelper.GetOperatorSpineAnimation(spineInfo);

                    Assert.NotNull(value.Item1);
                    Assert.NotNull(value.Item2);
                    Assert.NotEmpty(value.Item3);
                }
            }
        }

        [Fact]
        public void GetOperatorTest()
        {
            OperatorTextResourceHelper operatorResourceHelper = new(OperatorTextResources.ResourceManager);
            Operator op = operatorResourceHelper.GetOperator("阿米娅", ChineseSimplifiedCultureInfo);
            Assert.Equal("阿米娅", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public void GetOperatorWithCodenameTest()
        {
            OperatorTextResourceHelper operatorResourceHelper = new(OperatorTextResources.ResourceManager);
            Operator op = operatorResourceHelper.GetOperatorWithCodename("gdglow", ChineseSimplifiedCultureInfo);
            Assert.Equal("澄闪", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public async void GetOperatorAsyncTest()
        {
            OperatorTextResourceHelper operatorResourceHelper = new(OperatorTextResources.ResourceManager);
            Operator op = await operatorResourceHelper.GetOperatorAsync("阿米娅", ChineseSimplifiedCultureInfo);
            Assert.Equal("阿米娅", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public async void GetOperatorWithCodenameAsyncTest()
        {
            OperatorTextResourceHelper operatorResourceHelper = new(OperatorTextResources.ResourceManager);
            Operator op = await operatorResourceHelper.GetOperatorWithCodenameAsync("gdglow", ChineseSimplifiedCultureInfo);
            Assert.Equal("澄闪", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public void GetIllustAssetBundleFileTest()
        {
            OperatorIllustResourceHelper operatorResourceHelper = new(OperatorIllustResources.ResourceManager);
            byte[] ab = operatorResourceHelper.GetIllustAssetBundleFile(new OperatorIllustrationInfo(string.Empty, string.Empty, "amiya", OperatorType.Elite0, string.Empty));
            Assert.NotEmpty(ab);
        }

        [Fact]
        public void GetVoiceAssetBundleFileTest()
        {
            OperatorVoiceResourceHelper operatorResourceHelper = new(OperatorCustomVoiceResources.ResourceManager);
            byte[] ab = operatorResourceHelper.GetVoiceAssetBundleFile(new OperatorVoiceItem("chyue", "CN_025", "作战中1", "形不成形，意不在意，再去练练吧。", OperatorVoiceType.ChineseRegional));
            Assert.NotEmpty(ab);
        }

        [Fact]
        public void GetOperatorCodenameMappingTest()
        {
            OperatorTextResourceHelper operatorResourceHelper = new(OperatorTextResources.ResourceManager);
            ImmutableDictionary<string, string> mapping = operatorResourceHelper.GetOperatorCodenameMapping(ChineseSimplifiedCultureInfo);
            Assert.NotEmpty(mapping);
        }

        [Fact]
        public void GetOperatorSkinMappingTest()
        {
            OperatorTextResourceHelper operatorResourceHelper = new(OperatorTextResources.ResourceManager);
            ImmutableDictionary<string, string[]> mapping = operatorResourceHelper.GetOperatorSkinMapping();
            Assert.NotEmpty(mapping);
        }

        [Fact]
        public void GetAllOperatorVoiceInfosTest()
        {
            OperatorTextResourceHelper operatorResourceHelper = new(OperatorTextResources.ResourceManager);
            ImmutableDictionary<string, OperatorVoiceInfo[]> mapping = operatorResourceHelper.GetAllOperatorVoiceInfos(AvailableCultureInfos.ChineseSimplifiedCultureInfo);
            Assert.NotEmpty(mapping);
        }
    }
}