using System.Text;

namespace TwoWorlds.AI
{
    static class AIJsonBuilder
    {
        public static string BuildChatCompletionRequest(
            string model,
            string systemPrompt,
            string userMessage,
            int maxTokens,
            float temperature)
        {
            var builder = new StringBuilder(256);
            builder.Append('{');
            builder.Append("\"model\":\"").Append(EscapeJson(model)).Append('"');
            builder.Append(",\"messages\":[");
            builder.Append("{\"role\":\"system\",\"content\":\"").Append(EscapeJson(systemPrompt)).Append("\"}");
            builder.Append(",{\"role\":\"user\",\"content\":\"").Append(EscapeJson(userMessage)).Append("\"}");
            builder.Append(']');
            builder.Append(",\"max_tokens\":").Append(maxTokens);
            builder.Append(",\"temperature\":").Append(temperature.ToString("0.###"));
            builder.Append('}');
            return builder.ToString();
        }

        static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length + 8);
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        builder.Append(ch);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
