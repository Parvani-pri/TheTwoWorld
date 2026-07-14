using System;

namespace TwoWorlds.AI
{
    [Serializable]
    public class ChatCompletionResponse
    {
        public ChatChoice[] choices;
        public ChatErrorBody error;
    }

    [Serializable]
    public class ChatChoice
    {
        public ChatMessage message;
    }

    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class ChatErrorBody
    {
        public string message;
        public string type;
    }
}
