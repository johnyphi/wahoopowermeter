﻿using Microsoft.Extensions.Logging;
using System;
using WahooPowerMeter.Models;

namespace WahooPowerMeter.Processors
{
    public class CSCPacketProcessor : IPacketProcessor
    {
        private readonly ILogger<CSCPacketProcessor> Logger;

        public CSCPacketProcessor(ILogger<CSCPacketProcessor> logger)
        {
            Logger = logger;
        }

        public CSCPacket Process(byte[] data)
        {
            if (data.Length == 0)
            {
                Logger.LogDebug("Empty packet = no data");
                return null;
            }

            int dataIndex = 0;

            byte dataFlags = data[dataIndex++];

            bool isWheelData = (dataFlags & (1 << 0)) != 0;
            bool isCrankData = (dataFlags & (1 << 1)) != 0;

            int dataBytesExpected = 0;

            if (isWheelData)
            {
                dataBytesExpected += 6;

                if (data.Length - dataIndex < dataBytesExpected)
                {
                    Logger.LogDebug("Error: Missing data in packet.");
                    return null;
                }

                int cumulativeWheelRevolutions = GetIntFromWithinByteArray(data, dataIndex);
                dataIndex += 4;
                
                short lastWheelEventTime = (short)GetShortFromWithinByteArray(data, dataIndex);

                return new CSCPacket
                {
                    Revolutions = cumulativeWheelRevolutions,
                    Ticks = lastWheelEventTime,
                    MeasurementType = CSCPacketType.Wheel
                };
            }
            else if (isCrankData)
            {
                dataBytesExpected += 4;

                if (data.Length - dataIndex < dataBytesExpected)
                {
                    Logger.LogDebug("Error: Missing data in packet.");
                    return null;
                }

                int cumulativeCrankRevolutions = (short)GetShortFromWithinByteArray(data, dataIndex);
                dataIndex += 2;

                short lastCrankEventTime = (short)GetShortFromWithinByteArray(data, dataIndex);

                return new CSCPacket
                {
                    Revolutions = cumulativeCrankRevolutions,
                    Ticks = lastCrankEventTime,
                    MeasurementType = CSCPacketType.Crank
                };
            }

            return null;
        }

        private int GetIntFromWithinByteArray(byte[] data, int index)
        {
            if (data.Length < index + 4)
            {
                Logger.LogDebug("Error: Insufficient bytes in the array.");
                return 0;
            }

            byte[] byteArrayIn = { data[index], data[index + 1], data[index + 2], data[index + 3] };

            return BitConverter.ToInt32(byteArrayIn, 0);
        }

        private int GetShortFromWithinByteArray(byte[] data, int index)
        {
            if (data.Length < index + 2)
            {
                Logger.LogDebug("Error: Insufficient bytes in the array.");
                return 0;
            }

            byte[] byteArrayIn = { data[index], data[index + 1] };

            return BitConverter.ToInt16(byteArrayIn, 0);
        }
    }
}
