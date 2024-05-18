using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WahooPowerMeter.Processors;
using WahooPowerMeter.Services;

namespace WahooPowerMeter.WPF
{
    internal enum ApplicationState
    {
        Stopped,
        Scanning,
        Running
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ISpeedSensorService SpeedSensorService;
        private IPowerMeterService PowerMeterService;
        private ISpeechService SpeechService;
        private IResistanceProcessor ResistanceProcessor;

        private int ResistanceLevel = 0;

        private ApplicationState State = ApplicationState.Stopped;

        private readonly ILogger<MainWindow> Logger;

        public MainWindow(ISpeedSensorService speedSensorService, IPowerMeterService powerMeterService, ISpeechService speechService, IResistanceProcessor resistanceProcessor, ILogger<MainWindow> logger)
        {
            SpeedSensorService = speedSensorService;
            PowerMeterService = powerMeterService;
            SpeechService = speechService;
            ResistanceProcessor = resistanceProcessor;

            Logger = logger;

            InitializeComponent();

            SpeedSensorService.ValueChanged += SpeedSensorService_ValueChanged;
            PowerMeterService.ValueChanged += PowerService_ValueChanged;
            SpeechService.Recognized += SpeechService_Recognized;
        }

        private async Task RunAsync()
        {
            var isConected = await SpeedSensorService.ConnectAsync();

            txtResistance.Text = ResistanceLevel.ToString();

            if (!isConected)
            {
                Logger.LogWarning("Could not connect to speed sensor");

                btnStart.IsEnabled = true;
                btnStart.Content = "Start";
                State = ApplicationState.Stopped;
            }
            else
            {
                btnStart.IsEnabled = false;
                btnStart.Content = "Stop";
                elSpeedSensor.Fill = Brushes.Green;
                State = ApplicationState.Running;

                await PowerMeterService.StartAsync();
                await SpeechService.StartContinuousRecognitionAsync();
            }
        }

        private async void SpeedSensorService_ValueChanged(float value)
        {
            Logger.LogInformation($"Speed: [{value}] {SpeedSensorService.Unit.ToString()}");

            Dispatcher.Invoke(() =>
            {
                txtSpeed.Text = value.ToString("F2");
            });
            
            await PowerMeterService.UpdateAsync(value, ResistanceLevel);
        }

        private void PowerService_ValueChanged(int value)
        {
            Logger.LogInformation($"Power: [{value}] {PowerMeterService.Unit.ToString()}");

            Dispatcher.Invoke(() =>
            {
                txtPower.Text = value.ToString();
            });
        }

        private async void SpeechService_Recognized(string value)
        {
            var command = value.ToLower().Trim('.');
            var message = string.Empty;

            var newResistanceLevel = ResistanceProcessor.ProcessResistanceCommand(command, ResistanceLevel);

            if (newResistanceLevel > ResistanceLevel)
            {
                message = $"Increasing resistance to {newResistanceLevel}";
            }
            else if (newResistanceLevel < ResistanceLevel)
            {
                message = $"Decreasing resistance to {newResistanceLevel}";
            }

            if (!string.IsNullOrEmpty(message))
            {
                await SpeechService.SpeakTextAsync(message);
                Logger.LogInformation(message);
            }

            ResistanceLevel = newResistanceLevel;

            Dispatcher.Invoke(() =>
            {
                txtResistance.Text = ResistanceLevel.ToString();
            });
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case ApplicationState.Stopped:
                    btnStart.IsEnabled = false;
                    btnStart.Content = "Scanning...";
                    await RunAsync();
                    break;
                case ApplicationState.Running:
                    // Stop everything
                    break;
            }            
        }
    }
}
