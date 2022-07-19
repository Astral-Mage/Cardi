using ChatApi;
using ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem;
using ChatBot.Bot.Plugins.LostRPG.RoleplaySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChatBot.Bot.Plugins.LostRPG
{
    public partial class LostRPG : PluginBase
    {
        /// <summary>
        /// just some random base vars
        /// </summary>
        public string BASE_COLOR = "white";

        /// <summary>
        /// handles all non-commands
        /// </summary>
        /// <param name="channel">the source channel</param>
        /// <param name="user">the sending user</param>
        /// <param name="message">the recieved message</param>
        public void HandleNonCommand(string channel, string user, string message)
        {
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

            Assembly.GetExecutingAssembly().GetTypes().Where(x => x.BaseType == typeof(BaseAction) && x.Name.Equals(command + "Action", StringComparison.InvariantCultureIgnoreCase)).ToList().ForEach((curAction) =>
             {
                 BaseAction inst = (BaseAction)Activator.CreateInstance(curAction);
                 inst.Execute(new ActionObject() { Channel = channel, CommandChar = CommandChar, User = sendingUser });
                 return;
             });
        }

        /// <summary>
        /// our base constructor
        /// </summary>
        /// <param name="api">f-list api interface</param>
        public LostRPG(ApiConnection api, string commandChar) : base(api, commandChar)
        {

        }

        /// <summary>
        /// our currently active channels
        /// </summary>
        readonly List<string> ActiveChannels = new List<string>();

        /// <summary>
        /// called whenever we've joined a channel
        /// </summary>
        /// <param name="channel">the channel we've joined</param>
        public override void HandleJoinedChannel(string channel)
        {
            ActiveChannels.Add(channel);
        }
    }
}
