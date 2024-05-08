using System;
using System.Threading.Tasks;
using WahooPowerMeter.Services;

namespace WahooPowerMeter
{
    public class Program
    {
        private static ISpeedSensorService speedSensorService = new SpeedSensorService();
        private static IPowerMeterService powerMeterService = new PowerMeterService();

        public static async Task Main(string[] args)
        {
            speedSensorService.ValueChanged += SpeedSensorService_ValueChanged;
            powerMeterService.ValueChanged += PowerService_ValueChanged;

            var isConected = await speedSensorService.ConnectAsync();

            if (!isConected)
            {
                Console.WriteLine("Could not connect to speed sensor");
                return;
            }

            await powerMeterService.StartAsync();

            while (true) { }
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