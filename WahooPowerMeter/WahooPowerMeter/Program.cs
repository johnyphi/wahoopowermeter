using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;
using WahooPowerMeter.Services;
using SpeechRecognizer = Microsoft.CognitiveServices.Speech.SpeechRecognizer;

namespace WahooPowerMeter
{
    public class Program
    {
        private static ISpeedSensorService speedSensorService = new SpeedSensorService();
        private static IPowerMeterService powerMeterService = new PowerMeterService();

        static string speechKey = "";
        static string speechRegion = "uksouth";

        static SpeechSynthesizer speechSynthesizer;
        static int ReistanceLevel = 0;

        static TaskCompletionSource<int> stopRecognition;

        async static Task Main(string[] args)
        {
            stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            speedSensorService.ValueChanged += SpeedSensorService_ValueChanged;
            powerMeterService.ValueChanged += PowerService_ValueChanged;

            var isConected = await speedSensorService.ConnectAsync();

            if (!isConected)
            {
                Console.WriteLine("Could not connect to speed sensor");
                return;
            }

            await powerMeterService.StartAsync();
            await ContinuousRecognitionAsync();            

            Console.ReadKey();
        }

        public static async Task ContinuousRecognitionAsync()
        {
            var config = SpeechConfig.FromSubscription(speechKey, speechRegion);

            // Creates a speech recognizer using microphone as audio input.
            using (var recognizer = new SpeechRecognizer(config))
            {
                // Subscribes to events.
                recognizer.Recognizing += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizingSpeech)
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    }
                };

                recognizer.Recognized += async (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        await ProcessRecognition(e.Result.Text);
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine("NOMATCH: Speech could not be recognized.");
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                };

                recognizer.SessionStarted += (s, e) =>
                {
                    Console.WriteLine("\n    Session started event.");
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\n    Session stopped event.");
                    Console.WriteLine("\nStop recognition.");
                    //stopRecognition.TrySetResult(0);
                };

                // Starts recognizing.
                Console.WriteLine($"Listening for commands...");
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                // Waits for a single successful keyword-triggered speech recognition (or error).
                // Use Task.WaitAny to keep the task rooted.
                Task.WaitAny(new[] { stopRecognition.Task });

                // Stops recognition.
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }
        }

        private static async Task ProcessRecognition(string text)
        {
            var command = text.ToLower().Trim('.');

            if (command.Contains("increase resistance"))
            {
                ReistanceLevel++;
                await SpeakAsync($"Resistance level increased to {ReistanceLevel}");
            }
            else if (command.Contains("decrease resistance"))
            {
                ReistanceLevel--;
                await SpeakAsync($"Resistance level decreased to {ReistanceLevel}");
            }
            else if (command.Contains("current resistance"))
            {
                await SpeakAsync($"Current resistance level is {ReistanceLevel}");
            }
            else if (command.Contains("stop session"))
            {
                await SpeakAsync("Stopping session");
                stopRecognition.TrySetResult(0);
            }
        }

        private static async Task SpeakAsync(string text)
        {
            if (speechSynthesizer == null)
            {
                speechSynthesizer = new SpeechSynthesizer(SpeechConfig.FromSubscription(speechKey, speechRegion));
            }

            using (var result = await speechSynthesizer.SpeakTextAsync(text))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Console.WriteLine($"SPEAKING: Text={text}");
                }
                else
                {
                    Console.WriteLine($"Speech synthesis failed with reason: {result.Reason}");
                }
            }
        }

        private static async void SpeedSensorService_ValueChanged(float value)
        {
            Console.WriteLine($"Speed: [{value}] {speedSensorService.Unit.ToString()}");
            await powerMeterService.UpdateAsync(value);
        }

        private static void PowerService_ValueChanged(int value)
        {
            Console.WriteLine($"Power: [{value}] {powerMeterService.Unit.ToString()}");
        }
    }
}