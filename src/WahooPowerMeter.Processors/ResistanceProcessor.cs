using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace WahooPowerMeter.Processors
{
    public class ResistanceProcessor : IResistanceProcessor
    {
        private readonly ILogger<ResistanceProcessor> Logger;

        public ResistanceProcessor(ILogger<ResistanceProcessor> logger) 
        { 
            Logger = logger;
        }

        public int ProcessResistanceCommand(string command, int currentResistance)
        {
            if (command.Contains("set resistance"))
            {
                int resistance = ExtractResistanceLevel(command);
                currentResistance = resistance;
            }

            return currentResistance;
        }

        private int ExtractResistanceLevel(string command)
        {
            var parts = command.Split(' ');
            var number = parts.FirstOrDefault(p => resistanceMap.ContainsKey(p));

            if (number != null && resistanceMap.ContainsKey(number))
            {
                return resistanceMap[number];
            }

            return 0;
        }

        private readonly IDictionary<string, int> resistanceMap = new Dictionary<string, int>()
        {
            ["zero"] = 0,
            ["0"] = 0,
            ["one"] = 1,
            ["1"] = 1,
            ["two"] = 2,
            ["2"] = 2,
            ["three"] = 3,
            ["3"] = 3,
            ["four"] = 4,
            ["4"] = 4,
            ["five"] = 5,
            ["5"] = 5,
            ["six"] = 6,
            ["6"] = 6,
            ["seven"] = 7,
            ["7"] = 7,
            ["eight"] = 8,
            ["8"] = 8,
            ["nine"] = 9,
            ["9"] = 9,
            ["ten"] = 10,
            ["10"] = 10,
            ["eleven"] = 11,
            ["11"] = 11
        };
    }
}
