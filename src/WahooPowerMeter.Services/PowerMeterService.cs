using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WahooPowerMeter.Models;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace WahooPowerMeter.Services
{
    public class PowerMeterService : IPowerMeterService
    {
        private GattLocalCharacteristic PowerCharacteristic = null;
        private int PowerInWatts = 0;

        public event PowerMeterValueChangedDelegate ValueChanged;
        public UnitOfMeasurement Unit { get; } = UnitOfMeasurement.Watts;
        public int Value => PowerInWatts;

        private readonly ILogger<PowerMeterService> Logger;

        public PowerMeterService(ILogger<PowerMeterService> logger)
        {
            Logger = logger;
        }

        public async Task StartAsync()
        {
            Logger.LogInformation("Starting power meter service...");

            var adapter = await BluetoothAdapter.GetDefaultAsync();

            if (adapter == null)
            {
                Logger.LogInformation("Bluetooth is not available on this device.");
                return;
            }

            // Create GATT server
            GattServiceProviderResult result = await GattServiceProvider.CreateAsync(GattServiceUuids.CyclingPower);
            
            if (result.Error != BluetoothError.Success)
            {
                Logger.LogError("Error creating GATT service provider.");
                return;
            }

            // Get GATT service
            GattServiceProvider serviceProvider = result.ServiceProvider;
            GattLocalService service = serviceProvider.Service;

            // Add characteristics
            var gattOperandParameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Notify | GattCharacteristicProperties.Read,
                WriteProtectionLevel = GattProtectionLevel.Plain,
                UserDescription = "Power Characteristic"
            };

            var powerMeasurementCharacteristicResult = await service.CreateCharacteristicAsync(GattCharacteristicUuids.CyclingPowerMeasurement, gattOperandParameters);
            PowerCharacteristic = powerMeasurementCharacteristicResult.Characteristic;

            // Start advertising
            var advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };

            serviceProvider.StartAdvertising(advParameters);
            Logger.LogInformation("Power meter service started.");
        }

        public async Task UpdateAsync(float speedInKmH, int resistanceLevel)
        {
            //PowerInWatts = CalcuatatePowerOG(speedInKmH);
            PowerInWatts = CalculateVirtualPower(speedInKmH, resistanceLevel);

            byte[] data = FormatPowerMeasurement(PowerInWatts);
            var writer = new DataWriter();
            writer.WriteBytes(data);
            await PowerCharacteristic?.NotifyValueAsync(writer.DetachBuffer());

            ValueChanged?.Invoke(PowerInWatts);
        }

        private static byte[] FormatPowerMeasurement(int powerOutput)
        {
            // Byte 0: Flags
            byte flags = 0x01; // Instantaneous power field present

            // Byte 1: Instantaneous power (LSB)
            byte powerLSB = (byte)(powerOutput & 0xFF);

            // Byte 2: Instantaneous power (MSB)
            byte powerMSB = (byte)((powerOutput >> 8) & 0xFF);

            // Byte 3: Pedal power balance (optional, set to 0)
            byte pedalPowerBalance = 0x00;

            // Byte 4: Pedal power balance reference (optional, set to 0)
            byte pedalPowerBalanceReference = 0x00;

            // Byte 5: Accumulated torque (optional, set to 0)
            byte accumulatedTorque = 0x00;

            // Byte 6: Accumulated torque source (optional, set to 0)
            byte accumulatedTorqueSource = 0x00;

            // Construct the packet
            byte[] packet = { flags, powerMSB, powerLSB, pedalPowerBalance, pedalPowerBalanceReference, accumulatedTorque, accumulatedTorqueSource };

            return packet;
        }

        private static int CalcuatatePowerOG(float speedInKmH)
        {
            double rawPowerInWatts = ((0.0009925 * speedInKmH + 0.003019) * speedInKmH + 6.377) * speedInKmH;
            return (int)rawPowerInWatts;
        }

        public int CalculateVirtualPower(double speedKmh, int resistanceLevel)
        {
            const double constant = 0.1;
            const double exponent = 2.5;

            // Calculate the resistance multiplier (for 12 resistance levels)
            double resistanceMultiplier = 1 + (resistanceLevel - 1) / 11.0;

            // Calculate power using the power curve formula
            double power = constant * Math.Pow(speedKmh, exponent) * resistanceMultiplier;

            return (int)power;
        }
    }
}
