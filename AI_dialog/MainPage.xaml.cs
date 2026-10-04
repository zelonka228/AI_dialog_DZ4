using AI_dialog.Services;
using Microsoft.Maui.ApplicationModel;

namespace AI_dialog
{
    public partial class MainPage : ContentPage
    {
        private readonly AiService _ai = new();
        private readonly SpeechToTextService _speech = new();
        private bool _busy;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSendClicked(object? sender, EventArgs e)
        {
            var prompt = PromptEntry.Text?.Trim();

            if (_busy || string.IsNullOrEmpty(prompt))
                return;

            PromptEntry.Text = string.Empty;
            await AskAiAsync(prompt);
        }

        private async void OnMicClicked(object? sender, EventArgs e)
        {
            if (_busy)
                return;

            try
            {
                if (!_speech.IsRecording)
                {
                    var status = await Permissions.RequestAsync<Permissions.Microphone>();

                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlertAsync("Мікрофон", "Немає доступу до мікрофона.", "OK");
                        return;
                    }

                    await _speech.StartAsync();
                    MicButton.Text = "⏹";
                    StatusLabel.Text = "Записую... натисніть ще раз, щоб відправити";
                    return;
                }

                MicButton.Text = "🎤";
                SetBusy(true, "Розпізнаю...");
                var text = await _speech.StopAndTranscribeAsync();
                SetBusy(false);

                if (string.IsNullOrWhiteSpace(text))
                {
                    await DisplayAlertAsync("Голос", "Не вдалося розпізнати мовлення.", "OK");
                    return;
                }

                await AskAiAsync(text);
            }
            catch (Exception ex)
            {
                MicButton.Text = "🎤";
                SetBusy(false);
                await DisplayAlertAsync("Помилка", ex.Message, "OK");
            }
        }

        private async Task AskAiAsync(string prompt)
        {
            SetBusy(true, "AI думає...");

            try
            {
                var answer = await _ai.AskAi(prompt);
                ChatLabel.Text += $"Ви: {prompt}\nAI: {answer}\n\n";
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Помилка", ex.Message, "OK");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            _speech.Language = LanguagePicker.SelectedIndex == 1 ? "uk" : "ru";
        }

        private void SetBusy(bool busy, string status = "")
        {
            _busy = busy;
            MicButton.IsEnabled = !busy;
            SendButton.IsEnabled = !busy;
            StatusLabel.Text = status;
        }
    }
}
