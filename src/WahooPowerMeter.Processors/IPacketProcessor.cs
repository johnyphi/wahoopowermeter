using WahooPowerMeter.Models;

namespace WahooPowerMeter.Processors
{
    public interface IPacketProcessor
    {
        CSCPacket Process(byte[] data);
    }
}
