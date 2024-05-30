using WahooPowerMeter.Services;

namespace WahooPowerMeter.WPF.Configuration
{
    internal class SpeedSensorServiceConfiguration : ISpeedSensorServiceConfiguration
    {
        public string SpeedSensorName => "Wahoo SPEED";

        public int WheelDiameterMillimeters => 450;
    }
}
