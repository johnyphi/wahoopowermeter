using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WahooPowerMeter.Processors;
using WahooPowerMeter.Services;

namespace WahooPowerMeter
{
    public class WahooPowerMeterApp
    {
        private ISpeedSensorService SpeedSensorService;
        private IPowerMeterService PowerMeterService;
        private ISpeechService SpeechService;

        private int ReistanceLevel = 0;

        private readonly ILogger<WahooPowerMeterApp> Logger;

        public WahooPowerMeterApp(ISpeedSensorService speedSensorService, IPowerMeterService powerMeterService, ISpeechService speechService, ILogger<WahooPowerMeterApp> logger)
        { 
            SpeedSensorService = speedSensorService;
            PowerMeterService = powerMeterService;
            SpeechService = speechService;

            Logger = logger;
        }

        public async Task RunAsync()
        {
            SpeedSensorService.ValueChanged += SpeedSensorService_ValueChanged;
            PowerMeterService.ValueChanged += PowerService_ValueChanged;
            SpeechService.Recognized += SpeechService_Recognized;

            var isConected = await SpeedSensorService.ConnectAsync();

            if (!isConected)
            {
                Logger.LogWarning("Could not connect to speed sensor");
                return;
            }

            await PowerMeterService.StartAsync();
            await SpeechService.StartContinuousRecognitionAsync();

            Console.ReadKey();
        }

        private async void SpeedSensorService_ValueChanged(float value)
        {
            Logger.LogInformation($"Speed: [{value}] {SpeedSensorService.Unit.ToString()}");
            await PowerMeterService.UpdateAsync(value);
        }

        private void PowerService_ValueChanged(int value)
        {
            Logger.LogInformation($"Power: [{value}] {PowerMeterService.Unit.ToString()}");
        }

        private async void SpeechService_Recognized(string value)
        {
            var command = value.ToLower().Trim('.');
            var message = string.Empty;

            if (command.Contains("increase resistance"))
            {
                ReistanceLevel++;
                message = $"Resistance level increased to {ReistanceLevel}";
            }
            else if (command.Contains("decrease resistance"))
            {
                ReistanceLevel--;
                message = $"Resistance level decreased to {ReistanceLevel}";
            }
            else if (command.Contains("current resistance"))
            {
                message = $"Current resistance level is {ReistanceLevel}";
            }
            else if (command.Contains("stop session"))
            {
                message = "Stopping session";
            }

            if (!string.IsNullOrEmpty(message))
            {
                await SpeechService.SpeakTextAsync(message);
                Logger.LogInformation(message);
            }
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
                .AddSingleton<ISpeechService, SpeechService>()
                .AddSingleton<IPacketProcessor, CSCPacketProcessor>()
                .BuildServiceProvider();

            var app = serviceProvider.GetRequiredService<WahooPowerMeterApp>();
            await app.RunAsync();            
        }      
    }
}