

using BepInEx.Logging;
using ShinyShoe.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BepinexLogIntercepter
{
    class BepinexLogProvider
    {
        static Dictionary<string, ManualLogSource> Loggers = new Dictionary<string, ManualLogSource>();

        static BepinexLogProvider()
        {
            foreach (LogGroups lg in Enum.GetValues(typeof(LogGroups))){
                Loggers.Add(lg.ToString(), BepInEx.Logging.Logger.CreateLogSource(lg.ToString()));
            }
            Loggers.Add(((LogGroups) (-1)).ToString(), BepInEx.Logging.Logger.CreateLogSource("Unknown"));
        }

        public static void Log(BepInEx.Logging.LogLevel level, LogGroups group, string message)
        {
            Loggers[group.ToString()].Log(level, message);
        }
    }
}
