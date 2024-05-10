using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace WahooPowerMeter.Services
{
    public class SpeechService : ISpeechService
    {
        private SpeechSynthesizer SpeechSynthesizer;
        private TaskCompletionSource<int> StopRecognitionTask;

        private string SpeechKey = "ad459262d8ec4047b6cb05084d89877d";
        private string SpeechRegion = "uksouth";

        private readonly ILogger<SpeechService> Logger;

        public SpeechService(ILogger<SpeechService> logger)
        {
            Logger = logger;
        }

        public event SpeechRecognizedDelegate Recognized;

        public async Task SpeakTextAsync(string text)
        {
            if (SpeechSynthesizer == null)
            {
                SpeechSynthesizer = new SpeechSynthesizer(SpeechConfig.FromSubscription(SpeechKey, SpeechRegion));
            }

            using (var result = await SpeechSynthesizer.SpeakTextAsync(text))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Logger.LogDebug($"SPEAKING: Text={text}");
                }
                else
                {
                    Logger.LogError($"Speech synthesis failed with reason: {result.Reason}");
                }
            }
        }

        public async Task StartContinuousRecognitionAsync()
        {
            var config = SpeechConfig.FromSubscription(SpeechKey, SpeechRegion);

            // Creates a speech recognizer using microphone as audio input.
            using (var recognizer = new SpeechRecognizer(config))
            {
                // Subscribes to events.
                recognizer.Recognizing += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizingSpeech)
                    {
                        Logger.LogDebug($"RECOGNIZING: Text={e.Result.Text}");
                    }
                };

                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Logger.LogDebug($"RECOGNIZED: Text={e.Result.Text}");
                        Recognized?.Invoke(e.Result.Text);
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        Logger.LogDebug("NOMATCH: Speech could not be recognized.");
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    Logger.LogDebug($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Logger.LogDebug($"CANCELED: ErrorCode={e.ErrorCode}");
                        Logger.LogDebug($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Logger.LogDebug($"CANCELED: Did you update the subscription info?");
                    }
                };

                recognizer.SessionStarted += (s, e) =>
                {
                    Logger.LogDebug("\n    Session started event.");
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Logger.LogDebug("\n    Session stopped event.");
                    Logger.LogDebug("\nStop recognition.");
                };

                // Starts recognizing.
                Logger.LogInformation($"Listening for commands...");
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                // Waits for a single successful keyword-triggered speech recognition (or error).
                // Use Task.WaitAny to keep the task rooted.
                Task.WaitAny(new[] { StopRecognitionTask.Task });

                // Stops recognition.
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }
        }

        public void StopContinuousRecognitionAsync()
        {
            StopRecognitionTask.SetResult(0);
        }
    }
}
