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

            toSend += $"Alias: {card.Alias} | UserId: {card.UserId} | Magic: [color={card.MainMagic.Color}][b]{card.MainMagic.Name}[/b][/color]";
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
                //new Command(CommandStrings.MoreHelp, BotCommandRestriction.Whisper, CommandSecurity.None, "returns additional help options"),
                //new Command(CommandStrings.MoreHelpLong, BotCommandRestriction.Whisper, CommandSecurity.None, "returns additional help options"),

                new Command(CommandStrings.Create, BotCommandRestriction.Both, CommandSecurity.None, "creates a new character"),
                //new Command(CommandStrings.Roll, BotCommandRestriction.Both, CommandSecurity.None, "rolls in the gatcha"),
                //new Command(CommandStrings.Dive, BotCommandRestriction.Both, CommandSecurity.None, "dives into the dungeon"),
                //new Command(CommandStrings.Set, BotCommandRestriction.Both, CommandSecurity.None, "handles various set commands", "help"),
                //new Command(CommandStrings.Box, BotCommandRestriction.Both, CommandSecurity.None, "handles various inventory commands", "help"),
                new Command(CommandStrings.Card, BotCommandRestriction.Both, CommandSecurity.None, "displays details about the character"),
                //new Command(CommandStrings.Smite, BotCommandRestriction.Both, CommandSecurity.Ops, "displays short details about the character"),

                //new Command(CommandStrings.Upgrade, BotCommandRestriction.Whisper, CommandSecurity.None, "upgrades your sockets for gold", "help"),

                //new Command(CommandStrings.Equip, BotCommandRestriction.Both, CommandSecurity.None, "equips items"),
                //new Command(CommandStrings.Unequip, BotCommandRestriction.Both, CommandSecurity.None, "unequips items"),
                //new Command(CommandStrings.Divefloor, BotCommandRestriction.Both, CommandSecurity.None, "sets your default dive floor"),
                //new Command(CommandStrings.DivefloorLong, BotCommandRestriction.Both, CommandSecurity.None, "sets your default dive floor"),
                new Command(CommandStrings.Status, BotCommandRestriction.Both, CommandSecurity.Ops, "sets the bot status"),

                //new Command(CommandStrings.Bully, BotCommandRestriction.Both, CommandSecurity.None, "attempts to bully your target"),
                //new Command(CommandStrings.Submit, BotCommandRestriction.Both, CommandSecurity.None, "submits to your bully and transfers them your stamina"),
                //new Command(CommandStrings.Fight, BotCommandRestriction.Both, CommandSecurity.None, "fight your bully and initiate a pvp duel"),
                //new Command(CommandStrings.Focus, BotCommandRestriction.Both, CommandSecurity.None, "focuses on a specific stat for enhanced  increases to it. 1 at a time."),

                //new Command(CommandStrings.Verbose, BotCommandRestriction.Both, CommandSecurity.None, "enables enhanced combat logging in whispers only."),

                //new Command(CommandStrings.BaseCooldown, BotCommandRestriction.Both, CommandSecurity.None, "sets the dive stamina required."),
                //new Command(CommandStrings.StardustCooldown, BotCommandRestriction.Both, CommandSecurity.None, "sets the gatcha stardust required."),
                //new Command(CommandStrings.Reset, BotCommandRestriction.Both, CommandSecurity.None, "resets a specific user's specific cooldown timer."),
                //new Command(CommandStrings.Gift, BotCommandRestriction.Both, CommandSecurity.None, "gives a gift of gold to another user."),

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
                //case CommandStrings.BaseCooldown:
                //    {
                //        if (!isOp)
                //            return true;
                //
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                //        {
                //            break;
                //        }
                //
                //        BaseCooldownAction(channel, message, card);
                //    }
                //    break;
                //case CommandStrings.Smite:
                //    {
                //        if (!isOp)
                //            return true;
                //
                //        if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard card))
                //        {
                //            break;
                //        }
                //
                //        Data.DataDb.DeleteCard(card.Name);
                //        Respond(channel, $"{card.DisplayName} has been smote.", user);
                //    }
                //    break;
                //case CommandStrings.StardustCooldown:
                //    {
                //        if (!isOp)
                //            return true;
                //
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                //        {
                //            break;
                //        }
                //
                //        GatchaRequiredStardustAction(channel, message, card);
                //    }
                //    break;
                //case CommandStrings.Reset:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out _))
                //        {
                //            break;
                //        }
                //
                //        if (!isOp)
                //            return true;
                //
                //        //List<string> messageBroken = 
                //        string cdTarget = message.Substring(message.Length - (LastUsedCooldownType.LastDive.GetDescription().ToString() + " ").Length).Replace(" ", "");
                //        string tu = message.Substring(0, message.Length - cdTarget.Length);
                //        tu = tu.TrimEnd(' ');
                //
                //        if (!Data.DataDb.UserExists(tu))
                //        {
                //            Respond(null, $"Unable to resolve card for user: {tu}. Format: {CommandChar}{CommandStrings.Reset} *Target* *CooldownType*", user);
                //            break;
                //        }
                //
                //        if (!RngGeneration.TryGetCard(tu, out Cards.PlayerCard targetCard))
                //        {
                //            Console.WriteLine($"Unknown error attempting to get card for user: {tu}");
                //            break;
                //        }
                //
                //        bool successful = false;
                //        foreach (var v in Enum.GetValues(typeof(LastUsedCooldownType)))
                //        {
                //            if (v.GetDescription().Equals(cdTarget, StringComparison.InvariantCultureIgnoreCase))
                //            {
                //                targetCard.LastTriggeredCds[(LastUsedCooldownType)v] = DateTime.MinValue;
                //                Data.DataDb.UpdateCard(targetCard);
                //                successful = true;
                //                break;
                //            }
                //        }
                //
                //        if (successful)
                //        {
                //            Respond(null, $"Succesfully reset {targetCard.Name}'s {cdTarget}!", user);
                //
                //        }
                //        else
                //        {
                //            Respond(null, $"Unknown error attempting to reset cooldown for {targetCard.Name}.", user);
                //        }
                //    }
                //    break;
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

        //public void BaseCooldownAction(string channel, string message, Cards.PlayerCard pc)
        //{
        //    try
        //    {
        //        int cd = Convert.ToInt32(message);
        //        REQUIRED_DIVE_STAMINA = cd;
        //        Respond(channel, $"[b]Base stamina required to dive has been set to: [color=green]{REQUIRED_DIVE_STAMINA}[/color]. Happy hunting![/b]", pc.Name);
        //    }
        //    catch
        //    {
        //        Respond(channel, $"Invalid format. Try again! ex: {CommandChar}{CommandStrings.BaseCooldown} 20 (for 20 stamina)", pc.Name);
        //    }
        //}

        //public void GatchaRequiredStardustAction(string channel, string message, Cards.PlayerCard pc)
        //{
        //    try
        //    {
        //        int cd = Convert.ToInt32(message);
        //        COST_TO_ROLL = cd;
        //        Respond(channel, $"[b]Base Stardust required to roll in the Gatcha has been set to: [color=green]{REQUIRED_DIVE_STAMINA}[/color]. Good luck![/b]", pc.Name);
        //    }
        //    catch
        //    {
        //        Respond(channel, $"Invalid format. Try again! ex: {CommandChar}{CommandStrings.StardustCooldown} 20 (for 20 stamina)", pc.Name);
        //    }
        //}

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
                //case CommandStrings.Gift:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                //        {
                //            break;
                //        }
                //
                //        GiftAction(channel, message, card);
                //    }
                //    break;
                //case CommandStrings.Reset:
                //    {
                //
                //    }
                //    break;
                //case CommandStrings.Roll:
                //    {
                //        if (string.IsNullOrWhiteSpace(message) || !int.TryParse(message, out int rollCount))
                //        {
                //            rollCount = 1;
                //        }
                //
                //        RollAction(rollCount, user, channel);
                //    }
                //    break;
                //case CommandStrings.Verbose:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                //        {
                //            break;
                //        }
                //
                //        ucard.Verbose = !ucard.Verbose;
                //        Data.DataDb.UpdateCard(ucard);
                //        Respond(null, $"Combat verbosity changed to {(ucard.Verbose == true ? "[color=green]Enabled[/color]" : "[color=orange]Disabled[/color]")}. This only takes effect in whispers.", ucard.Name);
                //    }
                //    break;
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
                //case CommandStrings.MoreHelpLong:
                //case CommandStrings.MoreHelp:
                //    {
                //        MoreHelpAction(user);
                //    }
                //    break;
                case CommandStrings.Create:
                    {
                        CreateCharacter(user);
                    }
                    break;
                //case CommandStrings.Dive:
                //    {
                //        DiveAction(channel, user, message);
                //    }
                //    break;
                //case CommandStrings.Set:
                //    {
                //        SetAction(channel, message, user);
                //    }
                //    break;
                //case CommandStrings.Box:
                //    {
                //        InventoryAction(channel, message, user);
                //    }
                //    break;
                case CommandStrings.Card:
                    {
                        CardAction(channel, user);
                    }
                    break;
                //case CommandStrings.Equip:
                //    {
                //        EquipAction(channel, message, user);
                //    }
                //    break;
                //case CommandStrings.Unequip:
                //    {
                //        UnequipAction(channel, message, user);
                //    }
                //    break;
                //case CommandStrings.DivefloorLong:
                //case CommandStrings.Divefloor:
                //    {
                //        SetDiveFloorAction(channel, message, user);
                //    }
                //    break;
                //case CommandStrings.RequestDuelLong:
                //case CommandStrings.RequestDuel:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                //        {
                //            break;
                //        }
                //
                //        if (!Data.DataDb.UserExists(message))
                //        {
                //            Respond(null, $"Unable to resolve card for user: {message}", user);
                //            break;
                //        }
                //
                //        if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard card))
                //        {
                //            Console.WriteLine($"Unknown error attempting to get card: {message}");
                //            break;
                //        }
                //
                //        Respond(null, $"{ucard.DisplayName} has challenged you to a duel. [sub][color=green]Accept by responding to me with: {CommandChar}{CommandStrings.AcceptDuelLong}[/color] | [color=red]Decline by responding to me with: {CommandChar}{CommandStrings.DenyDuelLong}[/color][/sub]", card.Name);
                //    }
                //    break;
                //case CommandStrings.CancelDuelLong:
                //case CommandStrings.CancelDuel:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                //        {
                //            break;
                //        }
                //
                //    }
                //    break;
                //case CommandStrings.AcceptDuelLong:
                //case CommandStrings.AcceptDuel:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                //        {
                //            break;
                //        }
                //
                //    }
                //    break;
                //case CommandStrings.DenyDuelLong:
                //case CommandStrings.DenyDuel:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                //        {
                //            break;
                //        }
                //
                //    }
                //    break;
                //case CommandStrings.StartDuelLong:
                //case CommandStrings.StartDuel:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                //        {
                //            break;
                //        }
                //
                //    }
                //    break;
                //case CommandStrings.Focus:
                //    {
                //        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                //        {
                //            break;
                //        }
                //
                //        if (!Enum.GetNames(typeof(StatTypes)).Any(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase)))
                //        {
                //            Respond(null, $"Unable to resolve valid focusable stat for value: {message}", user);
                //            break;
                //        }
                //
                //        StatTypes toFocus = (StatTypes)Enum.Parse(typeof(StatTypes), message, true);
                //
                //        if (!RngGeneration.GetAllFocusableStats().Contains(toFocus))
                //        {
                //            Respond(null, $"Unable to resolve valid focusable stat for value: {message}", user);
                //            break;
                //        }
                //        ucard.GetStat(StatTypes.Foc);
                //        ucard.SetStat(StatTypes.Foc, (int)toFocus);
                //        Data.DataDb.UpdateCard(ucard);
                //
                //        Respond(null, $"Successfully set stat focus to: {message}", user);
                //    }
                //    break;
                //case CommandStrings.Bully:
                //    {
                //        // setup our vars 
                //        Encounter enc = null;
                //
                //        // do our basic bully checks
                //        if (!BasicBullyChecks(channel, user, message, out Cards.PlayerCard card, out Cards.PlayerCard targetCard))
                //            break;
                //
                //        // start a new encounter
                //        enc = new Encounter(targetCard.BaseCooldowns[PlayerActionTimeoutTypes.BullyAttemptCooldown], card.Name)
                //        {
                //            Creator = card.Name
                //        };
                //
                //        // add bully
                //        enc.AddParticipant(1, card);
                //        card.LastTriggeredCds[LastUsedCooldownType.LastBully] = enc.CreationDate;
                //
                //        // add the bullied target
                //        enc.AddParticipant(2, targetCard);
                //        targetCard.LastTriggeredCds[LastUsedCooldownType.LastBullied] = enc.CreationDate;
                //
                //        // add the encounter
                //        encounterTracker.AddEncounter(enc);
                //        Data.DataDb.UpdateCard(card);
                //        Data.DataDb.UpdateCard(targetCard);
                //        Respond(channel, $"{card.DisplayName} is attempting to bully you, {targetCard.DisplayName}! [sub][color=pink]You can submit by replying with: {CommandChar}{CommandStrings.Submit}[/color] | [color=red]You can fight back by replying with: {CommandChar}{CommandStrings.Fight}[/color][/sub]", string.Empty);
                //    }
                //    break;
                //case CommandStrings.Submit:
                //    {
                //        // do some basic submit checks here
                //        if (!BasicSubmitChecks(channel, user, message, out Cards.PlayerCard card))
                //            break;
                //
                //        // find encounter
                //        Encounter enc = null;
                //        foreach (var v in encounterTracker.PendingEncounters)
                //        {
                //            if (v.Value.EncounterType == EncounterTypes.Bully && v.Value.Participants.Any(x => x.Participant.Name.Equals(card.Name, StringComparison.InvariantCultureIgnoreCase)))
                //            {
                //                enc = v.Value;
                //                break;
                //            }
                //        }
                //
                //        // break out if we couldn't find it for some reason
                //        if (enc == null)
                //            break;
                //
                //        // bail out if we can't find our bully
                //        RngGeneration.TryGetCard(enc.Creator, out Cards.PlayerCard targetCard);
                //        if (null == targetCard)
                //            break;
                //
                //        // bail out if we've timed out
                //        if (enc.HasTimedOut())
                //        {
                //            card.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
                //            targetCard.LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
                //            encounterTracker.KillEncounter(enc);
                //            break;
                //        }
                //
                //        // submit calc
                //        string submitStr = string.Empty;
                //        TimeSpan submitVal = new TimeSpan(0, 20, 0);
                //
                //        int staLost;
                //        if (card.GetStat(StatTypes.Sta) > submitVal.TotalSeconds)
                //        {
                //            card.AddStat(StatTypes.Sta, -(submitVal.TotalSeconds));
                //            staLost = Convert.ToInt32(submitVal.TotalSeconds);
                //        }
                //        else
                //        {
                //            staLost = card.GetStat(StatTypes.Sta);
                //            card.SetStat(StatTypes.Sta, 0);
                //        }
                //
                //        //double pants = 90.0 / ucard.GetStat(StatTypes.StM);
                //        double whatever = XPMULT * staLost;
                //
                //        submitStr += $"{card.DisplayName}, you submit to {targetCard.DisplayName}'s aggressive bullying, losing {whatever} stamina. ";
                //
                //        // bully calc
                //        int staWon;
                //        if (targetCard.GetStat(StatTypes.Sta) + staLost >= targetCard.GetStat(StatTypes.StM))
                //        {
                //            staWon = Convert.ToInt32(targetCard.GetStat(StatTypes.StM) - targetCard.GetStat(StatTypes.Sta));
                //            targetCard.SetStat(StatTypes.Sta, targetCard.GetStat(StatTypes.StM));
                //        }
                //        else
                //        {
                //            staWon = staLost;
                //            targetCard.AddStat(StatTypes.Sta, staLost);
                //        }
                //
                //        whatever = XPMULT * staWon;
                //        submitStr += $"{targetCard.DisplayName}, you gain {whatever} stamina for your successful bullying.";
                //
                //        // finalize
                //        targetCard.AddStat(StatTypes.Bly, 1, false, false, false);
                //        card.AddStat(StatTypes.Sbm, 1, false, false, false);
                //        Data.DataDb.UpdateCard((targetCard as Cards.PlayerCard));
                //        Data.DataDb.UpdateCard(card);
                //
                //        // end the encounter
                //        encounterTracker.KillEncounter(enc);
                //
                //        // respond
                //        Respond(channel, submitStr, string.Empty);
                //    }
                //    break;
                //case CommandStrings.Fight:
                //    {
                //        // do some basic submit checks here
                //        if (!BasicSubmitChecks(channel, user, message, out Cards.PlayerCard card))
                //            break;
                //
                //        // find encounter
                //        Encounter enc = null;
                //        foreach (var v in encounterTracker.PendingEncounters)
                //        {
                //            if (v.Value.EncounterType == EncounterTypes.Bully && v.Value.Participants.Any(x => x.Participant.Name.Equals(card.Name, StringComparison.InvariantCultureIgnoreCase)))
                //            {
                //                enc = v.Value;
                //                break;
                //            }
                //        }
                //
                //        // break out if we couldn't find it for some reason
                //        if (enc == null)
                //            break;
                //
                //        // bail out if we can't find our bully
                //        RngGeneration.TryGetCard(enc.Creator, out Cards.PlayerCard targetCard);
                //        if (null == targetCard)
                //            break;
                //
                //        // bail out if we've timed out
                //        if (enc.HasTimedOut())
                //        {
                //            card.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
                //            targetCard.LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
                //            encounterTracker.KillEncounter(enc);
                //            break;
                //        }
                //
                //        // refresh cards being used
                //        enc.Participants = new List<EncounterCard>();
                //        enc.AddParticipant(0, card);
                //        enc.AddParticipant(1, targetCard);
                //
                //        // start fight here
                //        Respond(channel, $"A fight is breaking out between {(string.IsNullOrEmpty(card.DisplayName) ? card.Name : card.DisplayName)} and {targetCard.DisplayName}!", string.Empty);
                //        enc.StartEncounter(EncounterTypes.Bully);
                //        var encResults = enc.RunEncounter();
                //
                //        Respond(channel, $"There was a winner but I haven't parsed the results yet.", string.Empty);
                //        Data.DataDb.UpdateCard(card);
                //        Data.DataDb.UpdateCard(targetCard);
                //    }
                //    break;
                //case CommandStrings.Upgrade:
                //    {
                //        UpgradeStuff(user, message, channel);
                //    }
                //    break;
            }

            return false;
        }

        //public bool BasicSubmitChecks(string channel, string user, string message, out Cards.PlayerCard card)
        //{
        //    // if you don't exist, get outta here
        //    if (!RngGeneration.TryGetCard(user, out card))
        //    {
        //        return false;
        //    }
        //
        //    // if you ask for help, print it and get outta here
        //    if (message.Equals(CommandStrings.Help, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        BullyHelpAction(user);
        //        return false;
        //    }
        //
        //    // if you're trying to dm a bully, get outta here
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        return false;
        //    }
        //
        //    return true;
        //}

        //public bool BasicBullyChecks(string channel, string user, string message, out Cards.PlayerCard card, out Cards.PlayerCard targetCard)
        //{
        //    targetCard = null;
        //
        //    // if you don't exist, get outta here
        //    if (!RngGeneration.TryGetCard(user, out card))
        //    {
        //        return false;
        //    }
        //
        //    // if you ask for help, print it and get outta here
        //    if (message.Equals(CommandStrings.Help, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        BullyHelpAction(user);
        //        return false;
        //    }
        //
        //    // if you're trying to dm a bully, get outta here
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        return false;
        //    }
        //
        //    // if the target of the bully attempt can't be found, get outta here
        //    if (!RngGeneration.TryGetCard(message, out targetCard))
        //    {
        //        Console.WriteLine($"Unknown error attempting to get card: {message}");
        //        return false;
        //    }
        //
        //    // if the target has been bullied too recently, get outta here
        //    if (DateTime.Now - targetCard.LastTriggeredCds[LastUsedCooldownType.LastBullied] < targetCard.BaseCooldowns[PlayerActionTimeoutTypes.HasBeenBulliedCooldown])
        //    {
        //        Respond(channel, $"{targetCard.DisplayName} has been bullied too recently, {card.DisplayName}.", user);
        //        return false;
        //    }
        //
        //    // if you try to bully yourself, get outta here
        //    if (targetCard.Name == card.Name)
        //    {
        //        Respond(channel, $"If you want to bully yourself, {targetCard.DisplayName}, go find a sad anime to watch.", user);
        //        return false;
        //    }
        //
        //    // if there's already an encounter in progress with you in it, get outta here
        //    if (encounterTracker.PendingEncounters.ContainsKey(card.Name))
        //    {
        //        Respond(channel, $"You're already attempting to bully a target, {targetCard.DisplayName}.", user);
        //        return false;
        //    }
        //
        //    // if there's already an encounter in progress with your target in it, get outta here
        //    //if (encounterTracker.PendingEncounters
        //    //    .Where(x => x.Value.EncounterType == EncounterTypes.Bully)
        //    //    .Any(y => y.Value.Participants
        //    //    .Any(z => z.Value
        //    //    .Any(a => a.Name.Equals(targetCard.Name)))))
        //    //{
        //    //    Respond(channel, $"Your bully target is already in a pending bully encounter, {targetCard.DisplayName}.", user);
        //    //    return false;
        //    //}
        //
        //    // check if any encounters have timed out and kill the encounter before starting a new one
        //    int etoc = encounterTracker.PendingEncounters.Count;
        //    var etocL = encounterTracker.PendingEncounters.Keys.ToList();
        //    List<Encounter> toKill = new List<Encounter>();
        //    for (int x = 0; x < etoc; x++)
        //    {
        //        var curEnc = encounterTracker.PendingEncounters[etocL[x]];
        //        if (curEnc.HasTimedOut())
        //        {
        //            // reset any info that might matter
        //            foreach (var v in curEnc.Participants)
        //            {
        //                //foreach (var pc in v)
        //                {
        //                    if (!RngGeneration.TryGetCard(v.Participant.Name, out Cards.PlayerCard upc))
        //                        continue;
        //
        //                    if (upc.Name.Equals(curEnc.Creator, StringComparison.InvariantCultureIgnoreCase))
        //                    {
        //                        upc.LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
        //                    }
        //                    else
        //                    {
        //                        upc.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
        //                    }
        //
        //                    Data.DataDb.UpdateCard(upc);
        //                }
        //            }
        //
        //            toKill.Add(encounterTracker.PendingEncounters[etocL[x]]);
        //        }
        //        else if (curEnc.EncounterStatus == EncounterStatus.Resolved)
        //        {
        //            toKill.Add(curEnc);
        //        }
        //    }
        //
        //    // kill the encounters that need killed
        //    foreach (var v in toKill)
        //        encounterTracker.KillEncounter(v);
        //
        //    return true;
        //}

        //public void MoreHelpAction(string sendingUser)
        //{
        //    string toSend = string.Empty;
        //
        //    toSend += $"[color=white]" +
        //        $"         Welcome to [b][color={"red"}]({"More"}) {CommandStrings.Help}[/color][/b]!" +
        //        $"\\n" +
        //        $"\\n[b]Stats:[/b]" +
        //        $"\\n";
        //
        //    foreach (var v in RngGeneration.GetAllFocusableStats())
        //    {
        //        if (v == StatTypes.Vit) toSend += $"\\n{v.ToString()} ⇒ Sustainability. 'Death' when this reaches 0.";
        //        if (v == StatTypes.Atk) toSend += $"\\n{v.ToString()} ⇒ Chance to hit with attacks.";
        //        if (v == StatTypes.Dmg) toSend += $"\\n{v.ToString()} ⇒ Base damage.";
        //        if (v == StatTypes.Dex) toSend += $"\\n{v.ToString()} ⇒ Physical damage multiplier.";
        //        if (v == StatTypes.Int) toSend += $"\\n{v.ToString()} ⇒ Magical damage multiplier.";
        //        if (v == StatTypes.Con) toSend += $"\\n{v.ToString()} ⇒ Temporary health. Reduced before [vit] damage.";
        //        if (v == StatTypes.Crc) toSend += $"\\n{v.ToString()} ⇒ Critical strike chance modifier.";
        //        if (v == StatTypes.Crt) toSend += $"\\n{v.ToString()} ⇒ Critical strike damage additional multiplier.";
        //        if (v == StatTypes.Ats) toSend += $"\\n{v.ToString()} ⇒ Reduces the high/low spread of various combat actions.";
        //        if (v == StatTypes.Spd) toSend += $"\\n{v.ToString()} ⇒ Determines order of attack during combat.";
        //        if (v == StatTypes.Pdf) toSend += $"\\n{v.ToString()} ⇒ Physical damage reduction modifier.";
        //        if (v == StatTypes.Mdf) toSend += $"\\n{v.ToString()} ⇒ Magical damage reduction modifier.";
        //        if (v == StatTypes.Eva) toSend += $"\\n{v.ToString()} ⇒ Chance to evade incoming attacks.";
        //    }
        //
        //    toSend += $"\\n" +
        //        $"\\n[b]Damage Types:[/b] ";
        //
        //    foreach (var v in Enum.GetValues(typeof(DamageTypes)))
        //    {
        //        if ((DamageTypes)v == DamageTypes.None)
        //            continue;
        //
        //        toSend += $"[color={((DamageTypes)v).GetDescription()}]{((DamageTypes)v).ToString()}[/color] ";
        //    }
        //
        //    toSend += $"\\n" +
        //        $"\\n[b]Additional Commands:[/b] " +
        //        $"\\n" +
        //        $"\\n[color={"green"}]{CommandChar}{CommandStrings.DivefloorLong} ⁕Value⁕[/color] sets your default dive depth." +
        //        $"\\n[color={"green"}]{CommandChar}{CommandStrings.Gift} ⁕Target⁕ ⁕Value⁕[/color] gives a gift of gold to another user." +
        //        $"\\n[color={"green"}]{CommandChar}{CommandStrings.Focus} ⁕Target⁕[/color] focuses on a specific stat to enhance while leveling." +
        //        $"\\n[color={"green"}]{CommandChar}{CommandStrings.Verbose}[/color] sets your combat verbosity when fighting in whispers and fight-channels." +
        //        $"\\n" +
        //        $"\\n[color={"green"}]{CommandChar}{CommandStrings.BaseCooldown} ⁕Value⁕[/color] mod-only - changes dive stamina required." +
        //        $"\\n[color={"green"}]{CommandChar}{CommandStrings.StardustCooldown} ⁕Value⁕[/color] mod-only - changes stardust required for the Gatcha.";
        //
        //
        //    toSend += $"\\n" +
        //        $"\\n[b]Cooldown Vars:[/b] ";
        //    foreach (var v in Enum.GetValues(typeof(PlayerActionTimeoutTypes)))
        //    {
        //        toSend += v.GetDescription();
        //        if (((PlayerActionTimeoutTypes)v) != (PlayerActionTimeoutTypes)(Enum.GetValues(typeof(PlayerActionTimeoutTypes)).Length - 1))
        //            toSend += " | ";
        //    }
        //
        //    toSend += $"[/color]";
        //
        //    Respond(null, toSend, sendingUser);
        //}

        //public void BullyHelpAction(string sendingUser)
        //{
        //    string toSend = string.Empty;
        //
        //    toSend += $"[color=white]" +
        //        $"         Welcome to [b][color={"cyan"}]{CommandStrings.Bully} {CommandStrings.Help}[/color][/b]!" +
        //        $"\\n    " +
        //        $"\\nBullying others is rude, but sometimes it's still worth while to do. When you [color={"cyan"}]{CommandChar}{CommandStrings.Bully}[/color]" +
        //        $"\\nsomeone, they can choose to either [color={"cyan"}]{CommandChar}{CommandStrings.Submit}[/color] or [color={"cyan"}]{CommandChar}{CommandStrings.Fight}[/color]!" +
        //        $"\\n" +
        //        $"\\nType [color={"cyan"}]{CommandChar}{CommandStrings.Bully} ⁕Target⁕[/color] to bully a specific target." +
        //        $"\\nExample: [color={"cyan"}]{CommandChar}{CommandStrings.Bully} Astral Mage[/color]" +
        //        $"\\n" +
        //        $"\\nIf you find yourself being bullied, you can either give in and submit, which gives" +
        //        $"\\nsome of your saved up stamina to whomever bullied you." +
        //        $"\\n" +
        //        $"\\nType [color={"cyan"}]{CommandChar}{CommandStrings.Submit}[/color] to submit to your bully and lose up to 15 stamina." +
        //        $"\\n" +
        //        $"\\nAlternatively, Type [color={"cyan"}]{CommandChar}{CommandStrings.Fight}[/color] to try and fight your bully!" +
        //        $"\\n" +
        //        $"\\nFighting your bully begin a Pvp combat encounter. You and your bully fight each" +
        //        $"\\nother over multiple rounds. Whichever one of you wins more rounds is considered" +
        //        $"\\nthe victor. The loser ends up submitting despite the struggle, and loses up to" +
        //        $"\\n30 stamina." +
        //        $"\\n" +
        //        $"\\nIf the Bully wins, they recieve any stamina the loser lost, up to your maximium." +
        //        $"[/color]";
        //
        //    Respond(null, toSend, sendingUser);
        //}

        //public void UpgradeHelpAction(string sendingUser)
        //{
        //    string toSend = string.Empty;
        //
        //    toSend += $"[color=white]" +
        //        $"         Welcome to [b][color={"pink"}]{CommandStrings.Upgrade} {CommandStrings.Help}[/color][/b]!" +
        //        $"\\n    " +
        //        $"\\nAll commands in this section are called in format: [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} ⁕Value⁕[/color]." +
        //        $"\\nExample: Type [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} 1[/color]" +
        //        $"\\n" +
        //        $"\\n[{CommandStrings.Upgrade}] is a system that allows you to upgrade items that you have" +
        //        $"\\nfound during your adventures. Upgrading items costs gold, which you" +
        //        $"\\ncan get from diving in the dungeon. Upgrading an item will increase" +
        //        $"\\nit's rarity, as well as increasing a few of that item's stastics. In" +
        //        $"\\nsome cases, an item may even gain entirely new stats! Equipped items," +
        //        $"\\nor items in your box are eligable to be upgraded." +
        //        $"\\n" +
        //        $"\\nType [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} 1[/color] to upgrade your weapon slot." +
        //        $"\\nType [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} 2[/color] to upgrade your armor slot." +
        //        $"\\nType [color={"pink"}]{CommandChar}{CommandStrings.Upgrade} 3[/color] to upgrade your passive slot." +
        //        $"[/color]";
        //
        //    Respond(null, toSend, sendingUser);
        //}

        //public void SetDiveFloorAction(string channel, string message, string user)
        //{
        //    if (!Data.DataDb.UserExists(user))
        //        return;
        //
        //    if (!int.TryParse(message, out int res) || FloorDb.GetAllFloors().Count < res || res <= 0)
        //    {
        //        Respond(channel, $"{user}, you must specify a valid floor", user);
        //        return;
        //    }
        //
        //    if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard pc))
        //        return;
        //
        //    pc.SetStat(StatTypes.Dff, res);
        //    Data.DataDb.UpdateCard(pc);
        //    Respond(channel, $"{pc.DisplayName}, you'll now dive to floor {res} by default.", user);
        //}

        //public void EquipAction(string channel, string message, string user)
        //{
        //    if (!Data.DataDb.UserExists(user))
        //        return;
        //
        //    if (!int.TryParse(message, out int res))
        //    {
        //        Respond(channel, $"{user}, you must specify a valid box slot you're attempting to equip. Ex: {CommandStrings.Equip} 1", user);
        //        return;
        //    }
        //
        //    if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
        //        return;
        //
        //    if (card.Inventory.Count < res || res < 1)
        //    {
        //        Respond(channel, $"{user}, you must specify a valid box slot you're attempting to equip. Ex: {CommandStrings.Equip} 1", user);
        //        return;
        //    }
        //
        //    Socket toAdd = card.Inventory[res - 1];
        //    if (!card.AvailableSockets.Contains(toAdd.SocketType))
        //    {
        //        Respond(channel, $"{user}, you must specify an equipment type you're allowed to equip. Ex: {CommandStrings.Equip} 1", user);
        //        return;
        //    }
        //
        //    // CHECK FOR CLASS RESTRICTIONS HERE
        //
        //    // ---------------------------------
        //
        //
        //    int numAllowed = card.AvailableSockets.Count(x => x == toAdd.SocketType);
        //    int numEquipped = card.ActiveSockets.Count(x => x.SocketType == toAdd.SocketType);
        //    Socket toRemove = null;
        //
        //    if (numEquipped >= numAllowed)
        //    {
        //        toRemove = card.ActiveSockets.First(x => x.SocketType == toAdd.SocketType);
        //        card.ActiveSockets.Remove(toRemove);
        //        card.Inventory.Add(toRemove);
        //    }
        //
        //    card.ActiveSockets.Add(toAdd);
        //    card.Inventory.Remove(toAdd);
        //
        //    string replyMessage = $"{card.DisplayName}, you've equipped your {toAdd.GetName()}.";
        //    if (toRemove != null) replyMessage += $" You had to unequip your {toRemove.GetName()} to do so.";
        //
        //    Data.DataDb.UpdateCard(card);
        //    Respond(channel, replyMessage, user);
        //}

        //public void UnequipAction(string channel, string message, string user)
        //{
        //    if (!Data.DataDb.UserExists(user))
        //        return;
        //
        //    if (!int.TryParse(message, out int res))
        //    {
        //        Respond(channel, $"{user}, you must specify a valid equipment slot you're attempting to un-equip. Ex: {CommandStrings.Unequip} 1", user);
        //        return;
        //    }
        //
        //    if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
        //        return;
        //
        //    if (res < 1)
        //    {
        //        Respond(channel, $"{user}, you must specify a valid equipment slot you're attempting to un-equip. Ex: {CommandStrings.Unequip} 1", user);
        //        return;
        //    }
        //
        //    if (card.Inventory.Count >= card.MaxInventory)
        //    {
        //        Respond(channel, $"{user}, you must have free inventory space to {CommandStrings.Unequip} gear.", user);
        //        return;
        //    }
        //
        //    Socket toUnequip = null;
        //    SocketTypes unequipType;
        //    if (res == 1)
        //        unequipType = SocketTypes.Weapon;
        //    else if (res == 2)
        //        unequipType = SocketTypes.Armor;
        //    else if (res == 3)
        //        unequipType = SocketTypes.Passive;
        //    else
        //        return;
        //
        //    if (card.ActiveSockets.Count(x => x.SocketType == unequipType) > 0)
        //    {
        //        toUnequip = card.ActiveSockets.First(x => x.SocketType == unequipType);
        //    }
        //    else
        //    {
        //        return;
        //    }
        //
        //    card.ActiveSockets.Remove(toUnequip);
        //    card.Inventory.Add(toUnequip);
        //    Data.DataDb.UpdateCard(card, true);
        //    Respond(channel, $"{card.DisplayName}, you've unequipped your {toUnequip.GetRarityString()} {toUnequip.GetName()}", user);
        //}

        public void CardAction(string channel, string user)
        {
            if (!DataDb.Instance.UserExists(user))
                return;

            UserCard card = DataDb.Instance.GetCard(user);

            string toSend = string.Empty;

            toSend += $"Alias: {card.Alias} | UserId: {card.UserId} | Magic: [color={card.MainMagic.Color}][b]{card.MainMagic.Name}[/b][/color]";
            SystemController.Instance.Respond(channel, toSend, user);
            //Cards.PlayerCard pc = null;
            //
            //if (!string.IsNullOrWhiteSpace(message))
            //{
            //    if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard tempCard))
            //    {
            //        RngGeneration.TryGetCard(user, out tempCard);
            //        pc = tempCard;
            //    }
            //    else
            //    {
            //        pc = tempCard;
            //    }
            //}
            //else
            //{
            //    RngGeneration.TryGetCard(user, out pc);
            //}
            //
            //if (pc == null)
            //{
            //    Respond(channel, "Invalid character name.", user);
            //    return;
            //}
            //
            //// display card
            //string cardStr = string.Empty;
            //
            //string displayname = (string.IsNullOrWhiteSpace(pc.DisplayName)) ? pc.Name : pc.DisplayName;
            //string species = (string.IsNullOrWhiteSpace(pc.SpeciesDisplayName)) ? ((SpeciesTypes)pc.GetStat(StatTypes.Sps)).GetDescription() : pc.SpeciesDisplayName;
            //string cClass = (string.IsNullOrWhiteSpace(pc.ClassDisplayName)) ? ((ClassTypes)pc.GetStat(StatTypes.Cs1)).GetDescription() : pc.ClassDisplayName;
            //
            ////int artificalmax = 90;
            ////double pants = 90.0 / pc.GetStat(StatTypes.StM);
            //double whatever = XPMULT * pc.GetStat(StatTypes.Sta);
            //
            //cardStr += $"[b]Name: [/b]{displayname} | [b]Species: [/b]{species} | [b]Class: [/b]{cClass} ";
            //
            //if (!string.IsNullOrWhiteSpace(pc.Signature) && showsig)
            //{
            //    cardStr += $"\\n                      {pc.Signature}";
            //}
            //// Rank ‣ {pc.GetStat(StatTypes.Pvr)} | 
            //cardStr += $"\\n                      [sub][color=pink](Sta: {Math.Round(whatever, 0)}/{Math.Floor(XPMULT * pc.GetStat(StatTypes.StM))})[/color] [b]Lvl: [/b]{pc.GetStat(StatTypes.Lvl)} | [color=yellow][b]Gold: [/b]{pc.GetStat(StatTypes.Gld)}[/color] | [color=cyan][b]Stardust: [/b]{pc.GetStat(StatTypes.Sds)}[/color] | [color=red][b]Defeated: [/b]{pc.GetStat(StatTypes.Kil) + pc.GetStat(StatTypes.KiB)}[/color] 〰 " +
            //    $"[b]Pvp:[/b] Bullied ‣ {pc.GetStat(StatTypes.Bly)} | Submitted ‣ {pc.GetStat(StatTypes.Sbm)}[/sub]" +
            //    $"\\n                      ";
            //cardStr += pc.GetStatsString();
            //
            //string boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Sharpness)) ? $"[color=cyan]◉[/color]" : $"[color=white]•[/color]";
            //if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Weapon) > 0)
            //{
            //    foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Weapon))
            //    {
            //        cardStr += $"\\n                      {boonAddition} ";
            //        cardStr += $"{v.GetRarityString()} {v.GetName()} {v.GetShortDescription()}";
            //    }
            //}
            //else
            //{
            //    cardStr += $"\\n                      {boonAddition} ";
            //    cardStr += "Bare Hands";
            //}
            //
            //boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Resiliance)) ? $"[color=cyan]◉[/color]" : $"[color=white]•[/color]";
            //if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Armor) > 0)
            //{
            //    foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Armor))
            //    {
            //        cardStr += $"\\n                      {boonAddition} ";
            //        cardStr += $"{v.GetRarityString()} {v.GetName()} {v.GetShortDescription()}";
            //    }
            //}
            //else
            //{
            //    cardStr += $"\\n                      {boonAddition} ";
            //    cardStr += "Birthday Suit";
            //}
            //
            //boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Empowerment)) ? $"[color=cyan]◉[/color]" : $"[color=white]•[/color]";
            //if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Passive) > 0)
            //{
            //
            //    foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Passive))
            //    {
            //        cardStr += $"\\n                      {boonAddition} ";
            //        cardStr += $"{v.GetRarityString()} {v.GetName()} {v.GetShortDescription()}";
            //    }
            //}
            //else
            //{
            //    cardStr += $"\\n                      {boonAddition} ";
            //    cardStr += "Bashful Gaze";
            //}
            //
            //Respond(channel, cardStr, user);
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
