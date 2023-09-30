using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    [Serializable]
    public class CustomizationDetails
    {
        public int cid { get; set; }
        public int currentlevel { get; set; }
        public bool isactive { get; set; }

        public CustomizationDetails()
        {
            cid = -1;
            currentlevel = 1;
            isactive = false;
        }
    }
}
