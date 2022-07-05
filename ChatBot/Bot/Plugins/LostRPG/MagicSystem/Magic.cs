using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.MagicSystem
{
    public class Magic
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public string Description { get; set; }

        public int MagicId { get; set; }


        public Magic()
        {
            Name = string.Empty;
            Color = "white";
            Description = string.Empty;
            MagicId = -1;
        }
    }
}
