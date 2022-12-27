using OperatorResources = ArknightsResources.Operators.Resources.Properties.Resources;

namespace ArknightsResources.Utility.Test
{
    public class OperatorResourceHelperTest
    {
        private readonly CultureInfo ChineseSimplifiedCultureInfo = new("zh-CN", false);

        public OperatorResourceHelperTest()
        {
            OperatorResourceHelper.ResourceManager = OperatorResources.ResourceManager;
        }

        [Fact]
        public async void GetAllOperatorTest()
        {
            OperatorsList value = OperatorResourceHelper.GetAllOperators(ChineseSimplifiedCultureInfo);
            OperatorsList valueAsync = await OperatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            Assert.NotEmpty(value.Operators);
            Assert.NotEmpty(valueAsync.Operators);
        }

        [Fact]
        public async void GetOperatorIllustrationTest()
        {
            OperatorsList list = await OperatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list.Operators)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    byte[] value = OperatorResourceHelper.GetOperatorIllustration(illustInfo);
                    Assert.NotEmpty(value);
                }
            }
        }

        [Fact]
        public async void GetOperatorIllustrationReturnImageTest()
        {
            OperatorsList list = await OperatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list.Operators)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    var value = OperatorResourceHelper.GetOperatorIllustrationReturnImage(illustInfo);
                    Assert.NotNull(value);
                    Assert.NotEqual(0, value.Height);
                    Assert.NotEqual(0, value.Width);
                }
            }
        }
        
        [Fact]
        public async void GetOperatorSpineAnimationTest()
        {
            OperatorsList list = await OperatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list.Operators)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    bool isSkin = illustInfo.Type == OperatorType.Skin;
                    string imageCodename = isSkin
                        ? illustInfo.ImageCodename
                        : pair.Value.Codename;

                    OperatorSpineInfo spineInfo = new(OperatorSpineModelSet.Build, imageCodename, isSkin);
                    var value = OperatorResourceHelper.GetOperatorSpineAnimation(spineInfo);

                    Assert.NotNull(value.Item1);
                    Assert.NotNull(value.Item2);
                    Assert.NotEmpty(value.Item3);
                }
            }
        }

        [Fact]
        public void GetOperatorTest()
        {
            Operator op = OperatorResourceHelper.GetOperator("阿米娅", ChineseSimplifiedCultureInfo);
            Assert.Equal("阿米娅", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public void GetOperatorWithCodenameTest()
        {
            Operator op = OperatorResourceHelper.GetOperatorWithCodename("gdglow", ChineseSimplifiedCultureInfo);
            Assert.Equal("澄闪", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public async void GetOperatorAsyncTest()
        {
            Operator op = await OperatorResourceHelper.GetOperatorAsync("阿米娅", ChineseSimplifiedCultureInfo);
            Assert.Equal("阿米娅", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public async void GetOperatorWithCodenameAsyncTest()
        {
            Operator op = await OperatorResourceHelper.GetOperatorWithCodenameAsync("gdglow", ChineseSimplifiedCultureInfo);
            Assert.Equal("澄闪", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public void GetAssetBundleFileTest()
        {
            byte[] ab = OperatorResourceHelper.GetAssetBundleFile(new OperatorIllustrationInfo(string.Empty, string.Empty, "amiya", OperatorType.Elite0, string.Empty));
            Assert.NotEmpty(ab);
        }

        [Fact]
        public void GetOperatorCodenameMappingTest()
        {
            Dictionary<string, string> mapping = OperatorResourceHelper.GetOperatorCodenameMapping(ChineseSimplifiedCultureInfo);
            Assert.NotEmpty(mapping);
        }

        [Fact]
        public void GetOperatorSkinMappingTest()
        {
            Dictionary<string, IList<string>> mapping = OperatorResourceHelper.GetOperatorSkinMapping();
            Assert.NotEmpty(mapping);
        }
    }
}