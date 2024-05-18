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

                if (resistance > 0)
                {
                    currentResistance = resistance;
                }
            }
            else if (command.Contains("increase resistance"))
            {
                int increment = ExtractResistanceLevel(command);

                if (increment > 0)
                {
                    currentResistance += increment;
                }
                else
                {
                    currentResistance++;
                }
            }
            else if (command.Contains("decrease resistance"))
            {
                int decrement = ExtractResistanceLevel(command);

                if (decrement > 0)
                {
                    currentResistance -= decrement;
                }
                else
                {
                    currentResistance--;
                }
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
            ["11"] = 11,
            ["twelve"] = 12,
            ["12"] = 12,
            ["thirteen"] = 13,
            ["13"] = 13,
            ["fourteen"] = 14,
            ["14"] = 14,
            ["fifteen"] = 15,
            ["15"] = 15,
            ["sixteen"] = 16,
            ["16"] = 16,
            ["seventeen"] = 17,
            ["17"] = 17,
            ["eighteen"] = 18,
            ["18"] = 18,
            ["nineteen"] = 19,
            ["19"] = 19,
            ["twenty"] = 20,
            ["20"] = 20,
            ["twenty-one"] = 21,
            ["21"] = 21,
            ["twenty-two"] = 22,
            ["22"] = 22,
            ["twenty-three"] = 23,
            ["23"] = 23,
            ["twenty-four"] = 24,
            ["24"] = 24,
            ["twenty-five"] = 25,
            ["25"] = 25,
            ["twenty-six"] = 26,
            ["26"] = 26,
            ["twenty-seven"] = 27,
            ["27"] = 27,
            ["twenty-eight"] = 28,
            ["28"] = 28,
            ["twenty-nine"] = 29,
            ["29"] = 29,
            ["thirty"] = 30,
            ["30"] = 30
        };
    }
}
