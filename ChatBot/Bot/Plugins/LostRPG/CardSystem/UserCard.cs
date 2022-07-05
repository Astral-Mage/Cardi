using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Enums;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem
{
    public class UserCard
    {
        // user stuff
        public string Name { get; protected set; }

        public int UserId { get; set; }

        public string Alias { get; set; }

        public UserStatus Status { get; set; }

        // magic stuff
        public MagicData MagicData { get; set; }

        //roleplay stuff
        public RoleplayData RpData { get; set; }

        public StatData Stats { get; set; }

        public UserCard(string name, string alias, int userId = -1)
        {
            UserId = userId;
            Status = UserStatus.Active;
            Name = name;
            Alias = alias;

            RpData = new RoleplayData();
            MagicData = new MagicData();
        }

        public UserCard(string name)
        {
            Name = name;
            Status = UserStatus.Active;
            Alias = Name;
            UserId = -1;

            RpData = new RoleplayData();
            MagicData = new MagicData();
        }
    }
}
