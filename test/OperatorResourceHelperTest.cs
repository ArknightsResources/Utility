using OperatorResources = ArknightsResources.Operators.IllustResources.Properties.Resources;

namespace ArknightsResources.Utility.Test
{
    public class OperatorResourceHelperTest
    {
        private readonly CultureInfo ChineseSimplifiedCultureInfo = new("zh-CN", false);

        [Fact]
        public async void GetAllOperatorTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            OperatorsList value = operatorResourceHelper.GetAllOperators(ChineseSimplifiedCultureInfo);
            OperatorsList valueAsync = await operatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            Assert.NotEmpty(value.Operators);
            Assert.NotEmpty(valueAsync.Operators);
        }

        [Fact]
        public async void GetOperatorIllustrationTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            OperatorsList list = await operatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list.Operators)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    byte[] value = operatorResourceHelper.GetOperatorIllustration(illustInfo);
                    Assert.NotEmpty(value);
                }
            }
        }

        [Fact]
        public async void GetOperatorIllustrationReturnImageTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            OperatorsList list = await operatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list.Operators)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    var value = operatorResourceHelper.GetOperatorIllustrationReturnImage(illustInfo);
                    Assert.NotNull(value);
                    Assert.NotEqual(0, value.Height);
                    Assert.NotEqual(0, value.Width);
                }
            }
        }
        
        [Fact]
        public async void GetOperatorSpineAnimationTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            OperatorsList list = await operatorResourceHelper.GetAllOperatorsAsync(ChineseSimplifiedCultureInfo);
            foreach (var pair in list.Operators)
            {
                foreach (var illustInfo in pair.Value.Illustrations)
                {
                    bool isSkin = illustInfo.Type == OperatorType.Skin;
                    string imageCodename = isSkin
                        ? illustInfo.ImageCodename
                        : pair.Value.Codename;

                    OperatorSpineInfo spineInfo = new(OperatorSpineModelSet.Build, imageCodename, isSkin);
                    var value = operatorResourceHelper.GetOperatorSpineAnimation(spineInfo);

                    Assert.NotNull(value.Item1);
                    Assert.NotNull(value.Item2);
                    Assert.NotEmpty(value.Item3);
                }
            }
        }

        [Fact]
        public void GetOperatorTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            Operator op = operatorResourceHelper.GetOperator("阿米娅", ChineseSimplifiedCultureInfo);
            Assert.Equal("阿米娅", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public void GetOperatorWithCodenameTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            Operator op = operatorResourceHelper.GetOperatorWithCodename("gdglow", ChineseSimplifiedCultureInfo);
            Assert.Equal("澄闪", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public async void GetOperatorAsyncTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            Operator op = await operatorResourceHelper.GetOperatorAsync("阿米娅", ChineseSimplifiedCultureInfo);
            Assert.Equal("阿米娅", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public async void GetOperatorWithCodenameAsyncTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            Operator op = await operatorResourceHelper.GetOperatorWithCodenameAsync("gdglow", ChineseSimplifiedCultureInfo);
            Assert.Equal("澄闪", op.Name);
            Assert.NotEmpty(op.Profiles);
            Assert.NotEqual(0, op.Birthday!.Value.Month);
            Assert.NotEqual(0, op.Birthday.Value.Day);
        }

        [Fact]
        public void GetAssetBundleFileTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            byte[] ab = operatorResourceHelper.GetIllustAssetBundleFile(new OperatorIllustrationInfo(string.Empty, string.Empty, "amiya", OperatorType.Elite0, string.Empty));
            Assert.NotEmpty(ab);
        }

        [Fact]
        public void GetOperatorCodenameMappingTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            Dictionary<string, string> mapping = operatorResourceHelper.GetOperatorCodenameMapping(ChineseSimplifiedCultureInfo);
            Assert.NotEmpty(mapping);
        }

        [Fact]
        public void GetOperatorSkinMappingTest()
        {
            OperatorResourceHelper operatorResourceHelper = new(OperatorResources.ResourceManager);
            Dictionary<string, IList<string>> mapping = operatorResourceHelper.GetOperatorSkinMapping();
            Assert.NotEmpty(mapping);
        }
    }
}