using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using WahooPowerMeter.Processors;
using WahooPowerMeter.Services;
using WahooPowerMeter.WPF.Configuration;

namespace WahooPowerMeter.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ServiceProvider = new ServiceCollection()
                .AddLogging(lb => lb.AddConsole())
                .AddTransient<MainWindow>()
                .AddSingleton<ISpeedSensorService, SpeedSensorService>()
                .AddSingleton<ISpeedSensorServiceConfiguration, SpeedSensorServiceConfiguration>()
                .AddSingleton<IPowerMeterService, PowerMeterService>()
                .AddSingleton<IPowerMeterServiceConfiguration, PowerMeterServiceConfiguration>()
                .AddSingleton<ISpeechService, SpeechService>()
                .AddSingleton<ISpeechServiceConfiguration, SpeechServiceConfiguration>()
                .AddSingleton<IPacketProcessor, CSCPacketProcessor>()
                .AddSingleton<IResistanceProcessor, ResistanceProcessor>()
                .BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
