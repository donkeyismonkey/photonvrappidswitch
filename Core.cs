using System;
using System.Diagnostics;
using System.Reflection;
using MelonLoader;

[assembly: MelonInfo(typeof(PhotonVRAppIdSwitch.Core), "PhotonVRAppIdSwitch", "1.0.2", "Me")]
[assembly: MelonGame(null, null)]

namespace PhotonVRAppIdSwitch
{
    public class Core : MelonMod
    {
        private const string AppId = "b0381d8e-117e-461b-915e-4c0d8dd4f1b0";
        private const string VoiceId = "838c89aa-44f7-4cf3-8e32-875636b6431f";
        private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        private readonly Stopwatch timer = Stopwatch.StartNew();
        private Type managerType;
        private long interval = 3000;
        private int lookups;
        private bool errorLogged;

        public override void OnUpdate()
        {
            if (timer.ElapsedMilliseconds < interval) return;
            timer.Restart();
            interval = 2000;
            Apply();
        }

        private void Apply()
        {
            try
            {
                if (managerType == null)
                {
                    if (lookups >= 5) return;
                    lookups++;
                    managerType = FindType();
                    LoggerInstance.Msg("Lookup " + lookups + ": manager=" + (managerType != null));
                    return;
                }

                object manager = Get(managerType, "Manager");
                if (manager == null) return;
                Set(manager, "AppId", AppId);
                Set(manager, "VoiceAppId", VoiceId);
            }
            catch (Exception e)
            {
                if (!errorLogged)
                {
                    errorLogged = true;
                    LoggerInstance.Error(e.ToString());
                }
            }
        }

        private static Type FindType()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;
                if (assemblyName != "Assembly-CSharp" && assemblyName != "Il2CppAssembly-CSharp") continue;
                Type type = assembly.GetType("Photon.VR.PhotonVRManager", false) ?? assembly.GetType("Il2CppPhoton.VR.PhotonVRManager", false);
                if (type != null) return type;
            }
            return null;
        }

        private static object Get(Type type, string name)
        {
            PropertyInfo property = type.GetProperty(name, All);
            if (property != null) return property.GetValue(null, null);
            FieldInfo field = type.GetField(name, All);
            return field?.GetValue(null);
        }

        private void Set(object instance, string name, string value)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(name, All);
            if (property != null && property.CanWrite)
            {
                if ((string)property.GetValue(instance, null) != value)
                {
                    property.SetValue(instance, value, null);
                    LoggerInstance.Msg(name + " replaced");
                }
                return;
            }
            FieldInfo field = type.GetField(name, All);
            if (field != null && (string)field.GetValue(instance) != value)
            {
                field.SetValue(instance, value);
                LoggerInstance.Msg(name + " replaced");
            }
        }
    }
}
