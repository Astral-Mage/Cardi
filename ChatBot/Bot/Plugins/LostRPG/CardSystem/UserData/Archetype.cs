using Accord;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class Archetype : BaseCustomization
    {
        public Archetype() : base()
        {
            Customization = CustomizationTypes.Archetype;
        }
    }
}