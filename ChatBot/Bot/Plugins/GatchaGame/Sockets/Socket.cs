using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Sockets
{
    [Serializable]
    public abstract class Socket
    {
        public RarityTypes SocketRarity;
        public SocketTypes SocketType;

        public string NameOverride;
        public string SocketDescription;

        public string SocketRaritySymbol;
        public string SocketRarityColor;

        public int BaseLevelUpCost;
        public RarityTypes MaxRarity;

        public Dictionary<StatTypes, int> StatModifiers;

        public string GetRarityString()
        {
            return $"[color=cyan][{(int)SocketRarity}][/color][b][color={SocketRarityColor}]{SocketRaritySymbol}[/color][/b]";
        }

        public abstract string GetShortDescription();

        public abstract string GetName();

        public Socket()
        {
            NameOverride = "";
            SocketDescription = string.Empty;
            SocketRarity = RarityTypes.One;
            SocketType = SocketTypes.Passive;
            StatModifiers = new Dictionary<StatTypes, int>();
            SocketRarityColor = "white";
            SocketRaritySymbol = "💠";
            BaseLevelUpCost = 55;
            MaxRarity = RarityTypes.Thirty;
        }

        public abstract string LevelUp();
    }
}