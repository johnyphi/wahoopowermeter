namespace WahooPowerMeter.Processors
{
    public interface IResistanceProcessor
    {
        int ProcessResistanceCommand(string command, int currentResistance);
    }
}
