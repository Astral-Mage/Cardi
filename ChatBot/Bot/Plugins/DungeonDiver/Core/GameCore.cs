using ChatApi;
using ChatBot.Bot.Plugins.DungeonDiver.Minigames.PlayingCardGame;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins
{
    public partial class GameCore : PluginBase
    {
        readonly Random rng                              = new Random();
        public int BaseDiveCooldown             = 90;
        public double CdReductionPerChar        = 0.5;
        readonly string xpcolor                          = "pink";
        readonly string progcolor                        = "green";
        readonly string lvlcolor                         = "cyan";
        readonly string goldcolor                        = "yellow";
        readonly string enemycolor                       = "red";
        readonly BotCore Bot                             = null;

        /// <summary>
        /// creates a list of valid commands for this plugin
        /// </summary>
        /// <returns></returns>
        public List<Command> GetCommandList()
        {
            return new List<Command>
            {
                new Command(CommandStrings.Help, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of help options"),
                new Command(CommandStrings.Create, BotCommandRestriction.Both, CommandSecurity.None, "creates a new character"),
                new Command(CommandStrings.Dive, BotCommandRestriction.Both, CommandSecurity.None, "dives into the dungeon"),
                new Command(CommandStrings.Prog, BotCommandRestriction.Both, CommandSecurity.None, "shows the current floor's progress"),
                new Command(CommandStrings.Gift, BotCommandRestriction.Message, CommandSecurity.None, "gifts gold to a player"),
                new Command(CommandStrings.Set, BotCommandRestriction.Whisper, CommandSecurity.None, "sets various card things", "help"),
                new Command(CommandStrings.Upgrade, BotCommandRestriction.Whisper, CommandSecurity.None, "upgrades your gear for gold", "help"),
                new Command(CommandStrings.Card, BotCommandRestriction.Both, CommandSecurity.None, "shows off your condensed character card"),
                new Command(CommandStrings.Cooldown, BotCommandRestriction.Both, CommandSecurity.None, "shows your current dive cooldown"),
                new Command(CommandStrings.Gold, BotCommandRestriction.Both, CommandSecurity.None, "shows your current rank in the leaderboard"),
                new Command(CommandStrings.Leaderboard, BotCommandRestriction.Both, CommandSecurity.None, "shows the monster leaderboard"),
                new Command(CommandStrings.Mute, BotCommandRestriction.Both, CommandSecurity.Ops, "mutes all in-room conversation if enabled"),
                new Command(CommandStrings.Smite, BotCommandRestriction.Both, CommandSecurity.Ops, "subtracts gold from a player"),
                new Command(CommandStrings.Execute, BotCommandRestriction.Both, CommandSecurity.Ops, "deletes a player's card", "name"),
                new Command(CommandStrings.BaseCooldown, BotCommandRestriction.Both, CommandSecurity.Ops, "changes the base cooldown"),
                new Command(CommandStrings.AddFloor, BotCommandRestriction.Whisper, CommandSecurity.Ops, "adds a new dungeon floor"),
                new Command(CommandStrings.Deck, BotCommandRestriction.Both, CommandSecurity.None, "creates a new deck of playing cards with specified players"),
                new Command(CommandStrings.JoinChannel, BotCommandRestriction.Whisper, CommandSecurity.Ops, "joins a channel"),
                new Command(CommandStrings.Roll, BotCommandRestriction.Whisper, CommandSecurity.Ops, "spends gold to roll in the gatcha"),
            };
        }

        /// <summary>
        /// responsible for handling commands any user can access
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="channel">the source channel</param>
        /// <param name="sendingUser">the user sending the command</param>
        /// <returns></returns>
        public bool HandleBaseCommands(string command, string channel, string sendingUser)
        {
            switch (command)
            {
                case CommandStrings.Help:
                    {
                        HelpAction(sendingUser);
                    }
                    return true;
                case CommandStrings.Create:
                    {
                        CreateAction(channel, sendingUser);
                    }
                    return true;
            }

            return false;
        }

        /// <summary>
        /// responsible for handling commands any op can access
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="channel">the source channel</param>
        /// <param name="isOp">if the user is an op</param>
        /// <param name="message">the cleaned up message</param>
        /// <param name="pc">player card for the sending user</param>
        /// <returns></returns>
        public bool HandleOpCommands(string command, string channel, bool isOp, string message, PlayerCard pc)
        {
            switch (command)
            {
                case CommandStrings.JoinChannel:
                    {
                        if (!isOp) { SystemController.Instance.Respond(channel, $"You need to be an Operator to use this command, {pc.name}. The authorities have been notified of your attempt, however.", pc.name); return true; }
                        JoinChannelAction(pc, message);
                    }
                    return true;
                case CommandStrings.LeaveChannel:
                    {
                        if (!isOp) { SystemController.Instance.Respond(channel, $"You need to be an Operator to use this command, {pc.name}. The authorities have been notified of your attempt, however.", pc.name); return true; }
                        LeaveChannelAction(pc, message);
                    }
                    return true;
                case CommandStrings.AddFloor:
                    {
                        if (!isOp) { SystemController.Instance.Respond(channel, $"You need to be an Operator to use this command, {pc.name}. The authorities have been notified of your attempt, however.", pc.name); return true; }
                        AddFloorAction(pc, message);
                    }
                    return true;
                case CommandStrings.Mute:
                    {
                        if (!isOp) { SystemController.Instance.Respond(channel, $"You need to be an Operator to use this command, {pc.name}. The authorities have been notified of your attempt, however.", pc.name); return true; }
                        MuteAction(channel, message, pc);
                    }
                    return true;
                case CommandStrings.Execute:
                    {
                        if (!isOp) { SystemController.Instance.Respond(channel, $"You need to be an Operator to use this command, {pc.name}. The authorities have been notified of your attempt, however.", pc.name); return true; }
                        ExecuteAction(channel, message, pc);
                    }
                    return true;
                case CommandStrings.BaseCooldown:
                    {
                        if (!isOp) { SystemController.Instance.Respond(channel, $"You need to be an Operator to use this command, {pc.name}. The authorities have been notified of your attempt, however.", pc.name); return true; }
                        BaseCooldownAction(channel, message, pc);
                    }
                    return true;
                case CommandStrings.Smite:
                    {
                        if (!isOp) { SystemController.Instance.Respond(channel, $"You need to be an Operator to use this command, {pc.name}. The authorities have been notified of your attempt, however.", pc.name); return true; }
                        SmiteAction(channel, message, pc);
                    }
                    return true;
            }

            return false;
        }

        /// <summary>
        /// responsible for handling commands any op can access
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="channel">the source channel</param>
        /// <param name="isOp">if the user is an op</param>
        /// <param name="message">the cleaned up message</param>
        /// <param name="pc">player card for the sending user</param>
        /// <returns></returns>
        public bool HandleUserCommands(string command, string channel, string message, PlayerCard pc)
        {
            switch (command)
            {
                case CommandStrings.Gift:
                    {
                        GiftAction(channel, message, pc);
                    }
                    return true;
                case CommandStrings.Card:
                    {
                        CardAction(channel, pc, message);
                    }
                    return true;
                case CommandStrings.Deck:
                    {
                        PlayingCards tpc = (PlayingCards)Bot.GetPlugin(typeof(PlayingCards));

                        try
                        {
                            command = message.Split(' ').First();
                        }
                        catch
                        {
                            command = message;
                        }

                        try
                        {
                            message = message.Substring(command.Length).TrimStart();
                            tpc.HandleUserCommands(command, channel, message, pc);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"Error in cardgame plugin. Sorry! Might be recoverable by stopping current game, might not.", pc.name);
                        }
                    }
                    break;
                case CommandStrings.Leaderboard:
                    {
                        LeaderboardAction(channel, pc);
                    }
                    return true;
                case CommandStrings.Upgrade:
                    {
                        UpgradeAction(channel, message, pc);
                    }
                    return true;
                case CommandStrings.Set:
                    {
                        SetAction(message, pc);
                    }
                    return true;
                case CommandStrings.Prog:
                    {
                        ProgAction(channel, pc);
                    }
                    return true;
                case CommandStrings.Gold:
                    {
                        GoldAction(channel, pc);
                    }
                    return true;
                case CommandStrings.Cooldown:
                    {
                        CooldownAction(pc);
                    }
                    return true;
                case CommandStrings.Dive:
                    {
                        DiveAction(channel, pc);
                    }
                    return true;
            }
            return false;
        }

        /// <summary>
        /// handles all received messages, parsing them where appropriate
        /// </summary>
        /// <param name="command">command being sent, if any</param>
        /// <param name="channel">the source channel</param>
        /// <param name="message">the cleaned message</param>
        /// <param name="sendingUser">the user sending the message</param>
        /// <param name="isOp">if the user is an op</param>
        public override void HandleRecievedMessage(string command, string channel, string message, string sendingUser, bool isOp)
        {
            // handle non-command conversation
            if (string.IsNullOrWhiteSpace(command))
            {
                HandleNonCommand(channel, sendingUser, message);
                return;
            }

            // check if the command is an actual command
            if (!GetCommandList().Any(x => x.command.Equals(command)))
            {
                SystemController.Instance.Respond(channel, $"Sorry, {sendingUser}, but I didn't understand your command.", sendingUser);
                return;
            }

            // handle non-user commands
            if (HandleBaseCommands(command, channel, sendingUser))
            {
                return;
            }

            // get user card
            PlayerCard pc = GetActiveUser(sendingUser);
            if (pc == null)
            {
                SystemController.Instance.Respond(channel, $"You need to create a character with -create before using this command, {sendingUser}.", sendingUser);
                return;
            }

            // handle op commands
            if (HandleOpCommands(command, channel, isOp, message, pc))
            {
                return;
            }

            // handle user commands
            if (HandleUserCommands(command, channel, message, pc))
            {
                return;
            }
        }

        /// <summary>
        /// fetches the card of an active user
        /// </summary>
        /// <param name="command">command being sent, if any</param>
        /// <param name="channel">the source channel</param>
        /// <param name="sendingUser">user sending the request</param>
        /// <returns></returns>
        public PlayerCard GetActiveUser(string sendingUser)
        {
            PlayerCard pc = GameDb.GetCard(sendingUser);
            if (pc != null)
            {
                return pc;
            }

            return null;
        }

        /// <summary>
        /// Handles adding activity bonuses to public chatter
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="pc">player that sent the post</param>
        /// <param name="message">the message</param>
        public void HandleChatActivityBonus(string channel, PlayerCard pc, string message)
        {
            int charCount = message.Length;
            int cooldown;
            TimeSpan ts;

            if (message.ToLowerInvariant().StartsWith("/me"))
            {
                cooldown = Convert.ToInt32(charCount * (1000 * CdReductionPerChar * 2));
            }
            else
            {
                cooldown = Convert.ToInt32(charCount * (1000 * CdReductionPerChar));
            }

            ts = new TimeSpan(0, 0, 0, 0, cooldown);
            string respond = string.Empty;

            // min for notification
            if (ts.TotalMinutes > 10)
            {
                respond = $"[sup]Cooldown Reduced by {ts} (Activity Bonus)[/sup]";
            }

            // max cap
            if (ts.TotalMinutes > 45)
            {
                ts = new TimeSpan(0, 45, 0);
                respond = $"[sup]Cooldown Reduced by {ts} (Activity Bonus | CAP!)[/sup]";

            }

            try
            {
                pc.lastDive -= ts;
            }
            catch
            {
                pc.lastDive = DateTime.Now - ts;
            }

            GameDb.UpdateCard(pc);

            if (ts.TotalMinutes > 10)
            {
                SystemController.Instance.Respond(channel, respond, pc.name);
            }
        }

        /// <summary>
        /// handles parsing non-command interactions
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="user">source user</param>
        /// <param name="message">message the user sent</param>
        public void HandleNonCommand(string channel, string user, string message)
        {
            PlayerCard pc = GameDb.GetCard(user);
            if (pc == null || string.IsNullOrWhiteSpace(channel))
            {
                return;
            }

            // parse for in-conversation commands


            // take care of activity bonus
            HandleChatActivityBonus(channel, pc, message);
        }

        /// <summary>
        /// our base constructor
        /// </summary>
        /// <param name="api">our chat api</param>
        public GameCore(ApiConnection api, string commandChar) : base(api, commandChar)
        {
            Bot = new BotCore();
            Bot.AddPlugin(new PlayingCards(api, commandChar));
            LoadMonsterDefeatedBlurbs();
        }
    }
}