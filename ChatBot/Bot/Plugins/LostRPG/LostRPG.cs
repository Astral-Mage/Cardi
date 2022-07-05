using ChatApi;
using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Dialogue;
using ChatBot.Bot.Plugins.LostRPG.RoleplaySystem;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG
{
    public partial class LostRPG : PluginBase
    {
        /// <summary>
        /// just some random base vars
        /// </summary>
        public string BASE_COLOR = "white";

        /// <summary>
        /// handles parsing non-command input and does work
        /// </summary>
        public EncounterTracker encounterTracker = new EncounterTracker();

        public void HandleInlineCommand(string channel, string user, string message)
        {
            List<string> possibleTriggers = message.Split(' ').ToList().Where(x => x.StartsWith(CommandChar)).ToList();

            bool canContinue = true;
            for (int loopCount = 0; loopCount < possibleTriggers.Count && canContinue; loopCount++)
            {
                switch(possibleTriggers[loopCount].StripPunctuation().ToLowerInvariant())
                {
                    case CommandStrings.Card:
                        {
                            SmallCardAction(channel, user);
                            canContinue = false;
                        }
                        break;
                    default:
                        {
                            canContinue = true;
                        }
                        break;
                }
            }
        }

        public void HandleNonCommand(string channel, string user, string message)
        {
            HandleInlineCommand(channel, user, message);

            DialogueController.Instance.TryProgressDialogue(user, message, DialogueSystem.Enums.DialogueType.Conversation, string.IsNullOrWhiteSpace(channel) ? DialogueSystem.Enums.DialogueLocale.Private : DialogueSystem.Enums.DialogueLocale.Public);

            if (message.ToLowerInvariant().StartsWith("/me"))
            {
                if (DataDb.Instance.UserExists(user))
                {
                    RoleplayController.Instance.ParsePost(user, message.Replace("/me", ""), channel);
                }
            }





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
            // handle inline commands and roleplay
            if (string.IsNullOrWhiteSpace(command))
            {
                HandleNonCommand(channel, sendingUser, message);
                return;
            }

            // check if the command is an actual command
            if (!GetCommandList().Any(x => x.command.Equals(command)))
            {
                return;
            }

            // handle op commands
            if (HandleOpCommands(command, channel, isOp, message, sendingUser))
            {
                return;
            }

            // handle user commands
            if (HandleUserCommands(command, channel, message, sendingUser))
            {
                return;
            }
        }

        /// <summary>
        /// returns a small card.
        /// </summary>
        /// <param name="channel">origin channel</param>
        /// <param name="message">original message</param>
        /// <param name="user">sending user</param>
        /// <param name="showsig">whether to show the signature or not</param>
        public void SmallCardAction(string channel, string user)
        {
            if (!DataDb.Instance.UserExists(user))
                return;

            UserCard card = DataDb.Instance.GetCard(user);

            string toSend = string.Empty;

            toSend += $"Alias: {card.Alias} | UserId: {card.UserId} | Magic: [color={card.MagicData.PrimaryMagic.Color}][b]{card.MagicData.PrimaryMagic.Name}[/b][/color]";
            SystemController.Instance.Respond(channel, toSend, user);
        }

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
                new Command(CommandStrings.Card, BotCommandRestriction.Both, CommandSecurity.None, "displays details about the character"),
                new Command(CommandStrings.Status, BotCommandRestriction.Both, CommandSecurity.Ops, "sets the bot status"),
            };
        }

        /// <summary>
        /// responsible for handling commands any op can access
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="channel">the source channel</param>
        /// <param name="isOp">if the user is an op</param>
        /// <param name="message">the cleaned up message</param>
        /// <returns>true if an op command was handled</returns>
        public bool HandleOpCommands(string command, string channel, bool isOp, string message, string user)
        {
            switch (command)
            {
                case CommandStrings.Status:
                    {
                        if (!isOp)
                            return true;

                        int result;
                        ChatStatus sta;

                        try
                        {
                            result = int.Parse(message.Split(' ').First());
                            sta = (ChatStatus)result;
                        }
                        catch (Exception)
                        {
                            SystemController.Instance.Respond(channel, $"Unable to parse status. Please try again.", user);
                            return true;
                        }

                        Api.SetStatus(sta.ToString(), message.Split(" ".ToCharArray(), 2).Last(), user);
                        SystemController.Instance.Respond(channel, $"Setting your status to: [{sta}] [{message.Split(" ".ToCharArray(), 2).Last()}]", user);
                    }
                    break;
            }

            return false;
        }

        public void CreateCharacter(string user)
        {
            if (!DataDb.Instance.UserExists(user))
            {
                DialogueController.Instance.StartDialogue(typeof(CreateUser), user);
                return;
            }
            else
            {
                UserCard card = DataDb.Instance.GetCard(user);
                SystemController.Instance.Respond(null, $"You've already created your character, {card.Alias}", user);
                return;
            }
        }

        /// <summary>
        /// responsible for handling commands any user can access
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="channel">the source channel</param>
        /// <param name="message">the cleaned up message</param>
        /// <returns>true if a user command was handled</returns>
        public bool HandleUserCommands(string command, string channel, string message, string user)
        {
            switch (command)
            {
                case CommandStrings.Help:
                    {
                        if (message.StartsWith("magic"))
                        {

                        }
                        else
                        {
                            HelpAction(user);
                        }
                    }
                    break;
                case CommandStrings.Create:
                    {
                        CreateCharacter(user);
                    }
                    break;
                case CommandStrings.Card:
                    {
                        CardAction(channel, user);
                    }
                    break;
            }

            return false;
        }

        public void CardAction(string channel, string user)
        {
            if (!DataDb.Instance.UserExists(user))
                return;

            UserCard card = DataDb.Instance.GetCard(user);

            string toSend = string.Empty;

            toSend += $"Alias: {card.Alias} | UserId: {card.UserId} | Magic: [color={card.MagicData.PrimaryMagic.Color}][b]{card.MagicData.PrimaryMagic.Name}[/b][/color]";
            SystemController.Instance.Respond(channel, toSend, user);
        }

        /// <summary>
        /// Sends a basic magic help blurb
        /// </summary>
        /// <param name="sendingUser">User that made the request</param>
        public void MagicHelpAction(string sendingUser)
        {
            string toSend = string.Empty;

            toSend += $"[color=white]" +
                            $"         Welcome to everything [b][color=cyan]Magic[/color][/b]!" +
                $"\\n    " +
                $"\\n        * Note that this bot is currently in [color=green]Alpha[/color]. Data resets may happen!" +
                $"\\n    " +
                $"\\nType {CommandChar}{CommandStrings.Create} to get started." +
                $"[/color]";

            SystemController.Instance.Respond(null, toSend, sendingUser);
        }

        /// <summary>
        /// Sends a basic help blurb
        /// </summary>
        /// <param name="sendingUser">User that made the request</param>
        public void HelpAction(string sendingUser)
        {
            string toSend = string.Empty;

            toSend += $"[color=white]" +
                            $"         Welcome to [b][color=red]Cardinal System[/color][/b]!" +
                $"\\n    " +
                $"\\n        * Note that this bot is currently in [color=green]Alpha[/color]. Data resets may happen!" +
                $"\\n    " +
                $"\\nType {CommandChar}{CommandStrings.Create} to get started." +
                $"[/color]";

            SystemController.Instance.Respond(null, toSend, sendingUser);
        }

        /// <summary>
        /// our base constructor
        /// </summary>
        /// <param name="api">f-list api interface</param>
        public LostRPG(ApiConnection api, string commandChar) : base(api, commandChar)
        {
            //StartBossTimedTriggerEvent();
        }

        readonly List<string> ActiveChannels = new List<string>();
        //readonly int bossId = 3733;
        
        //readonly List<TriggeredEvent> ActiveTriggeredEvents = new List<TriggeredEvent>();

        public override void HandleJoinedChannel(string channel)
        {
            ActiveChannels.Add(channel);
        }
    }
}
