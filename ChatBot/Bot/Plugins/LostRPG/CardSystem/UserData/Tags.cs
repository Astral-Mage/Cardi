
namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class Tags
    {
        public int TagId { get; set; }
        public string Name { get; set; }

        public Tags()
        {
            TagId = 0;
            Name = string.Empty;
        }

        public Tags(string str)
        {
            TagId = 0;
            Name = str;
        }

        public Tags(string str, int id)
        {
            TagId = id;
            Name = str;
        }
    }
}
