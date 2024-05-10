using InTheHand.Bluetooth;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WahooPowerMeter.Models;
using WahooPowerMeter.Processors;

namespace WahooPowerMeter.Services
{
    public class SpeedSensorService : ISpeedSensorService
    {
        private const string SpeedSensorName = "Wahoo SPEED";

        private int PreviousRevolutions;
        private int PreviousTicks;
        private float RevolutionsPerSecond;
        private float KilometresPerHour;
        private const float WheelDiameterMillimeters = 450; 
        private const float PI = (float)Math.PI;

        private readonly Guid ServiceUUID = new Guid("00001816-0000-1000-8000-00805f9b34fb");
        private readonly Guid CharacteristicUUID = new Guid("00002a5b-0000-1000-8000-00805f9b34fb");
        private readonly IPacketProcessor PacketProcessor;

        public event SpeedSensorValueChangedDelegate ValueChanged;        
        public UnitOfMeasurement Unit => UnitOfMeasurement.KmPerHour;
        public float Value => KilometresPerHour;

        private readonly ILogger<SpeedSensorService> Logger;

        public SpeedSensorService(IPacketProcessor packetProcessor, ILogger<SpeedSensorService> logger)
        {
            PacketProcessor = packetProcessor;
            Logger = logger;
        }

        public async Task<bool> ConnectAsync()
        {
            Logger.LogInformation("Scanning for devices...");

            var discoveredDevices = await Bluetooth.ScanForDevicesAsync();
            Logger.LogInformation($"Found [{discoveredDevices?.Count}] devices");

            var device = discoveredDevices.FirstOrDefault(d => d.Name.Contains(SpeedSensorName));

            if (device == null)
            {
                Logger.LogError($"Could not find device [{SpeedSensorName }]");
                return false;
            }

            await device.Gatt.ConnectAsync();

            Logger.LogInformation($"Connected to [{device.Name}]");

            var serives = await device.Gatt.GetPrimaryServicesAsync();
            var service = serives.FirstOrDefault(s => s.Uuid.Value == ServiceUUID);

            if (service == null)
            {
                Logger.LogError("Could not find service");
                return false;
            }

            var characteristics = await service.GetCharacteristicsAsync();
            var characteristic = characteristics.FirstOrDefault(c => c.Uuid.Value == CharacteristicUUID);

            if (characteristic == null)
            {
                Logger.LogError("Could not find characteristic");
                return false;
            }            

            await characteristic.StartNotificationsAsync();
            characteristic.CharacteristicValueChanged += Characteristic_ValueChanged;

            Logger.LogDebug($"Subscribing to notifications for characteristic [{characteristic.UserDescription}] on service [{service.Uuid}]");

            return true;
        }

        private void Characteristic_ValueChanged(object sender, GattCharacteristicValueChangedEventArgs args)
        {
            var packet = PacketProcessor.Process(args.Value);

            if (packet == null || packet.MeasurementType != CSCPacketType.Wheel)
            {
                return;
            }

            int revolutions = (packet.Revolutions - PreviousRevolutions) & 0xFFFFF;
            int ticks = (packet.Ticks - PreviousTicks) & 0xFFFFF;
                
            if (revolutions >= 1 && revolutions <= 5)
            {
                float ticksPerRevolution = Math.Abs(ticks) / (float)revolutions;

                if (ticksPerRevolution >= 100 && ticksPerRevolution <= 10000)
                {
                    RevolutionsPerSecond = 1000 / ticksPerRevolution;

                    float metersPerSecond = RevolutionsPerSecond * (WheelDiameterMillimeters / 1000) * PI;
                    KilometresPerHour = metersPerSecond * 3600 / 1000;
                    ValueChanged?.Invoke(KilometresPerHour);
                }
            }

            PreviousRevolutions = packet.Revolutions;
            PreviousTicks = packet.Ticks;
        }
    }
}
