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
        public void GetOperatorImageTest()
        {
            OperatorIllustrationInfo operatorInfo = new(string.Empty, string.Empty, "lionhd_snow", OperatorType.Skin, string.Empty);
            byte[] value = OperatorResourceHelper.GetOperatorImage(operatorInfo);
            Assert.NotEmpty(value);
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
            Operator op = OperatorResourceHelper.GetOperatorWithImageCodename("gdglow", ChineseSimplifiedCultureInfo);
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
            Operator op = await OperatorResourceHelper.GetOperatorWithImageCodenameAsync("gdglow", ChineseSimplifiedCultureInfo);
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
        public void GetOperatorImageMappingTest()
        {
            Dictionary<string, string> mapping = OperatorResourceHelper.GetOperatorImageCodenameMapping(ChineseSimplifiedCultureInfo);
            Assert.NotEmpty(mapping);
        }
    }
}