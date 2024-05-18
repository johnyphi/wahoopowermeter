using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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

        private int ReistanceLevel = 0;

        private ApplicationState State = ApplicationState.Stopped;

        private readonly ILogger<MainWindow> Logger;

        public MainWindow(ISpeedSensorService speedSensorService, IPowerMeterService powerMeterService, ISpeechService speechService, ILogger<MainWindow> logger)
        {
            SpeedSensorService = speedSensorService;
            PowerMeterService = powerMeterService;
            SpeechService = speechService;

            Logger = logger;

            InitializeComponent();

            SpeedSensorService.ValueChanged += SpeedSensorService_ValueChanged;
            PowerMeterService.ValueChanged += PowerService_ValueChanged;
            SpeechService.Recognized += SpeechService_Recognized;
        }

        private async Task RunAsync()
        {
            var isConected = await SpeedSensorService.ConnectAsync();

            txtResistance.Text = ReistanceLevel.ToString();

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
            
            await PowerMeterService.UpdateAsync(value);
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

            if (command.Contains("increase resistance"))
            {
                int increment = ExtractResistanceLevel(command);

                if (increment > 0)
                {
                    ReistanceLevel += increment;
                }
                else
                {
                    ReistanceLevel++;
                }

                message = $"Resistance level increased to {ReistanceLevel}";
            }
            else if (command.Contains("decrease resistance"))
            {
                int decrement = ExtractResistanceLevel(command);

                if (decrement > 0)
                {
                    ReistanceLevel -= decrement;
                }
                else
                {
                    ReistanceLevel--;
                }

                message = $"Resistance level decreased to {ReistanceLevel}";
            }
            else if (command.Contains("set resistance"))
            {
                int resistance = ExtractResistanceLevel(command);
                ReistanceLevel = resistance;

                message = $"Resistance level set to {ReistanceLevel}";
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

            Dispatcher.Invoke(() =>
            {
                txtResistance.Text = ReistanceLevel.ToString();
            });
        }

        private int ExtractResistanceLevel(string command)
        {
            var parts = command.Split(' ');
            var level = 0;

            foreach (var part in parts)
            {
                if (int.TryParse(part, out level))
                {
                    return level;
                }
            }

            return level;
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
