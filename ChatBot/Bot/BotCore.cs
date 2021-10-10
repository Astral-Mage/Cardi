using ChatApi;
using ChatBot.Bot.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot
{
    /// <summary>
    /// This is our main bot interface
    /// </summary>
    public class BotCore
    {
        /// <summary>
        /// Our list of active plugins
        /// </summary>
        private readonly List<PluginBase> plugins;

        /// <summary>
        /// Constructor, inits plugins
        /// </summary>
        public BotCore()
        {
            plugins = new List<PluginBase>();
        }

        /// <summary>
        /// Adds a plugin to the list of active plugins
        /// </summary>
        /// <param name="plugin"></param>
        public void AddPlugin(PluginBase plugin)
        {
            if (plugins.Where(x => x.GetType().Equals(plugin)).Count() > 0)
            {
                Console.WriteLine("Unable to add same plugin twice at this time. Sorry!");
                return;
            }

            plugins.Add(plugin);
        }

        /// <summary>
        /// Returns plugin of a specific type if available
        /// </summary>
        /// <param name="type">type to return</param>
        /// <returns>plugin if found, otherwise null</returns>
        public PluginBase GetPlugin(Type type)
        {
            PluginBase toReturn = null;

            try
            {
                return plugins.First((x) => x.GetType().Equals(type));
            }
            catch
            {
                return toReturn;
            }
        } 

        /// <summary>
        /// Clean up our plugins if we're closing things down
        /// </summary>
        public void Shutdown()
        {
            foreach (var plugin in plugins)
            {
                plugin.Shutdown();
            }
        }

        /// <summary>
        /// Handles when we receive a message from the chat server
        /// </summary>
        /// <param name="channel">originating channel</param>
        /// <param name="message">cleaned up message</param>
        /// <param name="sendingUser">user that send the message</param>
        /// <param name="command">command being sent, if any</param>
        /// <param name="isOp">if the sending user is an op</param>
        public void HandleMessage(string channel, string message, string sendingUser, string command, bool isOp)
        {
            foreach (PluginBase plugin in plugins)
            {
                plugin.HandleRecievedMessage(command, channel, message, sendingUser, isOp);
            }
        }
    }
}
