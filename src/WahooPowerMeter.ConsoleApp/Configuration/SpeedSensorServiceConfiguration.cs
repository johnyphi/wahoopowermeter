using WahooPowerMeter.Services;

namespace WahooPowerMeter.ConsoleApp.Configuration
{
    internal class SpeedSensorServiceConfiguration : ISpeedSensorServiceConfiguration
    {
        public string SpeedSensorName => "Wahoo SPEED";

        public int WheelDiameterMillimeters => 450;
    }
}
