using Accord;
using ChatBot.Bot.Plugins.LostRPG.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class Calling : BaseCustomization
    {

        public Calling()
        {
            Customization = Data.Enums.CustomizationTypes.Calling;
        }
    }
}
