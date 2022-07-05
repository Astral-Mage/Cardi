using ChatApi;
using System;

namespace ChatBot.Core
{
    public class SystemController
    {
        private static SystemController instance;

        private string BaseColor;

        private SystemController()
        {
            BaseColor = "white";
        }

        private static readonly object iLocker = new object();

        ApiConnection Api;

        public static SystemController Instance
        {
            get
            {
                lock (iLocker)
                {
                    if (instance == null)
                    {
                        instance = new SystemController();
                    }
                    return instance;
                }
            }
        }

        public void SetBaseColor(string color)
        {
            BaseColor = color;
        }

        public void SetApi(ApiConnection api)
        {
            Api = api;
        }

        /// <summary>
        /// replies via the f-list api
        /// </summary>
        /// <param name="channel">channel to reply to</param>
        /// <param name="message">message to reply with</param>
        /// <param name="recipient">person to reply to</param>
        public void Respond(string channel, string message, string recipient)
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
        /// <param name="messagetype">type of message we're sending</param>
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

            message = $"[color={BaseColor}]{message}[/color]";

            Api.SendMessage(channel, message, recipient, messagetype);
        }
    }
}
