using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.DungeonDiver.Minigames.PlayingCardGame
{
    public enum Rank
    {
        [Description("🤡")]
        Joker = 0,

        [Description("2")]
        Two = 2,

        [Description("3")]
        Three,

        [Description("4")]

        Four,
        [Description("5")]
        Five,

        [Description("6")]
        Six,

        [Description("7")]
        Seven,

        [Description("8")]
        Eight,

        [Description("9")]
        Nine,

        [Description("10")]
        Ten,

        [Description("J")]
        Jack,

        [Description("Q")]
        Queen,

        [Description("K")]
        King,

        [Description("A")]
        Ace,
    }

    public enum Suit
    {
        [Description("[color=black][color=white]♠[/color][/color]")]
        Spades,

        [Description("[color=orange]♣[/color]")]
        Clubs,

        [Description("[color=blue]♦[/color]")]
        Diamonds,

        [Description("[color=red]♥[/color]")]
        Hearts,
    }
}