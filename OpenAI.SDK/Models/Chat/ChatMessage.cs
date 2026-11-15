using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    /// <summary>
    /// Chat message format.
    /// See <see href="https://platform.openai.com/docs/guides/chat/introduction">the documentation</see> for more details
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Role of the message author: <c>system</c>, <c>user</c>, <c>assistant</c>, <c>tool</c> or <c>function</c>.
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; }

        /// <summary>
        /// Message content. Accepts a <see cref="string"/> or an array of content parts
        /// (for example <c>text</c> and <c>image_url</c> parts for vision input).
        /// When a response is deserialized, plain text is returned as <see cref="string"/> and
        /// multi-part content as a <see cref="JsonElement"/> array.
        /// </summary>
        [JsonPropertyName("content")]
        [JsonConverter(typeof(ChatMessageContentConverter))]
        public object Content { get; set; }

        /// <summary>
        /// Optional participant name, used to distinguish authors with the same role.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Tool calls requested by the assistant.
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public List<ChatToolCall> ToolCalls { get; set; }

        /// <summary>
        /// Id of the tool call this message answers. Set on messages with the <c>tool</c> role.
        /// </summary>
        [JsonPropertyName("tool_call_id")]
        public string ToolCallId { get; set; }

        /// <summary>
        /// Legacy function call returned by the assistant (deprecated in favour of <see cref="ToolCalls"/>).
        /// </summary>
        [JsonPropertyName("function_call")]
        public ChatFunctionCall FunctionCall { get; set; }

        /// <summary>
        /// Returns <see cref="Content"/> as text when it is a string or a JSON string element;
        /// returns <c>null</c> for multi-part content.
        /// </summary>
        [JsonIgnore]
        public string ContentText
        {
            get
            {
                if (Content is string text)
                {
                    return text;
                }

                if (Content is JsonElement element && element.ValueKind == JsonValueKind.String)
                {
                    return element.GetString();
                }

                return null;
            }
        }
    }
}
