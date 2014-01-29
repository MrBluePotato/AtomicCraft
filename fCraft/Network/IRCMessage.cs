namespace fCraft
{
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    public sealed class IRCMessage
    {
        public IRCMessage(string from, string nick, string ident, string host, string channel, string message,
            string rawMessage, IRCMessageType type, IRCReplyCode replycode)
        {
            RawMessage = rawMessage;
            RawMessageArray = rawMessage.Split(new[] {' '});
            Type = type;
            ReplyCode = replycode;
            From = from;
            Nick = nick;
            Ident = ident;
            Host = host;
            Channel = channel;
            if (message != null)
            {
                // message is optional
                Message = message;
                MessageArray = message.Split(new[] {' '});
            }
        }

        public string From { get; private set; }
        public string Nick { get; private set; }
        public string Ident { get; private set; }
        public string Host { get; private set; }
        public string Channel { get; private set; }
        public string Message { get; private set; }
        public string[] MessageArray { get; private set; }
        public string RawMessage { get; private set; }
        public string[] RawMessageArray { get; private set; }
        public IRCMessageType Type { get; private set; }
        public IRCReplyCode ReplyCode { get; private set; }
    }
}