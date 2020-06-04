using BepInEx;
using ShinyShoe.Logging;
using UnityEngine;
using MonoMod;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SupressChatterLogs
{

    [BepInPlugin("me.harbingerof.supresschatter","SupressChatter","1.0")]
    public class PluginEntryPoint : BaseUnityPlugin
    {
        List<Hook> myHooks;

        public void Awake()
        {
            myHooks = new List<Hook>();

            var LogHook = typeof(PluginEntryPoint).GetMethod("GenericLogHook", BindingFlags.NonPublic | BindingFlags.Static);
            var LogParamHook = typeof(PluginEntryPoint).GetMethod("GenericParamLogHook", BindingFlags.NonPublic | BindingFlags.Static);

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
                    Hook h = new Hook(m, LogHook);
                    h.Apply();
                    myHooks.Add(h);
                }
                else
                {
                    if (paras.Length == 3 && Attribute.IsDefined(paras[2],typeof(ParamArrayAttribute)))
                    {
                        Hook h = new Hook(m, LogParamHook);
                        h.Apply();
                        myHooks.Add(h);
                    }
                }
            }

        }

        public void OnDestroy()
        {
            foreach(Hook h in myHooks)
            {
                h.Undo();
            }
        }

        internal delegate void GenericLogDel(LogGroups logGroup, string message);
        internal delegate void GenericLogFormatDel(LogGroups logGroup, string message, params object[] paramList);



        private static void GenericLogHook(GenericLogDel orig, LogGroups logGroup, string message)
        {
            if(logGroup == LogGroups.Chatter)
            {
                return;
            }
            orig(logGroup, message);
        }

        private static void GenericParamLogHook(GenericLogFormatDel orig, LogGroups logGroup, string message, params object[] paramList)
        {
            if (logGroup == LogGroups.Chatter)
            {
                return;
            }
            orig(logGroup, message, paramList);
        }

    }
}
