using Newtonsoft.Json;

namespace WhoWantsToBeMillionaire
{
    class AchievementText
    {
        [JsonConstructor]
        public AchievementText(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public string Title {  get; }
        public string Description { get; }

        public void GetData(out string title, out string description)
        {
            title = Title;
            description = Description;
        }
    }
}
