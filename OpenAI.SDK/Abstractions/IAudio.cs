using OpenAI.SDK.Models.Audio;
using System.IO;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    /// <summary>
    /// Provides access to the <see href="https://platform.openai.com/docs/api-reference/audio">audio</see> api
    /// </summary>
    public interface IAudio
    {
        /// <summary>
        /// POST https://api.openai.com/v1/audio/transcriptions
        /// Transcribes audio into the input language.
        /// </summary>
        /// <param name="file">The audio file to transcribe, in one of these formats: mp3, mp4, mpeg, mpga, m4a, wav, or webm.</param>
        /// <param name="fileName">The file name</param>
        /// <param name="model">ID of the model to use. Only whisper-1 is currently available.</param>
        /// <param name="prompt">An optional text to guide the model's style or continue a previous audio segment. The prompt should match the audio language.</param>
        /// <param name="responseFormat">The format of the transcript output, in one of these options: json, text, srt, verbose_json, or vtt.</param>
        /// <param name="temperatue">What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic. We generally recommend altering this or top_p but not both.</param>
        /// <param name="language">The language of the input audio. Supplying the input language in ISO-639-1 format will improve accuracy and latency.</param>
        /// <returns></returns>
        Task<TranscriptionResponse> CreateTranscription(Stream file, string fileName, string model, string prompt, string responseFormat, double? temperatue, string language);

        /// <summary>
        /// POST https://api.openai.com/v1/audio/translations
        /// Translates audio into into English.
        /// </summary>
        /// <param name="file">The audio file to translate, in one of these formats: mp3, mp4, mpeg, mpga, m4a, wav, or webm.</param>
        /// <param name="fileName">The file name</param>
        /// <param name="model">ID of the model to use. Only whisper-1 is currently available.</param>
        /// <param name="prompt">An optional text to guide the model's style or continue a previous audio segment. The prompt should be in English.</param>
        /// <param name="responseFormat">The format of the transcript output, in one of these options: json, text, srt, verbose_json, or vtt.</param>
        /// <param name="temperatue">The sampling temperature, between 0 and 1. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic. If set to 0, the model will use log probability to automatically increase the temperature until certain thresholds are hit.</param>
        /// <returns></returns>
        Task<TranslationResponse> CreateTranslation(Stream file, string fileName, string model, string prompt, string responseFormat, double? temperatue);
    }
}
