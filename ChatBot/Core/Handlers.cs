using ChatApi;
using System.Linq;

namespace ChatBot
{
    public partial class Program
    {
        /// <summary>
        /// Takes care of pushing messages to the bot
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandleMessageReceived(object sender, MessageEventArgs e)
        {
            bool isOp = Ops.Any(x => x.Equals(e.user));
            string command = string.Empty;
            if (e.message.StartsWith(CommandChar))
            {
                e.message = e.message.TrimStart(CommandChar.ToCharArray());
                if (e.message.Split(' ').Length > 1)
                {
                    command = e.message.Split(' ').First();
                }
                else
                {
                    command = e.message;
                }
            }

            Bot.HandleMessage(e.channel, Utility.ReplaceFirst(e.message, command, "").TrimStart(), e.user, command.ToLowerInvariant(), isOp);
        }

        /// <summary>
        /// We've joined a channel
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandleJoinedChannel(object sender, ChannelEventArgs e)
        {

        }

        /// <summary>
        /// We've left a channel
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandleLeftChannel(object sender, ChannelEventArgs e)
        {

        }

        /// <summary>
        /// We got a private list of channels
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandlePrivateChannelsReceived(object sender, ChannelEventArgs e)
        {
            // check and join starting channel here
        }

        /// <summary>
        /// We got a public list of channels
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandlePublicChannelsReceived(object sender, ChannelEventArgs e)
        {
            // check and join starting channel here

        }
    }
}