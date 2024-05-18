using System.Configuration;
using WahooPowerMeter.Services;

namespace WahooPowerMeter.WPF.Configuration
{
    public class SpeechServiceConfiguration : ISpeechServiceConfiguration
    {
        public string SpeechKey => ConfigurationManager.AppSettings["SpeechKey"];

        public string SpeechRegion => ConfigurationManager.AppSettings["SpeechRegion"];
    }
}
