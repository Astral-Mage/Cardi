using Accord;
using ChatApi;
using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Cards.Floor;
using ChatBot.Bot.Plugins.GatchaGame.Data;
using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    public partial class GatchaGame : PluginBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string BASE_COLOR = "white";
        const double XPMULT = RngGeneration.XPMULT;

        /// <summary>
        /// 
        /// </summary>
        public EncounterTracker encounterTracker = new EncounterTracker();

        public bool HandleNonCommand(string channel, string user, string message)
        {
            if (message.Split(' ').ToList().Any(x => x.StartsWith(CommandChar) && x.StripPunctuation().Equals($"{CommandStrings.Card}", StringComparison.OrdinalIgnoreCase)))
            {
                SmallCardAction(channel, message, user);
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
            //if (string.IsNullOrWhiteSpace(command))
            //{
            //    HandleNonCommand(channel, sendingUser, message);
            //    return;
            //}

            // check for dynamically added trigger commands
            if (HandleTriggeredCommands(sendingUser, channel, command))
            {
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
        public void SmallCardAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            Cards.PlayerCard pc = null;

            if (!string.IsNullOrWhiteSpace(message))
            {
                if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard tempCard))
                {
                    RngGeneration.TryGetCard(user, out tempCard);
                    pc = tempCard;
                }
                else
                {
                    pc = tempCard;
                }
            }
            else
            {
                RngGeneration.TryGetCard(user, out pc);
            }

            if (pc == null)
            {
                SystemController.Instance.Respond(channel, "Invalid character name.", user);
                return;
            }

            // display card
            string cardStr = string.Empty;

            string displayname = (string.IsNullOrWhiteSpace(pc.DisplayName)) ? pc.Name : pc.DisplayName;
            string species = (string.IsNullOrWhiteSpace(pc.SpeciesDisplayName)) ? ((SpeciesTypes)pc.GetStat(StatTypes.Sps)).GetDescription() : pc.SpeciesDisplayName;
            string cClass = (string.IsNullOrWhiteSpace(pc.ClassDisplayName)) ? ((ClassTypes)pc.GetStat(StatTypes.Cs1)).GetDescription() : pc.ClassDisplayName;

            //int artificalmax = 90;
            //double pants = 90.0 / pc.GetStat(StatTypes.StM);
            double whatever = XPMULT * pc.GetStat(StatTypes.Sta);

            cardStr += $"[b]Lvl: [/b]{pc.GetStat(StatTypes.Lvl)} | [b]Name: [/b]{displayname} | [b]Species: [/b]{species} | [b]Class: [/b]{cClass} | ";

            string boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Sharpness)) ? $"[color=cyan]◉[/color]" : $"[color=white]•[/color]";
            if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Weapon) > 0)
            {
                foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Weapon))
                {
                    cardStr += $"{boonAddition} ";
                    cardStr += $"{v.GetRarityString()} {v.GetName()}";
                }
            }
            else
            {
                cardStr += $"{boonAddition} ";
                cardStr += "Bare Hands";
            }

            if (!string.IsNullOrWhiteSpace(pc.Signature))
            {
                cardStr += $"\\n                      {pc.Signature}";
            }

            SystemController.Instance.Respond(channel, cardStr, user);
        }

        /// <summary>
        /// creates a list of valid commands for this plugin
        /// </summary>
        /// <returns></returns>
        public List<Command> GetCommandList()
        {
            return new List<Command>
            {
                new Command(CommandStrings.Help, ChatTypeRestriction.Whisper, CommandSecurity.None, "returns the list of help options"),
                new Command(CommandStrings.MoreHelp, ChatTypeRestriction.Whisper, CommandSecurity.None, "returns additional help options"),
                new Command(CommandStrings.MoreHelpLong, ChatTypeRestriction.Whisper, CommandSecurity.None, "returns additional help options"),

                new Command(CommandStrings.Create, ChatTypeRestriction.Both, CommandSecurity.None, "creates a new character"),
                new Command(CommandStrings.Roll, ChatTypeRestriction.Both, CommandSecurity.None, "rolls in the gatcha"),
                new Command(CommandStrings.Dive, ChatTypeRestriction.Both, CommandSecurity.None, "dives into the dungeon"),
                new Command(CommandStrings.Set, ChatTypeRestriction.Both, CommandSecurity.None, "handles various set commands", "help"),
                new Command(CommandStrings.Box, ChatTypeRestriction.Both, CommandSecurity.None, "handles various inventory commands", "help"),
                new Command(CommandStrings.Card, ChatTypeRestriction.Both, CommandSecurity.None, "displays details about the character"),
                new Command(CommandStrings.Smite, ChatTypeRestriction.Both, CommandSecurity.Ops, "displays short details about the character"),

                new Command(CommandStrings.Upgrade, ChatTypeRestriction.Whisper, CommandSecurity.None, "upgrades your sockets for gold", "help"),

                new Command(CommandStrings.Equip, ChatTypeRestriction.Both, CommandSecurity.None, "equips items"),
                new Command(CommandStrings.Unequip, ChatTypeRestriction.Both, CommandSecurity.None, "unequips items"),
                new Command(CommandStrings.Divefloor, ChatTypeRestriction.Both, CommandSecurity.None, "sets your default dive floor"),
                new Command(CommandStrings.DivefloorLong, ChatTypeRestriction.Both, CommandSecurity.None, "sets your default dive floor"),
                new Command(CommandStrings.Status, ChatTypeRestriction.Both, CommandSecurity.Ops, "sets the bot status"),

                new Command(CommandStrings.Bully, ChatTypeRestriction.Both, CommandSecurity.None, "attempts to bully your target"),
                new Command(CommandStrings.Submit, ChatTypeRestriction.Both, CommandSecurity.None, "submits to your bully and transfers them your stamina"),
                new Command(CommandStrings.Fight, ChatTypeRestriction.Both, CommandSecurity.None, "fight your bully and initiate a pvp duel"),
                new Command(CommandStrings.Focus, ChatTypeRestriction.Both, CommandSecurity.None, "focuses on a specific stat for enhanced  increases to it. 1 at a time."),

                new Command(CommandStrings.Verbose, ChatTypeRestriction.Both, CommandSecurity.None, "enables enhanced combat logging in whispers only."),

                new Command(CommandStrings.BaseCooldown, ChatTypeRestriction.Both, CommandSecurity.None, "sets the dive stamina required."),
                new Command(CommandStrings.StardustCooldown, ChatTypeRestriction.Both, CommandSecurity.None, "sets the gatcha stardust required."),
                new Command(CommandStrings.Reset, ChatTypeRestriction.Both, CommandSecurity.None, "resets a specific user's specific cooldown timer."),
                new Command(CommandStrings.Gift, ChatTypeRestriction.Both, CommandSecurity.None, "gives a gift of gold to another user."),

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
                case CommandStrings.BaseCooldown:
                    {
                        if (!isOp)
                            return true;

                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                        {
                            break;
                        }

                        BaseCooldownAction(channel, message, card);
                    }
                    break;
                case CommandStrings.Smite:
                    {
                        if (!isOp)
                            return true;

                        if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard card))
                        {
                            break;
                        }

                        Data.DataDb.DeleteCard(card.Name);
                        SystemController.Instance.Respond(channel, $"{card.DisplayName} has been smote.", user);
                    }
                    break;
                case CommandStrings.StardustCooldown:
                    {
                        if (!isOp)
                            return true;

                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                        {
                            break;
                        }

                        GatchaRequiredStardustAction(channel, message, card);
                    }
                    break;
                case CommandStrings.Reset:
                    {
                        if (!RngGeneration.TryGetCard(user, out _))
                        {
                            break;
                        }

                        if (!isOp)
                            return true;

                        //List<string> messageBroken = 
                        string cdTarget = message.Substring(message.Length - (LastUsedCooldownType.LastDive.GetDescription().ToString() + " ").Length).Replace(" ", "");
                        string tu = message.Substring(0, message.Length - cdTarget.Length);
                        tu = tu.TrimEnd(' ');

                        if (!Data.DataDb.UserExists(tu))
                        {
                            SystemController.Instance.Respond(null, $"Unable to resolve card for user: {tu}. Format: {CommandChar}{CommandStrings.Reset} *Target* *CooldownType*", user);
                            break;
                        }

                        if (!RngGeneration.TryGetCard(tu, out Cards.PlayerCard targetCard))
                        {
                            Console.WriteLine($"Unknown error attempting to get card for user: {tu}");
                            break;
                        }

                        bool successful = false;
                        foreach(var v in Enum.GetValues(typeof(LastUsedCooldownType)))
                        {
                            if (v.GetDescription().Equals(cdTarget, StringComparison.InvariantCultureIgnoreCase))
                            {
                                targetCard.LastTriggeredCds[(LastUsedCooldownType)v] = DateTime.MinValue;
                                Data.DataDb.UpdateCard(targetCard);
                                successful = true;
                                break;
                            }
                        }

                        if (successful)
                        {
                            SystemController.Instance.Respond(null, $"Succesfully reset {targetCard.Name}'s {cdTarget}!", user);

                        }
                        else
                        {
                            SystemController.Instance.Respond(null, $"Unknown error attempting to reset cooldown for {targetCard.Name}.", user);
                        }
                    }
                    break;
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

        public void CreateCharacter(string channel, string user)
        {
            if (Data.DataDb.UserExists(user))
                return;

            Cards.PlayerCard pc = new Cards.PlayerCard()
            {
                Name = user,
                DisplayName = user,
                MaxInventory = 10
            };

            pc.AvailableSockets.Add(SocketTypes.Weapon);
            pc.AvailableSockets.Add(SocketTypes.Armor);
            pc.AvailableSockets.Add(SocketTypes.Passive);

            RngGeneration.GenerateNewCharacterStats(pc);
            Data.DataDb.AddNewUser(pc);
            SystemController.Instance.Respond(channel, $"              [b][color=orange]Welcome to Cardinal's Cathedral, {user}!![/color][/b] " +
                $"\\n" +
                $"\\nMagic manifests itself in many different ways." +
                $"\\n" +
                $"\\nRunes, Magic Circles, Stands, Incantations, Chants, Ki, Will, Diving Blessing, Unholy Curse, etc..." +
                $"\\n" +
                $"\\nAll a different manifestation from a single source; a font of power, of significance. And here, a" +
                $"\\nsmall sphere where everything blends together. An ancient Cathedral, left by builders unknown," +
                $"\\nhas weakened and exposed a rip between the corporeal and the incorporeal. Adventurer, you" +
                $"\\nare but one of the few powerful enough to withstand the corruption even for a short time. It is" +
                $"\\nup to you to dive inside and fend off the evil denizens that scrape their way up from the void." +
                $"\\n" +
                $"\\nPlease, {pc.DisplayName}..." +
                $"\\n" +
                $"\\nWon't you help us? Help... everyone?" +
                $"\\n" +
                $"\\nType [color=orange]{CommandChar}{CommandStrings.Help}[/color] to learn what kind of things you can do here.", user);
        }

        public void GiftAction(string channel, string message, Cards.PlayerCard pc)
        {
            Cards.PlayerCard gc;
            int goldAmount = 0;
            string nickname = (string.IsNullOrWhiteSpace(pc.DisplayName)) ? $"[color=white]{pc.Name}[/color]" : $"[color=white]{pc.DisplayName}[/color]";

            // public only
            if (channel == null)
            {
                SystemController.Instance.Respond(null, $"Sorry, {nickname}, but you can only gift in public channels!", pc.Name);
                return;
            }

            try
            {
                goldAmount = Convert.ToInt32(message.Split(' ').Last());
                string targetName = message.Replace(goldAmount.ToString(), "").TrimEnd();
                RngGeneration.TryGetCard(targetName, out gc);
                if (gc == null)
                {
                    SystemController.Instance.Respond(channel, $"Sorry, {nickname}, but {targetName} isn't a valid user! Check your spelling or casing.", pc.Name);
                    return;
                }
                else if (goldAmount <= 0)
                {
                    SystemController.Instance.Respond(channel, $"Sorry, {nickname}, but you can only give positive amounts of gold to other people!", pc.Name);
                    return;
                }
                else if (goldAmount > pc.GetStat(StatTypes.Gld))
                {
                    SystemController.Instance.Respond(channel, $"Sorry, {nickname}, but you can only give as much gold as you have ({pc.GetStat(StatTypes.Gld)}) to someone!", pc.Name);
                    return;
                }
                else if (pc.Name.Equals(gc.Name))
                {
                    SystemController.Instance.Respond(channel, $"Sorry, {nickname}, but you can't gift yourself!", pc.Name);
                    return;
                }
            }
            catch
            {
                gc = new Cards.PlayerCard();
            }

            if (string.IsNullOrWhiteSpace(gc.Name))
            {
                SystemController.Instance.Respond(channel, $"Sorry, {nickname}, but unable to find user: {message}. Expected format: {CommandChar}{CommandStrings.Gift} Rng 34    {CommandChar}{CommandStrings.Gift} Cardinal System 14", pc.Name);
            }
            else
            {
                gc.AddStat(StatTypes.Gld, goldAmount);
                pc.AddStat(StatTypes.Gld, -goldAmount);
                Data.DataDb.UpdateCard(gc);
                Data.DataDb.UpdateCard(pc);
                string nicknameTwo = (string.IsNullOrWhiteSpace(gc.DisplayName)) ? $"[color=white]{gc.Name}[/color]" : $"[color=white]{gc.DisplayName}[/color]";
                SystemController.Instance.Respond(channel, $"[b]{nickname}[/b] has gifted [b]{nicknameTwo}[/b] [color=yellow][b]{goldAmount}[/b][/color] gold!", pc.Name);
            }
        }

        public void BaseCooldownAction(string channel, string message, Cards.PlayerCard pc)
        {
            try
            {
                int cd = Convert.ToInt32(message);
                REQUIRED_DIVE_STAMINA = cd;
                SystemController.Instance.Respond(channel, $"[b]Base stamina required to dive has been set to: [color=green]{REQUIRED_DIVE_STAMINA}[/color]. Happy hunting![/b]", pc.Name);
            }
            catch
            {
                SystemController.Instance.Respond(channel, $"Invalid format. Try again! ex: {CommandChar}{CommandStrings.BaseCooldown} 20 (for 20 stamina)", pc.Name);
            }
        }

        public void GatchaRequiredStardustAction(string channel, string message, Cards.PlayerCard pc)
        {
            try
            {
                int cd = Convert.ToInt32(message);
                COST_TO_ROLL = cd;
                SystemController.Instance.Respond(channel, $"[b]Base Stardust required to roll in the Gatcha has been set to: [color=green]{REQUIRED_DIVE_STAMINA}[/color]. Good luck![/b]", pc.Name);
            }
            catch
            {
                SystemController.Instance.Respond(channel, $"Invalid format. Try again! ex: {CommandChar}{CommandStrings.StardustCooldown} 20 (for 20 stamina)", pc.Name);
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
            switch(command)
            {
                case CommandStrings.Gift:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                        {
                            break;
                        }

                        GiftAction(channel, message, card);
                    }
                    break;
                case CommandStrings.Reset:
                    {

                    }
                    break;
                case CommandStrings.Roll:
                    {
                        if (string.IsNullOrWhiteSpace(message) || !int.TryParse(message, out int rollCount))
                        {
                            rollCount = 1;
                        }

                        RollAction(rollCount, user, channel);
                    }
                    break;
                case CommandStrings.Verbose:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                        {
                            break;
                        }

                        ucard.Verbose = !ucard.Verbose;
                        Data.DataDb.UpdateCard(ucard);
                        SystemController.Instance.Respond(null, $"Combat verbosity changed to {(ucard.Verbose == true ? "[color=green]Enabled[/color]" : "[color=orange]Disabled[/color]")}. This only takes effect in whispers.", ucard.Name);
                    }
                    break;
                case CommandStrings.Help:
                    {
                        HelpAction(user);
                    }
                    break;
                case CommandStrings.MoreHelpLong:
                case CommandStrings.MoreHelp:
                    {
                        MoreHelpAction(user);
                    }
                    break;
                case CommandStrings.Create:
                    {
                        CreateCharacter(channel, user);
                    }
                    break;
                case CommandStrings.Dive:
                    {
                        DiveAction(channel, user, message);
                    }
                    break;
                case CommandStrings.Set:
                    {
                        SetAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Box:
                    {
                        InventoryAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Card:
                    {
                        CardAction(channel, message, user, true);
                    }
                    break;
                case CommandStrings.Equip:
                    {
                        EquipAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Unequip:
                    {
                        UnequipAction(channel, message, user);
                    }
                    break;
                case CommandStrings.DivefloorLong:
                case CommandStrings.Divefloor:
                    {
                        SetDiveFloorAction(channel, message, user);
                    }
                    break;
                case CommandStrings.RequestDuelLong:
                case CommandStrings.RequestDuel:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                        {
                            break;
                        }

                        if (!Data.DataDb.UserExists(message))
                        {
                            SystemController.Instance.Respond(null, $"Unable to resolve card for user: {message}", user);
                            break;
                        }

                        if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard card))
                        {
                            Console.WriteLine($"Unknown error attempting to get card: {message}");
                            break;
                        }

                        SystemController.Instance.Respond(null, $"{ucard.DisplayName} has challenged you to a duel. [sub][color=green]Accept by responding to me with: {CommandChar}{CommandStrings.AcceptDuelLong}[/color] | [color=red]Decline by responding to me with: {CommandChar}{CommandStrings.DenyDuelLong}[/color][/sub]", card.Name);
                    }
                    break;
                case CommandStrings.CancelDuelLong:
                case CommandStrings.CancelDuel:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                        {
                            break;
                        }

                    }
                    break;
                case CommandStrings.AcceptDuelLong:
                case CommandStrings.AcceptDuel:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                        {
                            break;
                        }

                    }
                    break;
                case CommandStrings.DenyDuelLong:
                case CommandStrings.DenyDuel:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                        {
                            break;
                        }

                    }
                    break;
                case CommandStrings.StartDuelLong:
                case CommandStrings.StartDuel:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                        {
                            break;
                        }

                    }
                    break;
                case CommandStrings.Focus:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                        {
                            break;
                        }

                        if (!Enum.GetNames(typeof(StatTypes)).Any(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            SystemController.Instance.Respond(null, $"Unable to resolve valid focusable stat for value: {message}", user);
                            break;
                        }

                        StatTypes toFocus = (StatTypes)Enum.Parse(typeof(StatTypes), message, true);

                        if (!RngGeneration.GetAllFocusableStats().Contains(toFocus))
                        {
                            SystemController.Instance.Respond(null, $"Unable to resolve valid focusable stat for value: {message}", user);
                            break;
                        }
                        ucard.GetStat(StatTypes.Foc);
                        ucard.SetStat(StatTypes.Foc, (int)toFocus);
                        Data.DataDb.UpdateCard(ucard);

                        SystemController.Instance.Respond(null, $"Successfully set stat focus to: {message}", user);
                    }
                    break;
                case CommandStrings.Bully:
                    {
                        // setup our vars 
                        Encounter enc = null;

                        // do our basic bully checks
                        if (!BasicBullyChecks(channel, user, message, out Cards.PlayerCard card, out Cards.PlayerCard targetCard))
                            break;

                        // start a new encounter
                        enc = new Encounter(targetCard.BaseCooldowns[PlayerActionTimeoutTypes.BullyAttemptCooldown], card.Name)
                        {
                            Creator = card.Name
                        };

                        // add bully
                        enc.AddParticipant(1, card);
                        card.LastTriggeredCds[LastUsedCooldownType.LastBully] = enc.CreationDate;

                        // add the bullied target
                        enc.AddParticipant(2, targetCard);
                        targetCard.LastTriggeredCds[LastUsedCooldownType.LastBullied] = enc.CreationDate;

                        // add the encounter
                        encounterTracker.AddEncounter(enc);
                        Data.DataDb.UpdateCard(card);
                        Data.DataDb.UpdateCard(targetCard);
                        SystemController.Instance.Respond(channel, $"{card.DisplayName} is attempting to bully you, {targetCard.DisplayName}! [sub][color=pink]You can submit by replying with: {CommandChar}{CommandStrings.Submit}[/color] | [color=red]You can fight back by replying with: {CommandChar}{CommandStrings.Fight}[/color][/sub]", string.Empty);
                    }
                    break;
                case CommandStrings.Submit:
                    {
                        // do some basic submit checks here
                        if (!BasicSubmitChecks(channel, user, message, out Cards.PlayerCard card))
                            break;

                        // find encounter
                        Encounter enc = null;
                        foreach (var v in encounterTracker.PendingEncounters)
                        {
                            if (v.Value.EncounterType == EncounterTypes.Bully && v.Value.Participants.Any(x => x.Participant.Name.Equals(card.Name, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                enc = v.Value;
                                break;
                            }
                        }

                        // break out if we couldn't find it for some reason
                        if (enc == null)
                            break;

                        // bail out if we can't find our bully
                        RngGeneration.TryGetCard(enc.Creator, out Cards.PlayerCard targetCard);
                        if (null == targetCard)
                            break;

                        // bail out if we've timed out
                        if (enc.HasTimedOut())
                        {
                            card.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
                            targetCard.LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
                            encounterTracker.KillEncounter(enc);
                            break;
                        }

                        // submit calc
                        string submitStr = string.Empty;
                        TimeSpan submitVal = new TimeSpan(0, 20, 0);

                        int staLost;
                        if (card.GetStat(StatTypes.Sta) > submitVal.TotalSeconds)
                        {
                            card.AddStat(StatTypes.Sta, -(submitVal.TotalSeconds));
                            staLost = Convert.ToInt32(submitVal.TotalSeconds);
                        }
                        else
                        {
                            staLost = card.GetStat(StatTypes.Sta);
                            card.SetStat(StatTypes.Sta, 0);
                        }

                        //double pants = 90.0 / ucard.GetStat(StatTypes.StM);
                        double whatever = XPMULT * staLost;

                        submitStr += $"{card.DisplayName}, you submit to {targetCard.DisplayName}'s aggressive bullying, losing {whatever} stamina. ";

                        // bully calc
                        int staWon;
                        if (targetCard.GetStat(StatTypes.Sta) + staLost >= targetCard.GetStat(StatTypes.StM))
                        {
                            staWon = Convert.ToInt32(targetCard.GetStat(StatTypes.StM) - targetCard.GetStat(StatTypes.Sta));
                            targetCard.SetStat(StatTypes.Sta, targetCard.GetStat(StatTypes.StM));
                        }
                        else
                        {
                            staWon = staLost;
                            targetCard.AddStat(StatTypes.Sta, staLost);
                        }

                        whatever = XPMULT * staWon;
                        submitStr += $"{targetCard.DisplayName}, you gain {whatever} stamina for your successful bullying.";

                        // finalize
                        targetCard.AddStat(StatTypes.Bly, 1, false, false, false);
                        card.AddStat(StatTypes.Sbm, 1, false, false, false);
                        Data.DataDb.UpdateCard((targetCard as Cards.PlayerCard));
                        Data.DataDb.UpdateCard(card);

                        // end the encounter
                        encounterTracker.KillEncounter(enc);

                        // respond
                        SystemController.Instance.Respond(channel, submitStr, string.Empty);
                    }
                    break;
                case CommandStrings.Fight:
                    {
                        // do some basic submit checks here
                        if (!BasicSubmitChecks(channel, user, message, out Cards.PlayerCard card))
                            break;

                        // find encounter
                        Encounter enc = null;
                        foreach (var v in encounterTracker.PendingEncounters)
                        {
                            if (v.Value.EncounterType == EncounterTypes.Bully && v.Value.Participants.Any(x => x.Participant.Name.Equals(card.Name, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                enc = v.Value;
                                break;
                            }
                        }

                        // break out if we couldn't find it for some reason
                        if (enc == null)
                            break;

                        // bail out if we can't find our bully
                        RngGeneration.TryGetCard(enc.Creator, out Cards.PlayerCard targetCard);
                        if (null == targetCard)
                            break;

                        // bail out if we've timed out
                        if (enc.HasTimedOut())
                        {
                            card.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
                            targetCard.LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
                            encounterTracker.KillEncounter(enc);
                            break;
                        }

                        // refresh cards being used
                        enc.Participants = new List<EncounterCard>();
                        enc.AddParticipant(0, card);
                        enc.AddParticipant(1, targetCard);

                        // start fight here
                        SystemController.Instance.Respond(channel, $"A fight is breaking out between {(string.IsNullOrEmpty(card.DisplayName) ? card.Name : card.DisplayName)} and {targetCard.DisplayName}!", string.Empty);
                        enc.StartEncounter(EncounterTypes.Bully);
                        var encResults = enc.RunEncounter();

                        SystemController.Instance.Respond(channel, $"There was a winner but I haven't parsed the results yet.", string.Empty);
                        Data.DataDb.UpdateCard(card);
                        Data.DataDb.UpdateCard(targetCard);
                    }
                    break;
                case CommandStrings.Upgrade:
                    {
                        UpgradeStuff(user, message, channel);
                    }
                    break;
            }

            return false;
        }

        public bool BasicSubmitChecks(string channel, string user, string message, out Cards.PlayerCard card)
        {
            // if you don't exist, get outta here
            if (!RngGeneration.TryGetCard(user, out card))
            {
                return false;
            }

            // if you ask for help, print it and get outta here
            if (message.Equals(CommandStrings.Help, StringComparison.InvariantCultureIgnoreCase))
            {
                BullyHelpAction(user);
                return false;
            }

            // if you're trying to dm a bully, get outta here
            if (string.IsNullOrWhiteSpace(channel))
            {
                return false;
            }

            return true;
        }

        public bool BasicBullyChecks(string channel, string user, string message, out Cards.PlayerCard card, out Cards.PlayerCard targetCard)
        {
            targetCard = null;

            // if you don't exist, get outta here
            if (!RngGeneration.TryGetCard(user, out card))
            {
                return false;
            }

            // if you ask for help, print it and get outta here
            if (message.Equals(CommandStrings.Help, StringComparison.InvariantCultureIgnoreCase))
            {
                BullyHelpAction(user);
                return false;
            }

            // if you're trying to dm a bully, get outta here
            if (string.IsNullOrWhiteSpace(channel))
            {
                return false;
            }

            // if the target of the bully attempt can't be found, get outta here
            if (!RngGeneration.TryGetCard(message, out targetCard))
            {
                Console.WriteLine($"Unknown error attempting to get card: {message}");
                return false;
            }

            // if the target has been bullied too recently, get outta here
            if (DateTime.Now - targetCard.LastTriggeredCds[LastUsedCooldownType.LastBullied] < targetCard.BaseCooldowns[PlayerActionTimeoutTypes.HasBeenBulliedCooldown])
            {
                SystemController.Instance.Respond(channel, $"{targetCard.DisplayName} has been bullied too recently, {card.DisplayName}.", user);
                return false;
            }

            // if you try to bully yourself, get outta here
            if (targetCard.Name == card.Name)
            {
                SystemController.Instance.Respond(channel, $"If you want to bully yourself, {targetCard.DisplayName}, go find a sad anime to watch.", user);
                return false;
            }

            // if there's already an encounter in progress with you in it, get outta here
            if (encounterTracker.PendingEncounters.ContainsKey(card.Name))
            {
                SystemController.Instance.Respond(channel, $"You're already attempting to bully a target, {targetCard.DisplayName}.", user);
                return false;
            }

            // if there's already an encounter in progress with your target in it, get outta here
            //if (encounterTracker.PendingEncounters
            //    .Where(x => x.Value.EncounterType == EncounterTypes.Bully)
            //    .Any(y => y.Value.Participants
            //    .Any(z => z.Value
            //    .Any(a => a.Name.Equals(targetCard.Name)))))
            //{
            //    Respond(channel, $"Your bully target is already in a pending bully encounter, {targetCard.DisplayName}.", user);
            //    return false;
            //}

            // check if any encounters have timed out and kill the encounter before starting a new one
            int etoc = encounterTracker.PendingEncounters.Count;
            var etocL = encounterTracker.PendingEncounters.Keys.ToList();
            List<Encounter> toKill = new List<Encounter>();
            for (int x = 0; x < etoc; x++)
            {
                var curEnc = encounterTracker.PendingEncounters[etocL[x]];
                if (curEnc.HasTimedOut())
                {
                    // reset any info that might matter
                    foreach (var v in curEnc.Participants)
                    { 
                        //foreach (var pc in v)
                        {
                            if (!RngGeneration.TryGetCard(v.Participant.Name, out Cards.PlayerCard upc))
                                continue;

                            if (upc.Name.Equals(curEnc.Creator, StringComparison.InvariantCultureIgnoreCase))
                            {
                                upc.LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
                            }
                            else
                            {
                                upc.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
                            }

                            Data.DataDb.UpdateCard(upc);
                        }
                    }

                    toKill.Add(encounterTracker.PendingEncounters[etocL[x]]);
                }
                else if (curEnc.EncounterStatus == EncounterStatus.Resolved)
                {
                    toKill.Add(curEnc);
                }
            }

            // kill the encounters that need killed
            foreach (var v in toKill)
                encounterTracker.KillEncounter(v);

            return true;
        }

        public void MoreHelpAction(string sendingUser)
        {
            string toSend = string.Empty;

            toSend += $"[color=white]" +
                $"         Welcome to [b][color={"red"}]({"More"}) {CommandStrings.Help}[/color][/b]!" +
                $"\\n" +
                $"\\n[b]Stats:[/b]" +
                $"\\n";

            foreach (var v in RngGeneration.GetAllFocusableStats())
            {
                if (v == StatTypes.Vit) toSend += $"\\n{v} ⇒ Sustainability. 'Death' when this reaches 0.";
                if (v == StatTypes.Atk) toSend += $"\\n{v} ⇒ Chance to hit with attacks.";
                if (v == StatTypes.Dmg) toSend += $"\\n{v} ⇒ Base damage.";
                if (v == StatTypes.Dex) toSend += $"\\n{v} ⇒ Physical damage multiplier.";
                if (v == StatTypes.Int) toSend += $"\\n{v} ⇒ Magical damage multiplier.";
                if (v == StatTypes.Con) toSend += $"\\n{v} ⇒ Temporary health. Reduced before [vit] damage.";
                if (v == StatTypes.Crc) toSend += $"\\n{v} ⇒ Critical strike chance modifier.";
                if (v == StatTypes.Crt) toSend += $"\\n{v} ⇒ Critical strike damage additional multiplier.";
                if (v == StatTypes.Ats) toSend += $"\\n{v} ⇒ Reduces the high/low spread of various combat actions.";
                if (v == StatTypes.Spd) toSend += $"\\n{v} ⇒ Determines order of attack during combat.";
                if (v == StatTypes.Pdf) toSend += $"\\n{v} ⇒ Physical damage reduction modifier.";
                if (v == StatTypes.Mdf) toSend += $"\\n{v} ⇒ Magical damage reduction modifier.";
                if (v == StatTypes.Eva) toSend += $"\\n{v} ⇒ Chance to evade incoming attacks.";
            }

            toSend += $"\\n" +
                $"\\n[b]Damage Types:[/b] ";

            foreach (var v in Enum.GetValues(typeof(DamageTypes)))
            {
                if ((DamageTypes)v == DamageTypes.None)
                    continue;

                toSend += $"[color={((DamageTypes)v).GetDescription()}]{((DamageTypes)v)}[/color] ";
            }

            toSend += $"\\n" +
                $"\\n[b]Additional Commands:[/b] " +
                $"\\n" +
                $"\\n[color={"green"}]{CommandChar}{CommandStrings.DivefloorLong} ⁕Value⁕[/color] sets your default dive depth." +
                $"\\n[color={"green"}]{CommandChar}{CommandStrings.Gift} ⁕Target⁕ ⁕Value⁕[/color] gives a gift of gold to another user." +
                $"\\n[color={"green"}]{CommandChar}{CommandStrings.Focus} ⁕Target⁕[/color] focuses on a specific stat to enhance while leveling." +
                $"\\n[color={"green"}]{CommandChar}{CommandStrings.Verbose}[/color] sets your combat verbosity when fighting in whispers and fight-channels." +
                $"\\n" +
                $"\\n[color={"green"}]{CommandChar}{CommandStrings.BaseCooldown} ⁕Value⁕[/color] mod-only - changes dive stamina required." +
                $"\\n[color={"green"}]{CommandChar}{CommandStrings.StardustCooldown} ⁕Value⁕[/color] mod-only - changes stardust required for the Gatcha.";


            toSend += $"\\n" +
                $"\\n[b]Cooldown Vars:[/b] ";
            foreach(var v in Enum.GetValues(typeof(PlayerActionTimeoutTypes)))
            {
                toSend += v.GetDescription();
                if (((PlayerActionTimeoutTypes)v) != (PlayerActionTimeoutTypes)(Enum.GetValues(typeof(PlayerActionTimeoutTypes)).Length - 1))
                    toSend += " | ";
            }

            toSend += $"[/color]";

            SystemController.Instance.Respond(null, toSend, sendingUser);
        }

        public void BullyHelpAction(string sendingUser)
        {
            string toSend = string.Empty;

            toSend += $"[color=white]" +
                $"         Welcome to [b][color={"cyan"}]{CommandStrings.Bully} {CommandStrings.Help}[/color][/b]!" +
                $"\\n    " +
                $"\\nBullying others is rude, but sometimes it's still worth while to do. When you [color={"cyan"}]{CommandChar}{CommandStrings.Bully}[/color]" +
                $"\\nsomeone, they can choose to either [color={"cyan"}]{CommandChar}{CommandStrings.Submit}[/color] or [color={"cyan"}]{CommandChar}{CommandStrings.Fight}[/color]!" +
                $"\\n" +
                $"\\nType [color={"cyan"}]{CommandChar}{CommandStrings.Bully} ⁕Target⁕[/color] to bully a specific target." +
                $"\\nExample: [color={"cyan"}]{CommandChar}{CommandStrings.Bully} Astral Mage[/color]" +
                $"\\n" +
                $"\\nIf you find yourself being bullied, you can either give in and submit, which gives" +
                $"\\nsome of your saved up stamina to whomever bullied you." +
                $"\\n" +
                $"\\nType [color={"cyan"}]{CommandChar}{CommandStrings.Submit}[/color] to submit to your bully and lose up to 15 stamina." +
                $"\\n" +
                $"\\nAlternatively, Type [color={"cyan"}]{CommandChar}{CommandStrings.Fight}[/color] to try and fight your bully!" +
                $"\\n" +
                $"\\nFighting your bully begin a Pvp combat encounter. You and your bully fight each" +
                $"\\nother over multiple rounds. Whichever one of you wins more rounds is considered" +
                $"\\nthe victor. The loser ends up submitting despite the struggle, and loses up to" +
                $"\\n30 stamina." +
                $"\\n" +
                $"\\nIf the Bully wins, they recieve any stamina the loser lost, up to your maximium." +
                $"[/color]";

            SystemController.Instance.Respond(null, toSend, sendingUser);
        }

        public void UpgradeHelpAction(string sendingUser)
        {
            string toSend = string.Empty;

            toSend += $"[color=white]" +
                $"         Welcome to [b][color={"pink"}]{CommandStrings.Upgrade} {CommandStrings.Help}[/color][/b]!" +
                $"\\n    " +
                $"\\nAll commands in this section are called in format: [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} ⁕Value⁕[/color]." +
                $"\\nExample: Type [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} 1[/color]" +
                $"\\n" +
                $"\\n[{CommandStrings.Upgrade}] is a system that allows you to upgrade items that you have" +
                $"\\nfound during your adventures. Upgrading items costs gold, which you" +
                $"\\ncan get from diving in the dungeon. Upgrading an item will increase" +
                $"\\nit's rarity, as well as increasing a few of that item's stastics. In" +
                $"\\nsome cases, an item may even gain entirely new stats! Equipped items," +
                $"\\nor items in your box are eligable to be upgraded." +
                $"\\n" +
                $"\\nType [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} 1[/color] to upgrade your weapon slot." +
                $"\\nType [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} 2[/color] to upgrade your armor slot." +
                $"\\nType [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} 3[/color] to upgrade your passive slot." +
                $"[/color]";

            SystemController.Instance.Respond(null, toSend, sendingUser);
        }

        public void SetDiveFloorAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res) || FloorDb.GetAllFloors().Count < res || res <= 0)
            {
                SystemController.Instance.Respond(channel, $"{user}, you must specify a valid floor", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard pc))
                return;

            pc.SetStat(StatTypes.Dff, res);
            Data.DataDb.UpdateCard(pc);
            SystemController.Instance.Respond(channel, $"{pc.DisplayName}, you'll now dive to floor {res} by default.", user);
        }

        public void EquipAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res))
            {
                SystemController.Instance.Respond(channel, $"{user}, you must specify a valid box slot you're attempting to equip. Ex: {CommandStrings.Equip} 1", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                return;

            if (card.Inventory.Count < res || res < 1)
            {
                SystemController.Instance.Respond(channel, $"{user}, you must specify a valid box slot you're attempting to equip. Ex: {CommandStrings.Equip} 1", user);
                return;
            }

            Socket toAdd = card.Inventory[res - 1];
            if (!card.AvailableSockets.Contains(toAdd.SocketType))
            {
                SystemController.Instance.Respond(channel, $"{user}, you must specify an equipment type you're allowed to equip. Ex: {CommandStrings.Equip} 1", user);
                return;
            }

            // CHECK FOR CLASS RESTRICTIONS HERE

            // ---------------------------------


            int numAllowed = card.AvailableSockets.Count(x => x == toAdd.SocketType);
            int numEquipped = card.ActiveSockets.Count(x => x.SocketType == toAdd.SocketType);
            Socket toRemove = null;

            if (numEquipped >= numAllowed)
            {
                toRemove = card.ActiveSockets.First(x => x.SocketType == toAdd.SocketType);
                card.ActiveSockets.Remove(toRemove);
                card.Inventory.Add(toRemove);
            }

            card.ActiveSockets.Add(toAdd);
            card.Inventory.Remove(toAdd);

            string replyMessage = $"{card.DisplayName}, you've equipped your {toAdd.GetName()}.";
            if (toRemove != null) replyMessage += $" You had to unequip your {toRemove.GetName()} to do so.";

            Data.DataDb.UpdateCard(card);
            SystemController.Instance.Respond(channel, replyMessage, user);
        }

        public void UnequipAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res))
            {
                SystemController.Instance.Respond(channel, $"{user}, you must specify a valid equipment slot you're attempting to un-equip. Ex: {CommandStrings.Unequip} 1", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                return;

            if (res < 1)
            {
                SystemController.Instance.Respond(channel, $"{user}, you must specify a valid equipment slot you're attempting to un-equip. Ex: {CommandStrings.Unequip} 1", user);
                return;
            }

            if (card.Inventory.Count >= card.MaxInventory)
            {
                SystemController.Instance.Respond(channel, $"{user}, you must have free inventory space to {CommandStrings.Unequip} gear.", user);
                return;
            }

            Socket toUnequip = null;
            SocketTypes unequipType;
            if (res == 1)
                unequipType = SocketTypes.Weapon;
            else if (res == 2)
                unequipType = SocketTypes.Armor;
            else if (res == 3)
                unequipType = SocketTypes.Passive;
            else
                return;

            if (card.ActiveSockets.Count(x => x.SocketType == unequipType) > 0)
            {
                toUnequip = card.ActiveSockets.First(x => x.SocketType == unequipType);
            }
            else
            {
                return;
            }

            card.ActiveSockets.Remove(toUnequip);
            card.Inventory.Add(toUnequip);
            Data.DataDb.UpdateCard(card, true);
            SystemController.Instance.Respond(channel, $"{card.DisplayName}, you've unequipped your {toUnequip.GetRarityString()} {toUnequip.GetName()}", user);
        }

        public void CardAction(string channel, string message, string user, bool showsig = false)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            Cards.PlayerCard pc = null;

            if (!string.IsNullOrWhiteSpace(message))
            {
                if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard tempCard))
                {
                    RngGeneration.TryGetCard(user, out tempCard);
                    pc = tempCard;
                }
                else
                {
                    pc = tempCard;
                }
            }
            else
            {
                RngGeneration.TryGetCard(user, out pc);
            }

            if (pc == null)
            {
                SystemController.Instance.Respond(channel, "Invalid character name.", user);
                return;
            }

            // display card
            string cardStr = string.Empty;

            string displayname = (string.IsNullOrWhiteSpace(pc.DisplayName)) ? pc.Name : pc.DisplayName;
            string species = (string.IsNullOrWhiteSpace(pc.SpeciesDisplayName)) ? ((SpeciesTypes)pc.GetStat(StatTypes.Sps)).GetDescription() : pc.SpeciesDisplayName;
            string cClass = (string.IsNullOrWhiteSpace(pc.ClassDisplayName)) ? ((ClassTypes) pc.GetStat(StatTypes.Cs1)).GetDescription() : pc.ClassDisplayName;

            //int artificalmax = 90;
            //double pants = 90.0 / pc.GetStat(StatTypes.StM);
            double whatever = XPMULT * pc.GetStat(StatTypes.Sta);

            cardStr += $"[b]Name: [/b]{displayname} | [b]Species: [/b]{species} | [b]Class: [/b]{cClass} ";

            if (!string.IsNullOrWhiteSpace(pc.Signature) && showsig)
            {
                cardStr += $"\\n                      {pc.Signature}";
            }
            // Rank ‣ {pc.GetStat(StatTypes.Pvr)} | 
            cardStr += $"\\n                      [sub][color=pink](Sta: {Math.Round(whatever, 0)}/{Math.Floor(XPMULT * pc.GetStat(StatTypes.StM))})[/color] [b]Lvl: [/b]{pc.GetStat(StatTypes.Lvl)} | [color=yellow][b]Gold: [/b]{pc.GetStat(StatTypes.Gld)}[/color] | [color=cyan][b]Stardust: [/b]{pc.GetStat(StatTypes.Sds)}[/color] | [color=red][b]Defeated: [/b]{pc.GetStat(StatTypes.Kil) + pc.GetStat(StatTypes.KiB)}[/color] 〰 " +
                $"[b]Pvp:[/b] Bullied ‣ {pc.GetStat(StatTypes.Bly)} | Submitted ‣ {pc.GetStat(StatTypes.Sbm)}[/sub]" +
                $"\\n                      ";
            cardStr += pc.GetStatsString();

            string boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Sharpness)) ? $"[color=cyan]◉[/color]" : $"[color=white]•[/color]";
            if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Weapon) > 0)
            {
                foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Weapon))
                {
                    cardStr += $"\\n                      {boonAddition} ";
                    cardStr += $"{v.GetRarityString()} {v.GetName()} {v.GetShortDescription()}";
                }
            }
            else
            {
                cardStr += $"\\n                      {boonAddition} ";
                cardStr += "Bare Hands";
            }

            boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Resiliance)) ? $"[color=cyan]◉[/color]" : $"[color=white]•[/color]";
            if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Armor) > 0)
            {
                foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Armor))
                {
                    cardStr += $"\\n                      {boonAddition} ";
                    cardStr += $"{v.GetRarityString()} {v.GetName()} {v.GetShortDescription()}";
                }
            }
            else
            {
                cardStr += $"\\n                      {boonAddition} ";
                cardStr += "Birthday Suit";
            }

            boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Empowerment)) ? $"[color=cyan]◉[/color]" : $"[color=white]•[/color]";
            if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Passive) > 0)
            {

                foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Passive))
                {
                    cardStr += $"\\n                      {boonAddition} ";
                    cardStr += $"{v.GetRarityString()} {v.GetName()} {v.GetShortDescription()}";
                }
            }
            else
            {
                cardStr += $"\\n                      {boonAddition} ";
                cardStr += "Bashful Gaze";
            }

            SystemController.Instance.Respond(channel, cardStr, user);
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
                $"\\nWelcome to Cardi's Cathedral! You've decided to become a new adventurer here." +
                $"\\nLet's get you started with that! Type [color={"green"}]{CommandChar}{CommandStrings.Create}[/color] to get started. This registers you as an" +
                $"\\nadventurer, giving you your own Adventurer Card! It stores your most important" +
                $"\\ndata. You also get access to an inventory when you become an adventurer." +
                $"\\n" +
                $"\\nType [color={"green"}]{CommandChar}{CommandStrings.Create}[/color] to get started." +
                $"\\n" +
                $"\\nType [color={"orange"}]{CommandChar}{CommandStrings.Card}[/color] to access your Adventurer card." +
                $"\\n" +
                $"\\nType [color={"purple"}]{CommandChar}{CommandStrings.Roll}[/color] to roll in the Gatcha." +
                $"\\n" +
                $"\\nYou can get more items by rolling in the Gatcha. Items will usually begin at rarity 1," +
                $"\\nbut can have many different arrangements of stastics. Make sure you build up sets of" +
                $"\\nitems for when you need to switch pieces out!" +
                $"\\n" +
                $"\\nType [color={"brown"}]{CommandChar}{CommandStrings.Box}[/color] to access your inventory. Your '{CommandStrings.Box}'." +
                $"\\n" +
                $"\\nNow you're all ready for your first dive into the dungeon! If you're ready to give it" +
                $"\\na shot, you can dive on in by typing [color={"yellow"}]{CommandChar}{CommandStrings.Dive}[/color]! Diving into the dungeon costs stamina," +
                $"\\nso you can't do it if you don't have enough. Stamina comes back over time." +
                $"\\n" +
                $"\\nType [color={"yellow"}]{CommandChar}{CommandStrings.Dive}[/color] to dive into the dungeon." +
                $"\\n" +
                $"\\nYou use Stardust to roll in the Gatcha. Stardust is obtained by diving." +
                $"\\n" +
                $"\\nThere's so much more you can do, too! Look into some of these useful commands" +
                $"\\nto help guide you around your time here in Cardi's Cathedral!" +
                $"\\n" +
                $"\\n[color={"blue"}]{CommandChar}{CommandStrings.Set} {CommandStrings.Help}[/color]          ↠ talks about customizing the look of your card." +
                $"\\n[color={"red"}]{CommandChar}{CommandStrings.Box} {CommandStrings.Help}[/color]        ↠ details your box system and how to manage it." +
                $"\\n[color={"cyan"}]{CommandChar}{CommandStrings.Bully} {CommandStrings.Help}[/color]      ↠ goes into the specifics on pvp interactions." +
                $"\\n[color={"pink"}]{CommandChar}{CommandStrings.Upgrade} {CommandStrings.Help}[/color] ↠ explains how you can upgrade your items." +
                $"\\n" +
                $"\\nDiscover many more commands by checking out my profile! You can also try " +
                $"\\ntyping [color={"red"}]{CommandChar}{CommandStrings.MoreHelpLong}[/color] as well!" +
                $"[/color]";

            SystemController.Instance.Respond(null, toSend, sendingUser);
        }

        /// <summary>
        /// replies via the f-list api
        /// </summary>
        /// <param name="channel">channel to reply to</param>
        /// <param name="message">message to reply with</param>
        /// <param name="recipient">person to reply to</param>
        public void Respond(string channel, string message, string recipient, MessageType messagetype)
        {
            if (!string.IsNullOrWhiteSpace(channel))
            {
                recipient = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(channel) && string.IsNullOrWhiteSpace(recipient))
            {
                Console.WriteLine($"Error attempting to send message with no valid channel or recipient.");
                return;
            }

            message = $"[color={BASE_COLOR}]{message}[/color]";

            Api.SendMessage(channel, message, recipient, messagetype);
        }

        /// <summary>
        /// our base constructor
        /// </summary>
        /// <param name="api">f-list api interface</param>
        public GatchaGame(ApiConnection api, string commandChar) : base(api, commandChar)
        {
            StartBossTimedTriggerEvent();
        }
        
        // TESTING CRAP OUT BELOW THIS LINE

        public bool HandleTriggeredCommands(string user, string channel, string command)
        {
            bool toReturn = false;
            lock(BossTimerLocker)
            {
                ActiveTriggeredEvents.ForEach(y =>
                {
                    var tdfe = y;
                    if (DateTime.Now - tdfe.CreatedDate > tdfe.Timeout && !tdfe.TimedOut)
                    {
                        foreach (var v in tdfe.GetCommandStrings())
                        {
                            if (ActiveTriggeredCommandStrings.Any(x => x.Equals(v, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                var lala = ActiveTriggeredCommandStrings.First(x => x.Equals(v, StringComparison.InvariantCultureIgnoreCase));
                                ActiveTriggeredCommandStrings.Remove(lala);
                                tdfe.TimedOut = true;
                            }
                        }

                        foreach (var v in ActiveChannels)
                        {
                            SystemController.Instance.Respond(v, tdfe.TimeoutMessage, null);
                        }
                        toReturn = true;
                        return;
                    }
                    else if (DateTime.Now - tdfe.CreatedDate > tdfe.Cooldown)
                    {
                        ActiveTriggeredEvents.Remove(tdfe);
                        foreach (var v in ActiveChannels)
                            SystemController.Instance.Respond(v, tdfe.CooldownMessage, null);
                        toReturn = true;
                        return;
                    }
                    else
                    {
                        return;
                    }
                });

                if (toReturn)
                    return false;

                foreach (string cs in ActiveTriggeredCommandStrings)
                {
                    if (!command.TrimStart(' ').StartsWith(cs, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (cs == CommandStrings.TB)
                    {
                        TB(channel, user);
                        return true;
                    }
                    
                }
            }
            return false;
        }

        public void TB(string channel, string user)
        {
            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
            {
                return;
            }

            var bossEvent = ActiveTriggeredEvents.First(x => x.EventId == bossId);

            if (bossEvent.PendingPlayers.Any(x => x.Name.Equals(card.Name, StringComparison.InvariantCultureIgnoreCase)))
                return;

            bossEvent.PendingPlayers.Add(card);
            SystemController.Instance.Respond(channel, $"Added {card.DisplayName} to the next wave. ({bossEvent.PendingPlayers.Count} participating Adventurers)", card.Name);
        }

        public object BossTimerLocker = new object();
        readonly List<string> ActiveTriggeredCommandStrings = new List<string>();
        readonly Dictionary<int, Timer> ActiveTriggeredTimers = new Dictionary<int, Timer>();
        readonly List<string> ActiveChannels = new List<string>();
        readonly int bossId = 3733;

        readonly List<TriggeredEvent> ActiveTriggeredEvents = new List<TriggeredEvent>();

        public override void HandleJoinedChannel(string channel)
        {
            ActiveChannels.Add(channel);
        }

        private void StartBossTimedTriggerEvent()
        {
            lock(BossTimerLocker)
            {
                // some preliminary checks
                if (ActiveTriggeredTimers.ContainsKey(bossId))
                {
                    return;
                }
                else if (ActiveTriggeredEvents.Any(x => x.EventId == bossId))
                {
                    var te = ActiveTriggeredEvents.First(x => x.EventId == bossId);
                    ActiveTriggeredEvents.Remove(te);
                }

                // start timers RngGeneration.Rng.Next(-30, 31)
                Timer bossTriggerTimer = new Timer(new TimeSpan(0, 0, 1, 0).TotalMilliseconds);
                bossTriggerTimer.Elapsed += BossTimedTriggerEvent_Elapsed;
                bossTriggerTimer.Enabled = true;
                bossTriggerTimer.Start();

                ActiveTriggeredTimers[bossId] = bossTriggerTimer;
            }
        }

        private void BossTimedTriggerEvent_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock(BossTimerLocker)
            {
                // check for already running event, before starting new one
                if (ActiveTriggeredEvents.Any(x => x.EventId == bossId))
                {
                    var tdfe = ActiveTriggeredEvents.First(x => x.EventId == bossId);
                    if (DateTime.Now - tdfe.CreatedDate > tdfe.Timeout && !tdfe.TimedOut)
                    {
                        tdfe.TriggerWave();
                        tdfe.TimedOut = true;

                        foreach (var v in tdfe.GetCommandStrings())
                        {
                            if (ActiveTriggeredCommandStrings.Any(x => x.Equals(v, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                var lala = ActiveTriggeredCommandStrings.First(x => x.Equals(v, StringComparison.InvariantCultureIgnoreCase));
                                ActiveTriggeredCommandStrings.Remove(lala);
                            }
                        }

                        foreach (var v in ActiveChannels)
                        {
                            SystemController.Instance.Respond(v, tdfe.TimeoutMessage, null);
                        }

                        return;
                    }
                    else if (DateTime.Now - tdfe.CreatedDate > tdfe.Cooldown)
                    {
                        ActiveTriggeredEvents.Remove(tdfe);
                        foreach (var v in ActiveChannels)
                            SystemController.Instance.Respond(v, tdfe.CooldownMessage, null);
                        return;
                    }
                    else if (tdfe._EventState == TriggeredEventState.Resolved)
                    {
                        ActiveTriggeredEvents.Remove(tdfe);
                        foreach (var v in ActiveChannels)
                            SystemController.Instance.Respond(v, "This event has been resolved. For now...", null);
                    }
                    else
                        return;
                }

                TriggeredEvent te = new TriggeredEvent(bossId, Api)
                {
                    ActiveRooms = ActiveChannels
                };

                // create new event and add to pool
                ActiveTriggeredEvents.Add(te);
                ActiveTriggeredCommandStrings.AddRange(te.GetCommandStrings());
                
                te.StartEvent();

                te.StartMessage = te.StartMessage.Replace("{cmdc}", CommandChar);

                foreach(var v in ActiveChannels)
                {
                    SystemController.Instance.Respond(v, te.StartMessage, null);
                }
            }
        }
    }
}
