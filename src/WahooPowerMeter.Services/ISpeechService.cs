using System.Threading.Tasks;

namespace WahooPowerMeter.Services
{
    public delegate void SpeechRecognizedDelegate(string value);

    public interface ISpeechService
    {
        event SpeechRecognizedDelegate Recognized;
        Task StartContinuousRecognitionAsync();
        Task SpeakTextAsync(string text);
        void StopContinuousRecognitionAsync();
    }
}
