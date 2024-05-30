namespace WahooPowerMeter.Services
{
    public interface ISpeedSensorServiceConfiguration
    {
        string SpeedSensorName { get; }
        int WheelDiameterMillimeters { get; }
    }
}
