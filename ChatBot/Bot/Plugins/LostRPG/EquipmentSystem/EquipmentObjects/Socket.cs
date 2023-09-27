using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using System;

namespace ChatBot.Bot.Plugins.LostRPG.EquipmentSystem.EquipmentObjects
{
    [Serializable]
    public class Socket
    {
        public int SocketLevel { get; set; }

        public SocketTypes SocketType { get; set; }

        public string NameOverride { get; set; }

        public string SocketDescription { get; set; }

        public string SocketRaritySymbol { get; set; }

        public string SocketRarityColor { get; set; }

        public int BaseLevelUpCost { get; set; }

        public int MaxLevel { get; set; }

        public StatData Stats { get; set; }

        public string GetRarityString()
        {
            return $"[color=cyan][{SocketLevel}][/color][b][color={SocketRarityColor}]{SocketRaritySymbol}[/color][/b]";
        }

        public virtual string GetShortDescription() { return string.Empty; }

        public virtual string GetName() { return string.Empty; }

        public Socket()
        {
            NameOverride = "";
            SocketDescription = string.Empty;
            SocketLevel = 1;
            SocketType = SocketTypes.Passive;
            SocketRarityColor = "white";
            SocketRaritySymbol = "💠";
            BaseLevelUpCost = 55;
            MaxLevel = 3;
            Stats = new StatData();
        }

        public virtual string LevelUp() { return string.Empty; }
    }
}