using System.Net.Http.Headers;
using System.Text.Json;
using Plugin.Maui.Audio;

namespace AI_dialog.Services;

public class SpeechToTextService
{
    private const string GroqApiKey = ApiKeys.Groq;

    private readonly IAudioRecorder _recorder = AudioManager.Current.CreateRecorder();
    private readonly HttpClient _httpClient = new();

    public string Language { get; set; } = "ru";

    public bool IsRecording => _recorder.IsRecording;

    public Task StartAsync() => _recorder.StartAsync();

    public async Task<string> StopAndTranscribeAsync()
    {
        var audioSource = await _recorder.StopAsync();
        using var audioStream = audioSource.GetAudioStream();

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GroqApiKey);

        using var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(audioStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        content.Add(fileContent, "file", "audio.wav");
        content.Add(new StringContent("whisper-large-v3-turbo"), "model");
        content.Add(new StringContent(Language), "language");

        var response = await _httpClient.PostAsync(
            "https://api.groq.com/openai/v1/audio/transcriptions",
            content
        );

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Помилка розпізнавання: {responseJson}");

        using var document = JsonDocument.Parse(responseJson);

        return document.RootElement.GetProperty("text").GetString() ?? "";
    }
}
