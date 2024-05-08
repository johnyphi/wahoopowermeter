using System.Threading.Tasks;
using WahooPowerMeter.Models;

namespace WahooPowerMeter.Services
{
    public delegate void PowerMeterValueChangedDelegate(int value);

    public interface IPowerMeterService
    {
        event PowerMeterValueChangedDelegate ValueChanged;
        UnitOfMeasurement Unit { get; }
        int Value { get; }

        Task StartAsync();
        Task UpdateAsync(float speedInKmH);
    }
}
