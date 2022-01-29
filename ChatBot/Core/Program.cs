﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChatApi;


using ChatBot.Bot.Plugins;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Data;
using ChatBot.Bot.Plugins.GatchaGame.Quests;


#if DEBUG
using ChatBot.Bot.Plugins.GatchaGame;
#else
using ChatBot.Bot.Plugins.GatchaGame;
#endif

namespace ChatBot
{
    public partial class Program
    {
        // -----------------------------------------------
        //             User Information
        // -----------------------------------------------
        static readonly string UserNameArg          = "UserName";
        static readonly string PassWordArg          = "Password";
        static readonly string CharacterNameArg     = "CharacterName";
        static readonly string StartingChannelArg   = "StartingChannel";
        static readonly string RetryAttemptsArg     = "RetryAttempts";
        static readonly string CommandCharArg       = "CommandChar";
        static readonly string OpsListArg           = "OpsList";
        // -----------------------------------------------

        static string StartingChannel = string.Empty;

        /// <summary>
        /// Our chat interface
        /// </summary>
        static ApiConnection Chat;

        /// <summary>
        /// Our bot interface
        /// </summary>
        static BotCore Bot;

        /// <summary>
        /// Our main command character
        /// </summary>
        static string CommandChar = string.Empty;

        /// <summary>
        /// A list of bot ops
        /// </summary>
        static List<string> Ops = new List<string>();

        /// <summary>
        /// A collection of any failed cli args in validation check
        /// </summary>
        static readonly List<string> FailedCliArgs = new List<string>();

        /// <summary>
        /// The bot's character name
        /// </summary>
        public static string CharacterName = string.Empty;

        /// <summary>
        /// Our main entry point
        /// </summary>
        static void Main(string[] args)
        {
            // import data from other game
            //TransferUserData();
            //TransferFloorData();

            //foreach (var v in q)
            //{
                DataDb.AddNewQuest(new Quest()
                {
                    QuestName = "The Shameful Hunt",
                    QuestId = 7001,
                    TriggerFloors = new int[] { },
                    LevelRequirement = 0,
                    PrerequisiteQuest = null,
                    DepthRequirement = 0,
                    Repeatable = true,
                    QuestText = "You find a hidden passage, leveraging your keen senses. It almost went unnoticed, but you manage to trudge through the small passage for some time. After that you encounter a dangerous looking room with a chest on the other end. It gleamed, expensive and pristine in the distance. Hours of careful treading and many close calls later, you read the end only to discover it... empty! You've been fooled, but gain [color=green]{int} Intelligence[/color] as your shame washes over you.",
                    TriggerChance = 1.5,
                    BlockedBy = null,
                    Rewards = new QuestReward()
                    {
                        Stats = new Bot.Plugins.GatchaGame.Cards.Stats.BaseStats()
                        {
                            Stats = new Dictionary<Bot.Plugins.GatchaGame.Enums.StatTypes, double>() {
                    { ChatBot.Bot.Plugins.GatchaGame.Enums.StatTypes.Int, 1 },
                }
                        },
                        OtherReward = UniqueRewards.None
                    },
                });

            // cli arg parsing and validation
            if (args.Length == 0)
            {
                Console.WriteLine($"Error: expecting launch arguments.");
                Environment.Exit(-1);
            }

            string cliArgumentsStr = string.Join(" ", args);
            List<string> cliArgs = cliArgumentsStr.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            cliArgs.ForEach(x => x = x.Replace(" ", ""));
            Dictionary<string, string> cliArgDict = new Dictionary<string, string>();
            foreach (var singleArg in cliArgs)
            {
                var splitThing = singleArg.Split('=');
                cliArgDict.Add(splitThing.First().ToLowerInvariant(), splitThing.Last().Trim());
            }

            ValidateArgument(out string Username,               UserNameArg, cliArgDict);
            ValidateArgument(out string Password,               PassWordArg, cliArgDict);
            ValidateArgument(out string CharacterName,          CharacterNameArg, cliArgDict);
            ValidateArgument(out string StartingChannel,        StartingChannelArg, cliArgDict, true);
            ValidateArgument(out CommandChar,                   CommandCharArg, cliArgDict);
            ValidateArgument(out Ops,                           OpsListArg, cliArgDict);

            ValidateArgument(out int RetryAttempts,             RetryAttemptsArg, cliArgDict);

            if (!ConfirmCliArgumentValidation())
            {
                Environment.Exit(-1);
            }

            while (RetryAttempts > 0)
            {
                try
                {
                    RunChat(Username, Password, CharacterName, StartingChannel, CommandChar, Ops);
                }
                catch(Exception e)
                {
                    RetryAttempts--;
                    Console.WriteLine($"Error. Attempting to reconnect to chat. Attempt {4 - RetryAttempts} out of 3 : {e}");
                }
            }

            Environment.Exit(0);
        }

        #region Validation
        static bool ConfirmCliArgumentValidation()
        {
            if (FailedCliArgs.Any())
            {
                foreach(string failedArgReply in FailedCliArgs)
                {
                    Console.WriteLine($"{failedArgReply}");
                }
                return false;
            }

            return true;
        }

        static void ValidateArgument(out List<string> argVal, string argName, Dictionary<string, string> rawArgs, bool isOptional = false)
        {
            argVal = new List<string>();
            argName = argName.ToLowerInvariant();

            if (!rawArgs.ContainsKey(argName))
            {
                if (!isOptional)
                    FailedCliArgs.Add($"Failed Validation: {argName}");
                return;
            }

            if (string.IsNullOrWhiteSpace(rawArgs[argName]))
            {
                FailedCliArgs.Add($"Failed Validation: {argName}");
                return;
            }

            argVal = rawArgs[argName].Split(',').ToList();
            Console.WriteLine($"{argName} --- {string.Join(",", argVal)}");
        }

        static void ValidateArgument(out string argVal, string argName, Dictionary<string, string> rawArgs, bool isOptional = false)
        {
            argVal = string.Empty;
            argName = argName.ToLowerInvariant();

            if (!rawArgs.ContainsKey(argName))
            {
                if (!isOptional)
                    FailedCliArgs.Add($"Failed Validation: {argName}");
                return;
            }

            if (string.IsNullOrWhiteSpace(rawArgs[argName]))
            {
                FailedCliArgs.Add($"Failed Validation: {argName}");
                return;
            }

            argVal = rawArgs[argName];
            Console.WriteLine($"{argName} --- {((argName.Equals(PassWordArg, StringComparison.InvariantCultureIgnoreCase)) ? "************" : $"{argVal}")}");
        }

        static void ValidateArgument(out int argVal, string argName, Dictionary<string, string> rawArgs, bool isOptional = false)
        {
            argVal = -1;
            argName = argName.ToLowerInvariant();

            if (!rawArgs.ContainsKey(argName))
            {
                if (!isOptional)
                    FailedCliArgs.Add($"Failed Validation: {argName}");
                return;
            }

            if (string.IsNullOrWhiteSpace(rawArgs[argName]))
            {
                FailedCliArgs.Add($"Failed Validation: {argName}");
                return;
            }

            if (!int.TryParse(rawArgs[argName], out argVal))
            {
                FailedCliArgs.Add($"Failed Validation: {argName}");
                return;
            }

            Console.WriteLine($"{argName} --- {argVal}");
        }

        static void ValidateFlag(out bool flagVal, string flagName, Dictionary<string, string> rawArgs, bool isOptional = false)
        {
            flagVal = false;
            flagName = flagName.ToLowerInvariant();

            if (!rawArgs.ContainsKey(flagName))
            {
                if (!isOptional)
                    FailedCliArgs.Add($"Failed Validation: {flagName}");
                return;
            }

            flagVal = true;
            Console.WriteLine($"{flagName} --- {flagVal}");
        }
#endregion

        /// <summary>
        /// Connection loop for easier retries
        /// </summary>
        /// <returns>false if we lose our connection</returns>
        static bool RunChat(string userName, string passWord, string characterName, string startingChannel, string commandChar, List<string> opsList)
        {
            if (Chat != null) Chat = null;
            if (Bot != null)
            {
                Bot.Shutdown();
                Bot = null;
                Thread.Sleep(10000);
            }

            try
            {
                Chat = new ApiConnection();
                Bot = new BotCore();

                // Add our plugins here ////////////////////////////////////////////////
#if DEBUG
                Bot.AddPlugin(new GatchaGame(Chat, commandChar));
#else
                Bot.AddPlugin(new GatchaGame(Chat, commandChar));
#endif
                // End Plugin Adding ///////////////////////////////////////////////////

                CharacterName = characterName;

                Chat.ConnectedToChat                = ConnectedToChat;
                Chat.MessageHandler                 = HandleMessageReceived;
                Chat.JoinedChannelHandler           = HandleJoinedChannel;
                Chat.CreatedChannelHandler          = HandleCreatedChannel;
                Chat.LeftChannelHandler             = HandleLeftChannel;
                Chat.PrivateChannelsReceivedHandler = HandlePrivateChannelsReceived;
                Chat.PublicChannelsReceivedHandler  = HandlePublicChannelsReceived;

                Chat.ConnectToChat(userName, passWord, characterName);
                StartingChannel = startingChannel;

                // initiate the loop
                while (Chat.IsConnected())
                {
                    Update();
                }

                return false;
            }
            catch (Exception e)
            {
                throw new Exception($"Error connecting to chat: {e}");
            }
        }

        /// <summary>
        /// update loop, do w/e in here
        /// </summary>
        static void Update()
        {
            Thread.Sleep(10000);

        }

        /// <summary>
        /// 
        /// </summary>
        static void TransferFloorData()
        {
            var ofloors = FloorDb.GetAllFloors();
            foreach (var floor in ofloors)
            {
                DataDb.AddNewFloor(floor);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void TransferUserData()
        {

            var ocards = GameDb.GetAllCards();
            foreach (var card in ocards)
            {
                Bot.Plugins.GatchaGame.Cards.PlayerCard pc = new Bot.Plugins.GatchaGame.Cards.PlayerCard();
                RngGeneration.GenerateNewCharacterStats(pc);
                pc.AddStat(StatTypes.Exp, Convert.ToInt32(card.level * 10), false, false, false);
                pc.AddStat(StatTypes.Gld, card.level * 5, false, false, false);

                pc.Name = card.name;
                pc.DisplayName = card.nickname;
                if (string.IsNullOrWhiteSpace(pc.DisplayName)) pc.DisplayName = pc.Name;
                pc.Signature = card.signature;
                pc.SpeciesDisplayName = card.species;
                pc.ClassDisplayName = card.mainClass;
                pc.AvailableSockets.Add(SocketTypes.Weapon);
                pc.AvailableSockets.Add(SocketTypes.Armor);
                pc.AvailableSockets.Add(SocketTypes.Passive);

                pc.AddStat(StatTypes.Sds, card.level * 2, false, false, false);
                pc.AddStat(StatTypes.Kil, card.killed, false, false, false);

                var tws = RngGeneration.GenerateRandomEquipment(out _, EquipmentTypes.Weapon, 1);
                tws.NameOverride = (string.IsNullOrWhiteSpace(card.weapon)) ? tws.NameOverride : card.weapon;
                pc.ActiveSockets.Add(tws);

                tws = RngGeneration.GenerateRandomEquipment(out _, EquipmentTypes.Armor, 1);
                tws.NameOverride = (string.IsNullOrWhiteSpace(card.gear)) ? tws.NameOverride : card.gear;
                pc.ActiveSockets.Add(tws);

                tws = RngGeneration.GenerateRandomPassive(out _, 1);
                tws.NameOverride = (string.IsNullOrWhiteSpace(card.special)) ? tws.NameOverride : card.special;
                pc.ActiveSockets.Add(tws);

                if (card.weaponperklvl == 1)
                {
                    pc.BoonsEarned.Add(BoonTypes.Sharpness);
                    pc.CompletedQuests.Add(2010);
                }
                if (card.gearperklvl == 1)
                {
                    pc.BoonsEarned.Add(BoonTypes.Resiliance);
                    pc.CompletedQuests.Add(2011);
                }
                if (card.specialperklvl == 1)
                {
                    pc.BoonsEarned.Add(BoonTypes.Empowerment);
                    pc.CompletedQuests.Add(3302);
                }


                var gcards = new List<Bot.Plugins.GatchaGame.Cards.PlayerCard>
                {
                    pc
                };

                // check for level up here
                int val1;
                int curlvl;
                int val2;
                do
                {
                    // -150 + 300x^1.8
                    val1 = pc.GetStat(StatTypes.Exp);
                    curlvl = pc.GetStat(StatTypes.Lvl);
                    val2 = Convert.ToInt32((-150 + (300 * Math.Pow(curlvl, 1.8))));
                    if (val1 > val2)
                    {
                        // leveled up
                        pc.LevelUp();
                
                    }
                } while (val1 > val2);

                DataDb.AddNewUser(pc);
            }
        }
    }
}