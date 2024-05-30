using WahooPowerMeter.Services;

namespace WahooPowerMeter.ConsoleApp.Configuration
{
    internal class PowerMeterServiceConfiguration : IPowerMeterServiceConfiguration
    {
        public double Constant => 0.1118;
    }
}
