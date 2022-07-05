using ChatBot.Bot.Plugins.LostRPG.MagicSystem;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class MagicData
    {
        public Magic PrimaryMagic { get; set; }

        public Magic SecondaryMagic { get; set; }

        public int UserId { get; set; }

        public MagicData()
        {
            UserId = -1;
            PrimaryMagic = null;
            SecondaryMagic = null;
        }
    }
}
