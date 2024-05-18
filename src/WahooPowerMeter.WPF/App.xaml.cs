using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using WahooPowerMeter.Processors;
using WahooPowerMeter.Services;

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
                .AddSingleton<IPowerMeterService, PowerMeterService>()
                .AddSingleton<ISpeechService, SpeechService>()
                .AddSingleton<IPacketProcessor, CSCPacketProcessor>()
                .BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
