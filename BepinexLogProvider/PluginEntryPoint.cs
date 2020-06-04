using BepInEx;
using MonoMod.RuntimeDetour;
using ShinyShoe.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BepinexLogIntercepter
{

    [BepInPlugin("me.harbingerof.monstertrainlogtobeplog","BepinexLogProvider","0.5")]
    public class PluginEntryPoint : BaseUnityPlugin
    {
        List<Hook> myHooks;

        public void Awake()
        {
            myHooks = new List<Hook>();

            var LogHook = typeof(PluginEntryPoint).GetMethod("GenericLogHook", BindingFlags.NonPublic | BindingFlags.Instance);
            var LogParamHook = typeof(PluginEntryPoint).GetMethod("GenericParamLogHook", BindingFlags.NonPublic | BindingFlags.Instance);

            var methods = typeof(Log).GetMethods().Where(m =>
            {
                if (m.ReturnType != typeof(void))
                    return false;
                var paras = m.GetParameters();
                if (paras.Length < 2)
                    return false;
                if (paras[0].ParameterType != typeof(LogGroups))
                    return false;
                if (paras[1].ParameterType != typeof(string))
                    return false;
                return true;
            });
            foreach (MethodInfo m in methods)
            {
                var paras = m.GetParameters();
                if (paras.Length == 2)
                {
                    Hook h = new Hook(m, LogHook,this);
                    h.Apply();
                    myHooks.Add(h);
                }
                else
                {
                    if (paras.Length == 3 && Attribute.IsDefined(paras[2],typeof(ParamArrayAttribute)))
                    {
                        Hook h = new Hook(m, LogParamHook,this);
                        h.Apply();
                        myHooks.Add(h);
                    }
                }
            }


        }

        public Dictionary<string, string> LogMapping;

        public void OnDestroy()
        {
            foreach(Hook h in myHooks)
            {
                h.Undo();
            }
        }

        internal delegate void GenericLogDel(LogGroups logGroup, string message);
        internal delegate void GenericLogFormatDel(LogGroups logGroup, string message, params object[] paramList);

        private void GenericLogHook(GenericLogDel orig, LogGroups logGroup, string message)
        {
            BepinexLogProvider.Log(GetLevelFromMethod(orig), logGroup, message);
        }

        private void GenericParamLogHook(GenericLogFormatDel orig, LogGroups logGroup, string message, params object[] paramList)
        {
            if(paramList!=null && paramList.Length > 0)
            {
                message = string.Format(message, paramList);
            }
            BepinexLogProvider.Log(GetLevelFromMethod(orig), logGroup, message);
        }

        private readonly Regex rx = new Regex(@"_Log::(\w+)>", RegexOptions.Compiled);

        private BepInEx.Logging.LogLevel GetLevelFromMethod(Delegate del)
        {
            Match match = rx.Match(del.Method.Name);
            var level = BepInEx.Logging.LogLevel.Message;
            if (match.Success)
            {
                switch (match.Groups[1].Value)
                {
                    case "Verbose": level = BepInEx.Logging.LogLevel.Debug; break;
                    case "Debug": level = BepInEx.Logging.LogLevel.Info; break;
                    case "Warning": level = BepInEx.Logging.LogLevel.Warning; break;
                    case "Error": level = BepInEx.Logging.LogLevel.Error; break;
                }
            }
            return level;
        }
    }
}
