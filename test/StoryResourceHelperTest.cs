using StoryResource = ArknightsResources.Stories.Resources.Properties.Resources;

namespace ArknightsResources.Utility
{
    public class StoryResourceHelperTest
    {
        private readonly CultureInfo ChineseSimplifiedCultureInfo = new("zh-CN", false);

        public StoryResourceHelperTest()
        {
            StoryResourceHelper.ResourceManager = StoryResource.ResourceManager;
        }

        [Fact]
        public void GetVideo_Success()
        {
            byte[] video = StoryResourceHelper.GetVideo("video_event_ic01", ChineseSimplifiedCultureInfo);
            Assert.True(video != null);
        }

        [Fact]
        public void GetStoryRawText_Success()
        {
            string text = StoryResourceHelper.GetStoryRawText("story_event_whoisreal_8_end", ChineseSimplifiedCultureInfo);
            Assert.True(!string.IsNullOrWhiteSpace(text));
        }
    }
}