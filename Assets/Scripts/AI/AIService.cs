using System;
using System.Collections;
using System.Text;
using TwoWorlds.Inventory;
using UnityEngine;
using UnityEngine.Networking;

namespace TwoWorlds.AI
{
    public class AIService : MonoBehaviour
    {
        public static AIService Instance { get; private set; }

        [SerializeField] AIConfig config;
        [SerializeField] AIContextBuilder contextBuilder;

        Coroutine activeRequest;
        bool isBusy;

        public bool IsBusy => isBusy;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (contextBuilder == null)
                contextBuilder = GetComponent<AIContextBuilder>();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Ask(
            string systemPrompt,
            string userMessage,
            Action<string> onSuccess,
            Action<string> onError)
        {
            if (isBusy)
            {
                onError?.Invoke("AI 正在处理上一个请求，请稍后再试。");
                return;
            }

            if (config == null)
            {
                onError?.Invoke("AIConfig 未配置。");
                return;
            }

            if (!config.IsConfigured())
            {
                onError?.Invoke("AI 未配置 API Key。请在 AIConfig 或 StreamingAssets/ai_secrets.json 中填写。");
                return;
            }

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                onError?.Invoke("问题不能为空。");
                return;
            }

            if (activeRequest != null)
                StopCoroutine(activeRequest);

            activeRequest = StartCoroutine(SendRequestCoroutine(
                systemPrompt ?? string.Empty,
                userMessage,
                onSuccess,
                onError));
        }

        public void AskWithInventoryContext(
            PlayerInventory inventory,
            string npcPersona,
            string userMessage,
            Action<string> onSuccess,
            Action<string> onError)
        {
            if (contextBuilder == null)
            {
                onError?.Invoke("AIContextBuilder 未配置。");
                return;
            }

            var systemPrompt = contextBuilder.BuildNpcSystemPrompt(null, npcPersona, inventory);
            Ask(systemPrompt, userMessage, onSuccess, onError);
        }

        public void AskItemInterpretation(
            ItemData item,
            Action<string> onSuccess,
            Action<string> onError)
        {
            if (contextBuilder == null)
            {
                onError?.Invoke("AIContextBuilder 未配置。");
                return;
            }

            var systemPrompt = contextBuilder.BuildItemInterpretPrompt(item);
            Ask(systemPrompt, "请解读此物。", onSuccess, onError);
        }

        IEnumerator SendRequestCoroutine(
            string systemPrompt,
            string userMessage,
            Action<string> onSuccess,
            Action<string> onError)
        {
            isBusy = true;

            if (!config.TryGetApiKey(out var apiKey))
            {
                isBusy = false;
                onError?.Invoke("无法读取 API Key。");
                yield break;
            }

            var requestJson = AIJsonBuilder.BuildChatCompletionRequest(
                config.ModelName,
                systemPrompt,
                userMessage,
                config.MaxTokens,
                config.Temperature);

            if (config.EnableDebugLog)
                Debug.Log($"[AIService] Request: {requestJson}");

            var url = config.GetChatCompletionsUrl();
            if (config.EnableDebugLog)
                Debug.Log($"[AIService] Sending POST to {url} (timeout {config.RequestTimeoutSeconds}s)...");

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            var bodyRaw = Encoding.UTF8.GetBytes(requestJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.timeout = Mathf.CeilToInt(config.RequestTimeoutSeconds);

            yield return request.SendWebRequest();

            isBusy = false;
            activeRequest = null;

            if (config.EnableDebugLog)
                Debug.Log($"[AIService] Finished. HTTP {request.responseCode}, result={request.result}, error={request.error}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                var errorMessage = BuildHttpErrorMessage(request);
                Debug.LogError("[AIService] " + errorMessage);
                onError?.Invoke(errorMessage);
                yield break;
            }

            var responseText = request.downloadHandler.text;
            if (config.EnableDebugLog)
                Debug.Log($"[AIService] Response: {responseText}");

            if (!TryExtractAssistantContent(responseText, out var content, out var parseError))
            {
                Debug.LogError("[AIService] " + parseError);
                onError?.Invoke(parseError);
                yield break;
            }

            onSuccess?.Invoke(content);

            if (config.EnableDebugLog)
                Debug.Log($"[AIService] Success: {content}");
        }

        static string BuildHttpErrorMessage(UnityWebRequest request)
        {
            var responseText = request.downloadHandler?.text;
            if (!string.IsNullOrWhiteSpace(responseText) &&
                TryExtractApiErrorMessage(responseText, out var apiError))
            {
                return $"API 错误 ({request.responseCode}): {apiError}";
            }

            return $"请求失败 ({request.responseCode}): {request.error}";
        }

        static bool TryExtractApiErrorMessage(string responseText, out string message)
        {
            message = null;
            try
            {
                var response = JsonUtility.FromJson<ChatCompletionResponse>(responseText);
                if (response?.error != null && !string.IsNullOrWhiteSpace(response.error.message))
                {
                    message = response.error.message;
                    return true;
                }
            }
            catch (Exception)
            {
                // Ignore parse failures here; caller will use HTTP error text.
            }

            return false;
        }

        static bool TryExtractAssistantContent(string responseText, out string content, out string error)
        {
            content = null;
            error = null;

            if (string.IsNullOrWhiteSpace(responseText))
            {
                error = "API 返回空响应。";
                return false;
            }

            ChatCompletionResponse response;
            try
            {
                response = JsonUtility.FromJson<ChatCompletionResponse>(responseText);
            }
            catch (Exception ex)
            {
                error = "无法解析 API 响应：" + ex.Message;
                return false;
            }

            if (response?.error != null && !string.IsNullOrWhiteSpace(response.error.message))
            {
                error = response.error.message;
                return false;
            }

            if (response?.choices == null || response.choices.Length == 0)
            {
                error = "API 响应中没有 choices。";
                return false;
            }

            content = response.choices[0]?.message?.content;
            if (string.IsNullOrWhiteSpace(content))
            {
                error = "API 响应中没有有效文本内容。";
                return false;
            }

            return true;
        }
    }
}
