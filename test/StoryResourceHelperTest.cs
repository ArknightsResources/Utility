using StoryVideoResource = ArknightsResources.Stories.VideoResources.Properties.Resources;

namespace ArknightsResources.Utility.Test
{
    public class StoryResourceHelperTest
    {
        private readonly CultureInfo ChineseSimplifiedCultureInfo = new("zh-CN", false);

        [Fact]
        public void GetVideo_Success()
        {
            StoryResourceHelper storyResourceHelper = new(StoryVideoResource.ResourceManager);
            byte[] video = storyResourceHelper.GetVideo("video_event_ic01", ChineseSimplifiedCultureInfo);
            Assert.True(video != null);
        }

        //[Fact]
        //public void GetStoryRawText_Success()
        //{
        //    StoryResourceHelper storyResourceHelper = new(StoryResource.ResourceManager);
        //    string text = storyResourceHelper.GetStoryRawText("story_event_whoisreal_8_end", ChineseSimplifiedCultureInfo);
        //    Assert.True(!string.IsNullOrWhiteSpace(text));
        //}
    }
}