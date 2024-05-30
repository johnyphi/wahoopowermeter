using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WahooPowerMeter.Processors;
using WahooPowerMeter.Services;

namespace WahooPowerMeter.ConsoleApp
{
    public class WahooPowerMeterApp
    {
        private ISpeedSensorService SpeedSensorService;
        private IPowerMeterService PowerMeterService;
        private IResistanceProcessor ResistanceProcessor;

        private int ResistanceLevel = 0;

        private readonly ILogger<WahooPowerMeterApp> Logger;

        public WahooPowerMeterApp(ISpeedSensorService speedSensorService, IPowerMeterService powerMeterService, ILogger<WahooPowerMeterApp> logger)
        { 
            SpeedSensorService = speedSensorService;
            PowerMeterService = powerMeterService;

            Logger = logger;
        }

        public async Task RunAsync()
        {
            SpeedSensorService.ValueChanged += SpeedSensorService_ValueChanged;
            PowerMeterService.ValueChanged += PowerService_ValueChanged;

            Logger.LogInformation("Press any key to start the session");
            Console.ReadKey();

            var isConected = await SpeedSensorService.ConnectAsync();

            if (!isConected)
            {
                Logger.LogWarning("Could not connect to speed sensor");
            }
            else
            {
                await PowerMeterService.StartAsync();
            }

            while (true)
            {
                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        Logger.LogInformation("Quitting...");
                        return;
                    case ConsoleKey.UpArrow:
                        ResistanceLevel = Math.Min(ResistanceLevel + 1, 11);
                        break;
                    case ConsoleKey.DownArrow:
                        ResistanceLevel = Math.Max(ResistanceLevel - 1, 0);
                        break;
                }

                Logger.LogInformation($"Resistance level: {ResistanceLevel}");

                Thread.Sleep(1000);
            }
        }

        private async void SpeedSensorService_ValueChanged(float value)
        {
            Logger.LogDebug($"Speed: [{value}] {SpeedSensorService.Unit.ToString()}");
            await PowerMeterService.UpdateAsync(value, ResistanceLevel);
        }

        private void PowerService_ValueChanged(int value)
        {
            Logger.LogDebug($"Power: [{value}] {PowerMeterService.Unit.ToString()}");
        }
    }

    public class Program
    {
        public async static Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(lb => lb.AddConsole())
                .AddSingleton<WahooPowerMeterApp>()
                .AddSingleton<ISpeedSensorService, SpeedSensorService>()
                .AddSingleton<IPowerMeterService, PowerMeterService>()
                .AddSingleton<IResistanceProcessor, ResistanceProcessor>()
                .AddSingleton<IPacketProcessor, CSCPacketProcessor>()
                .BuildServiceProvider();

            var app = serviceProvider.GetRequiredService<WahooPowerMeterApp>();
            await app.RunAsync();            
        }      
    }
}