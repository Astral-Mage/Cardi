using Accord;
using ChatApi;
using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;

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
                Respond(channel, "Invalid character name.", user);
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

            Respond(channel, cardStr, user);
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
                new Command(CommandStrings.MoreHelp, BotCommandRestriction.Whisper, CommandSecurity.None, "returns additional help options"),
                new Command(CommandStrings.MoreHelpLong, BotCommandRestriction.Whisper, CommandSecurity.None, "returns additional help options"),

                new Command(CommandStrings.Create, BotCommandRestriction.Both, CommandSecurity.None, "creates a new character"),
                new Command(CommandStrings.Roll, BotCommandRestriction.Both, CommandSecurity.None, "rolls in the gatcha"),
                new Command(CommandStrings.Dive, BotCommandRestriction.Both, CommandSecurity.None, "dives into the dungeon"),
                new Command(CommandStrings.Set, BotCommandRestriction.Both, CommandSecurity.None, "handles various set commands", "help"),
                new Command(CommandStrings.Box, BotCommandRestriction.Both, CommandSecurity.None, "handles various inventory commands", "help"),
                new Command(CommandStrings.Card, BotCommandRestriction.Both, CommandSecurity.None, "displays details about the character"),
                new Command(CommandStrings.Smite, BotCommandRestriction.Both, CommandSecurity.Ops, "displays short details about the character"),

                new Command(CommandStrings.Upgrade, BotCommandRestriction.Whisper, CommandSecurity.None, "upgrades your sockets for gold", "help"),

                new Command(CommandStrings.Equip, BotCommandRestriction.Both, CommandSecurity.None, "equips items"),
                new Command(CommandStrings.Unequip, BotCommandRestriction.Both, CommandSecurity.None, "unequips items"),
                new Command(CommandStrings.Divefloor, BotCommandRestriction.Both, CommandSecurity.None, "sets your default dive floor"),
                new Command(CommandStrings.DivefloorLong, BotCommandRestriction.Both, CommandSecurity.None, "sets your default dive floor"),
                new Command(CommandStrings.Status, BotCommandRestriction.Both, CommandSecurity.Ops, "sets the bot status"),

                new Command(CommandStrings.Bully, BotCommandRestriction.Both, CommandSecurity.None, "attempts to bully your target"),
                new Command(CommandStrings.Submit, BotCommandRestriction.Both, CommandSecurity.None, "submits to your bully and transfers them your stamina"),
                new Command(CommandStrings.Fight, BotCommandRestriction.Both, CommandSecurity.None, "fight your bully and initiate a pvp duel"),
                new Command(CommandStrings.Focus, BotCommandRestriction.Both, CommandSecurity.None, "focuses on a specific stat for enhanced  increases to it. 1 at a time."),

                new Command(CommandStrings.Verbose, BotCommandRestriction.Both, CommandSecurity.None, "enables enhanced combat logging in whispers only."),

                new Command(CommandStrings.BaseCooldown, BotCommandRestriction.Both, CommandSecurity.None, "sets the dive stamina required."),
                new Command(CommandStrings.StardustCooldown, BotCommandRestriction.Both, CommandSecurity.None, "sets the gatcha stardust required."),
                new Command(CommandStrings.Reset, BotCommandRestriction.Both, CommandSecurity.None, "resets a specific user's specific cooldown timer."),
                new Command(CommandStrings.Gift, BotCommandRestriction.Both, CommandSecurity.None, "gives a gift of gold to another user."),

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
                        Respond(channel, $"{card.DisplayName} has been smote.", user);
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
                            Respond(null, $"Unable to resolve card for user: {tu}. Format: {CommandChar}{CommandStrings.Reset} *Target* *CooldownType*", user);
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
                            Respond(null, $"Succesfully reset {targetCard.Name}'s {cdTarget}!", user);

                        }
                        else
                        {
                            Respond(null, $"Unknown error attempting to reset cooldown for {targetCard.Name}.", user);
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
                            Respond(channel, $"Unable to parse status. Please try again.", user);
                            return true;
                        }

                        Api.SetStatus(sta.ToString(), message.Split(" ".ToCharArray(), 2).Last(), user);
                        Respond(channel, $"Setting your status to: [{sta.ToString()}] [{message.Split(" ".ToCharArray(), 2).Last()}]", user);
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
            Respond(channel, $"              Welcome to Cardinal's Cathedral, {user}!! " +
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
                Respond(null, $"Sorry, {nickname}, but you can only gift in public channels!", pc.Name);
                return;
            }

            try
            {
                goldAmount = Convert.ToInt32(message.Split(' ').Last());
                string targetName = message.Replace(goldAmount.ToString(), "").TrimEnd();
                RngGeneration.TryGetCard(targetName, out gc);
                if (gc == null)
                {
                    Respond(channel, $"Sorry, {nickname}, but {targetName} isn't a valid user! Check your spelling or casing.", pc.Name);
                    return;
                }
                else if (goldAmount <= 0)
                {
                    Respond(channel, $"Sorry, {nickname}, but you can only give positive amounts of gold to other people!", pc.Name);
                    return;
                }
                else if (goldAmount > pc.GetStat(StatTypes.Gld))
                {
                    Respond(channel, $"Sorry, {nickname}, but you can only give as much gold as you have ({pc.GetStat(StatTypes.Gld)}) to someone!", pc.Name);
                    return;
                }
                else if (pc.Name.Equals(gc.Name))
                {
                    Respond(channel, $"Sorry, {nickname}, but you can't gift yourself!", pc.Name);
                    return;
                }
            }
            catch
            {
                gc = new Cards.PlayerCard();
            }

            if (string.IsNullOrWhiteSpace(gc.Name))
            {
                Respond(channel, $"Sorry, {nickname}, but unable to find user: {message}. Expected format: {CommandChar}{CommandStrings.Gift} Rng 34    {CommandChar}{CommandStrings.Gift} Cardinal System 14", pc.Name);
            }
            else
            {
                gc.AddStat(StatTypes.Gld, goldAmount);
                pc.AddStat(StatTypes.Gld, -goldAmount);
                Data.DataDb.UpdateCard(gc);
                Data.DataDb.UpdateCard(pc);
                string nicknameTwo = (string.IsNullOrWhiteSpace(gc.DisplayName)) ? $"[color=white]{gc.Name}[/color]" : $"[color=white]{gc.DisplayName}[/color]";
                Respond(channel, $"[b]{nickname}[/b] has gifted [b]{nicknameTwo}[/b] [color=yellow][b]{goldAmount}[/b][/color] gold!", pc.Name);
            }
        }

        public void BaseCooldownAction(string channel, string message, Cards.PlayerCard pc)
        {
            try
            {
                int cd = Convert.ToInt32(message);
                REQUIRED_DIVE_STAMINA = cd;
                Respond(channel, $"[b]Base stamina required to dive has been set to: [color=green]{REQUIRED_DIVE_STAMINA}[/color]. Happy hunting![/b]", pc.Name);
            }
            catch
            {
                Respond(channel, $"Invalid format. Try again! ex: {CommandChar}{CommandStrings.BaseCooldown} 20 (for 20 stamina)", pc.Name);
            }
        }

        public void GatchaRequiredStardustAction(string channel, string message, Cards.PlayerCard pc)
        {
            try
            {
                int cd = Convert.ToInt32(message);
                COST_TO_ROLL = cd;
                Respond(channel, $"[b]Base Stardust required to roll in the Gatcha has been set to: [color=green]{REQUIRED_DIVE_STAMINA}[/color]. Good luck![/b]", pc.Name);
            }
            catch
            {
                Respond(channel, $"Invalid format. Try again! ex: {CommandChar}{CommandStrings.StardustCooldown} 20 (for 20 stamina)", pc.Name);
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
                        Respond(null, $"Combat verbosity changed to {(ucard.Verbose == true ? "[color=green]Enabled[/color]" : "[color=orange]Disabled[/color]")}. This only takes effect in whispers.", ucard.Name);
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
                            Respond(null, $"Unable to resolve card for user: {message}", user);
                            break;
                        }

                        if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard card))
                        {
                            Console.WriteLine($"Unknown error attempting to get card: {message}");
                            break;
                        }

                        Respond(null, $"{ucard.DisplayName} has challenged you to a duel. [sub][color=green]Accept by responding to me with: {CommandChar}{CommandStrings.AcceptDuelLong}[/color] | [color=red]Decline by responding to me with: {CommandChar}{CommandStrings.DenyDuelLong}[/color][/sub]", card.Name);
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
                            Respond(null, $"Unable to resolve valid focusable stat for value: {message}", user);
                            break;
                        }

                        StatTypes toFocus = (StatTypes)Enum.Parse(typeof(StatTypes), message, true);

                        if (!RngGeneration.GetAllFocusableStats().Contains(toFocus))
                        {
                            Respond(null, $"Unable to resolve valid focusable stat for value: {message}", user);
                            break;
                        }
                        ucard.GetStat(StatTypes.Foc);
                        ucard.SetStat(StatTypes.Foc, (int)toFocus);
                        Data.DataDb.UpdateCard(ucard);

                        Respond(null, $"Successfully set stat focus to: {message}", user);
                    }
                    break;
                case CommandStrings.Bully:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard))
                        {
                            break;
                        }

                        if (message.Equals(CommandStrings.Help, StringComparison.InvariantCultureIgnoreCase))
                        {
                            BullyHelpAction(user);
                            return true;
                        }

                        if (string.IsNullOrWhiteSpace(channel))
                        {
                            break;
                        }

                        if (!Data.DataDb.UserExists(message))
                        {
                            Respond(null, $"Unable to resolve card for user: {message}", user);
                            break;
                        }

                        if (!RngGeneration.TryGetCard(message, out Cards.PlayerCard targetCard))
                        {
                            Console.WriteLine($"Unknown error attempting to get card: {message}");
                            break;
                        }

                        if ((targetCard.LastTriggeredCds[LastUsedCooldownType.LastBullied] + targetCard.BaseCooldowns[PlayerActionTimeoutTypes.HasBeenBulliedCooldown]) > DateTime.Now)
                        {
                            Respond(channel, $"{targetCard.DisplayName} has been bullied too recently, {ucard.DisplayName}.", user);
                            break;
                        }

                        if (targetCard.Name == ucard.Name)
                        {
                            Respond(channel, $"If you want to bully yourself, {targetCard.DisplayName}, go find a sad anime to watch.", user);
                            break;
                        }

                        //if ((ucard.LastTriggeredCds[LastUsedCooldownType.LastBully] + ucard.BaseCooldowns[PlayerActionTimeoutTypes.BulliedSomeoneCooldown]) > DateTime.Now)
                        //{
                        //    Respond(null, $"You can only successfully bully once every {ucard.BaseCooldowns[PlayerActionTimeoutTypes.BulliedSomeoneCooldown].TotalMinutes} minutes, {ucard.DisplayName}.", user);
                        //    break;
                        //}

                        // find encounter
                        Encounter enc = null;
                        Cards.BaseCard enemyCard = null;
                        foreach (var v in encounterTracker.PendingEncounters)
                        {
                            if (v.Value.Bullied.Equals(ucard))
                            {
                                enc = v.Value;
                                enemyCard = v.Value.Bully;

                                // timeout stuff
                                DateTime timeoutTime = enc.CreationDate + enc.PrepTimeout;
                                DateTime rightNow = DateTime.Now;

                                if (timeoutTime < DateTime.Now)
                                {
                                    // kill the encounter
                                    encounterTracker.KillEncounter(enc);
                                    ucard.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
                                    (enemyCard as Cards.PlayerCard).LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
                                    Data.DataDb.UpdateCard((enemyCard as Cards.PlayerCard));
                                    Data.DataDb.UpdateCard(ucard);
                                    break;
                                }
                                else if (enc.EncounterStatus == EncounterStatus.Resolved)
                                {
                                    encounterTracker.KillEncounter(enc);
                                    break;
                                }
                            }
                        }

                        enc = new Encounter(targetCard.BaseCooldowns[PlayerActionTimeoutTypes.BullyAttemptCooldown]);

                        // add bully
                        enc.AddParticipant(1, ucard);
                        enc.Bully = ucard;
                        ucard.LastTriggeredCds[LastUsedCooldownType.LastBully] = enc.CreationDate;

                        // add the bullied target
                        enc.AddParticipant(2, targetCard);
                        enc.Bullied = targetCard;
                        targetCard.LastTriggeredCds[LastUsedCooldownType.LastBullied] = enc.CreationDate;

                        // add the encounter
                        encounterTracker.AddEncounter(enc);
                        Data.DataDb.UpdateCard(ucard);
                        Data.DataDb.UpdateCard(targetCard);
                        Respond(channel, $"{ucard.DisplayName} is attempting to bully you, {targetCard.DisplayName}! [sub][color=pink]You can submit by replying with: {CommandChar}{CommandStrings.Submit}[/color] | [color=red]You can fight back by replying with: {CommandChar}{CommandStrings.Fight}[/color][/sub]", string.Empty);
                    }
                    break;
                case CommandStrings.Submit:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard)) break;

                        if (string.IsNullOrWhiteSpace(channel)) break;

                        // find encounter
                        Encounter enc = null;
                        Cards.BaseCard enemyCard = null;
                        foreach (var v in encounterTracker.PendingEncounters)
                        {
                            if (v.Value.Bullied.Name.Equals(ucard.Name))
                            {
                                enc = v.Value;
                                enemyCard = v.Value.Bully;
                                break;
                            }
                        }

                        if (enc == null)
                        {
                            break;
                        }

                        // timeout stuff
                        DateTime timeoutTime = enc.CreationDate + enc.PrepTimeout;
                        DateTime rightNow = DateTime.Now;

                        if (timeoutTime < DateTime.Now)
                        {
                            // kill the encounter
                            encounterTracker.KillEncounter(enc);
                            ucard.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
                            (enemyCard as Cards.PlayerCard).LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
                            Data.DataDb.UpdateCard((enemyCard as Cards.PlayerCard));
                            Data.DataDb.UpdateCard(ucard);
                            Respond(channel, $"{ucard.DisplayName}, you had a pending bully attack, but it seems to of expired.", string.Empty);

                            break;
                        }
                        else if (enc.EncounterStatus == EncounterStatus.Resolved)
                        {
                            encounterTracker.KillEncounter(enc);
                            break;
                        }

                        // submit calc
                        string submitStr = string.Empty;
                        TimeSpan submitVal = new TimeSpan(0, 20, 0);

                        int staLost;
                        if (ucard.GetStat(StatTypes.Sta) > submitVal.TotalSeconds)
                        {
                            ucard.AddStat(StatTypes.Sta, -(submitVal.TotalSeconds));
                            staLost = Convert.ToInt32(submitVal.TotalSeconds);
                        }
                        else
                        {
                            staLost = ucard.GetStat(StatTypes.Sta);
                            ucard.SetStat(StatTypes.Sta, 0);
                        }

                        //double pants = 90.0 / ucard.GetStat(StatTypes.StM);
                        double whatever = XPMULT * staLost;

                        submitStr += $"{ucard.DisplayName}, you submit to {enemyCard.DisplayName}'s aggressive bullying, losing {whatever} stamina. ";

                        // bully calc
                        int staWon;
                        if (enemyCard.GetStat(StatTypes.Sta) + staLost >= enemyCard.GetStat(StatTypes.StM))
                        {
                            staWon = Convert.ToInt32(enemyCard.GetStat(StatTypes.StM) - enemyCard.GetStat(StatTypes.Sta));
                            enemyCard.SetStat(StatTypes.Sta, enemyCard.GetStat(StatTypes.StM));
                        }
                        else
                        {
                            staWon = staLost;
                            enemyCard.AddStat(StatTypes.Sta, staLost);
                        }

                        //pants = 90.0 / ucard.GetStat(StatTypes.StM);
                        whatever = XPMULT * staWon;

                        submitStr += $"{enemyCard.DisplayName}, you gain {whatever} stamina for your successful bullying.";

                        // finalize
                        enemyCard.AddStat(StatTypes.Bly, 1, false, false, false);
                        ucard.AddStat(StatTypes.Sbm, 1, false, false, false);
                        Data.DataDb.UpdateCard((enemyCard as Cards.PlayerCard));
                        Data.DataDb.UpdateCard(ucard);

                        // end the encounter
                        encounterTracker.KillEncounter(enc);

                        // respond
                        Respond(channel, submitStr, string.Empty);
                    }
                    break;
                case CommandStrings.Fight:
                    {
                        if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard ucard)) break;

                        if (string.IsNullOrWhiteSpace(channel)) break;

                        // find encounter
                        Encounter enc = null;
                        Cards.BaseCard enemyCard = null;
                        foreach (var v in encounterTracker.PendingEncounters)
                        {
                            if (v.Value.Bullied.Name.Equals(ucard.Name))
                            {
                                enc = v.Value;
                                enemyCard = v.Value.Bully;
                                break;
                            }
                        }

                        if (enc == null)
                        {
                            break;
                        }

                        // refresh encounter cards
                        var tec = (enemyCard as Cards.PlayerCard);
                        RngGeneration.TryGetCard(enemyCard.Name, out tec);
                        enc.Bully = tec;
                        enc.Bullied = ucard;

                        // timeout stuff
                        DateTime timeoutTime = enc.CreationDate + enc.PrepTimeout;
                        DateTime rightNow = DateTime.Now;

                        if (timeoutTime < DateTime.Now)
                        {
                            // kill the encounter
                            encounterTracker.KillEncounter(enc);
                            ucard.LastTriggeredCds[LastUsedCooldownType.LastBullied] = DateTime.MinValue;
                            tec.LastTriggeredCds[LastUsedCooldownType.LastBully] = DateTime.MinValue;
                            Data.DataDb.UpdateCard(tec);
                            Data.DataDb.UpdateCard(ucard);
                            break;
                        }
                        else if (enc.EncounterStatus == EncounterStatus.Resolved)
                        {
                            encounterTracker.KillEncounter(enc);
                            break;
                        }

                        // start fight here
                        Respond(channel, $"A fight is breaking out between {(string.IsNullOrEmpty(ucard.DisplayName) ? ucard.Name : ucard.DisplayName)} and {tec.DisplayName}! Check it out here ⇒", string.Empty);
                        enc.StartASyncEncounter(Api, Api.GetChannelByNameOrCode(channel));
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
                if (v == StatTypes.Vit) toSend += $"\\n{v.ToString()} ⇒ Sustainability. 'Death' when this reaches 0.";
                if (v == StatTypes.Atk) toSend += $"\\n{v.ToString()} ⇒ Chance to hit with attacks.";
                if (v == StatTypes.Dmg) toSend += $"\\n{v.ToString()} ⇒ Base damage.";
                if (v == StatTypes.Dex) toSend += $"\\n{v.ToString()} ⇒ Physical damage multiplier.";
                if (v == StatTypes.Int) toSend += $"\\n{v.ToString()} ⇒ Magical damage multiplier.";
                if (v == StatTypes.Con) toSend += $"\\n{v.ToString()} ⇒ Temporary health. Reduced before [vit] damage.";
                if (v == StatTypes.Crc) toSend += $"\\n{v.ToString()} ⇒ Critical strike chance modifier.";
                if (v == StatTypes.Crt) toSend += $"\\n{v.ToString()} ⇒ Critical strike damage additional multiplier.";
                if (v == StatTypes.Ats) toSend += $"\\n{v.ToString()} ⇒ Reduces the high/low spread of various combat actions.";
                if (v == StatTypes.Spd) toSend += $"\\n{v.ToString()} ⇒ Determines order of attack during combat.";
                if (v == StatTypes.Pdf) toSend += $"\\n{v.ToString()} ⇒ Physical damage reduction modifier.";
                if (v == StatTypes.Mdf) toSend += $"\\n{v.ToString()} ⇒ Magical damage reduction modifier.";
                if (v == StatTypes.Eva) toSend += $"\\n{v.ToString()} ⇒ Chance to evade incoming attacks.";
            }

            toSend += $"\\n" +
                $"\\n[b]Damage Types:[/b] ";

            foreach (var v in Enum.GetValues(typeof(DamageTypes)))
            {
                if ((DamageTypes)v == DamageTypes.None)
                    continue;

                toSend += $"[color={((DamageTypes)v).GetDescription()}]{((DamageTypes)v).ToString()}[/color] ";
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

            Respond(null, toSend, sendingUser);
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

            Respond(null, toSend, sendingUser);
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

            Respond(null, toSend, sendingUser);
        }

        public void SetDiveFloorAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res) || FloorDb.GetAllFloors().Count < res || res <= 0)
            {
                Respond(channel, $"{user}, you must specify a valid floor", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard pc))
                return;

            pc.SetStat(StatTypes.Dff, res);
            Data.DataDb.UpdateCard(pc);
            Respond(channel, $"{pc.DisplayName}, you'll now dive to floor {res} by default.", user);
        }

        public void EquipAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res))
            {
                Respond(channel, $"{user}, you must specify a valid box slot you're attempting to equip. Ex: {CommandStrings.Equip} 1", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                return;

            if (card.Inventory.Count < res || res < 1)
            {
                Respond(channel, $"{user}, you must specify a valid box slot you're attempting to equip. Ex: {CommandStrings.Equip} 1", user);
                return;
            }

            Socket toAdd = card.Inventory[res - 1];
            if (!card.AvailableSockets.Contains(toAdd.SocketType))
            {
                Respond(channel, $"{user}, you must specify an equipment type you're allowed to equip. Ex: {CommandStrings.Equip} 1", user);
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
            Respond(channel, replyMessage, user);
        }

        public void UnequipAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res))
            {
                Respond(channel, $"{user}, you must specify a valid equipment slot you're attempting to un-equip. Ex: {CommandStrings.Unequip} 1", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                return;

            if (res < 1)
            {
                Respond(channel, $"{user}, you must specify a valid equipment slot you're attempting to un-equip. Ex: {CommandStrings.Unequip} 1", user);
                return;
            }

            if (card.Inventory.Count >= card.MaxInventory)
            {
                Respond(channel, $"{user}, you must have free inventory space to {CommandStrings.Unequip} gear.", user);
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
            Respond(channel, $"{card.DisplayName}, you've unequipped your {toUnequip.GetRarityString()} {toUnequip.GetName()}", user);
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
                Respond(channel, "Invalid character name.", user);
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

            Respond(channel, cardStr, user);
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

            Respond(null, toSend, sendingUser);
        }

        /// <summary>
        /// handles parsing non-command interactions
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="user">source user</param>
        /// <param name="message">message the user sent</param>
        //public void HandleNonCommand(string channel, string user, string message)
        //{
        //
        //}

        /// <summary>
        /// replies via the f-list api
        /// </summary>
        /// <param name="channel">channel to reply to</param>
        /// <param name="message">message to reply with</param>
        /// <param name="recipient">person to reply to</param>
        public override void Respond(string channel, string message, string recipient)
        {
            MessageType mt = MessageType.Basic;
            if (string.IsNullOrWhiteSpace(channel))
            {
                mt = MessageType.Whisper;
            }

            Respond(channel, message, recipient, mt);
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
            // start timers

        }
    }
}
