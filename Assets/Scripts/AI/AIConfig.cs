using UnityEngine;

namespace TwoWorlds.AI
{
    public enum AIProvider
    {
        DeepSeek,
        Custom
    }

    [CreateAssetMenu(fileName = "AIConfig", menuName = "Two Worlds/AI Config")]
    public class AIConfig : ScriptableObject
    {
        [SerializeField] AIProvider provider = AIProvider.DeepSeek;
        [SerializeField] string apiBaseUrl = "https://api.deepseek.com";
        [SerializeField] string chatEndpoint = "/chat/completions";
        [SerializeField] string modelName = "deepseek-chat";
        [SerializeField] string apiKey;
        [SerializeField] int maxTokens = 150;
        [SerializeField] float temperature = 0.7f;
        [SerializeField] float requestTimeoutSeconds = 15f;
        [SerializeField] bool enableDebugLog = true;
        [SerializeField] bool loadApiKeyFromStreamingAssets = true;
        [SerializeField] string secretsFileName = "ai_secrets.json";

        public AIProvider Provider => provider;
        public string ModelName => modelName;
        public int MaxTokens => maxTokens;
        public float Temperature => temperature;
        public float RequestTimeoutSeconds => requestTimeoutSeconds;
        public bool EnableDebugLog => enableDebugLog;

        public string GetChatCompletionsUrl()
        {
            var baseUrl = apiBaseUrl?.TrimEnd('/') ?? string.Empty;
            var endpoint = string.IsNullOrWhiteSpace(chatEndpoint) ? "/chat/completions" : chatEndpoint;
            if (!endpoint.StartsWith("/"))
                endpoint = "/" + endpoint;

            return baseUrl + endpoint;
        }

        public bool TryGetApiKey(out string resolvedKey)
        {
            resolvedKey = apiKey;

            if (!string.IsNullOrWhiteSpace(resolvedKey))
                return true;

            if (!loadApiKeyFromStreamingAssets)
                return false;

            resolvedKey = AISecretsLoader.TryLoadApiKey(secretsFileName);
            return !string.IsNullOrWhiteSpace(resolvedKey);
        }

        public bool IsConfigured() =>
            TryGetApiKey(out _) && !string.IsNullOrWhiteSpace(GetChatCompletionsUrl());
    }
}
