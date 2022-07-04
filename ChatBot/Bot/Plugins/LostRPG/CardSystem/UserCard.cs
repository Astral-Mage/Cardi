using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Enums;
using ChatBot.Bot.Plugins.LostRPG.MagicSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Magic MainMagic { get; set; }

        //roleplay stuff
        public RoleplayData RpData { get; set; }

        public UserCard(string name, string alias, int userId = -1)
        {
            UserId = userId;
            Status = UserStatus.Active;
            Name = name;
            Alias = alias;

            MainMagic = null;

            RpData = new RoleplayData();
        }

        public UserCard(string name)
        {
            Name = name;
            Status = UserStatus.Active;
            Alias = Name;
            UserId = -1;

            MainMagic = null;

            RpData = new RoleplayData();
        }
    }
}
