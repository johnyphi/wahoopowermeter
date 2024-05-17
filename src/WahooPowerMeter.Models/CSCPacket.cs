namespace WahooPowerMeter.Models
{
    public class CSCPacket
    {
        public int Revolutions { get; set; }
        public short Ticks { get; set; }
        public CSCPacketType MeasurementType { get; set; }
    }
}
