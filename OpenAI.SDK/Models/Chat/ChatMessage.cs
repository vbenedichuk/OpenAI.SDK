using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    /// <summary>
    /// Chat message format.
    /// See <see href="https://platform.openai.com/docs/guides/chat/introduction">the documentation</see> for more details
    /// </summary>
    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public object Content { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<ChatToolCall> ToolCalls { get; set; }

        [JsonPropertyName("tool_call_id")]
        public string ToolCallId { get; set; }

        [JsonPropertyName("function_call")]
        public ChatFunctionCall FunctionCall { get; set; }
    }
}