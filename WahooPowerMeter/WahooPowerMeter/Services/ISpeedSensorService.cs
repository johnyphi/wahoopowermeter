using System.Threading.Tasks;
using WahooPowerMeter.Models;

namespace WahooPowerMeter.Services
{
    public delegate void SpeedSensorValueChangedDelegate(float value);

    public interface ISpeedSensorService
    {       
        event SpeedSensorValueChangedDelegate ValueChanged;
        UnitOfMeasurement Unit { get; }
        float Value { get; }

        Task<bool> ConnectAsync();
    }
}
