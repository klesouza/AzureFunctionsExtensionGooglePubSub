﻿using System;

namespace AzureFunctions.Extensions.GooglePubSub.PubSub
{
    public class SubscriberPullResponse
    {
        public Receivedmessage[] receivedMessages { get; set; }
    }

    public class Receivedmessage
    {
        public string ackId { get; set; }
        public Message message { get; set; }
    }

    public class Message
    {
        public string data { get; set; }
        public string messageId { get; set; }
        public DateTime publishTime { get; set; }

        public string dataString => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data));

    }

}