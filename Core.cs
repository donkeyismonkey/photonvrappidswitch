using System;
using System.Diagnostics;
using System.Reflection;
using MelonLoader;

[assembly: MelonInfo(typeof(PhotonVRAppIdSwitch.Core), "PhotonVRAppIdSwitch", "1.0.4", "Me")]
[assembly: MelonGame(null, null)]

namespace PhotonVRAppIdSwitch
{
    public class Core : MelonMod
    {
        private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private readonly Stopwatch timer = Stopwatch.StartNew();
        private bool done;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("PhotonVRAppIdSwitch loaded");
        }

        public override void OnUpdate()
        {
            if (done || timer.ElapsedMilliseconds < 10000) return;
            done = true;
            try
            {
                Scan();
            }
            catch (Exception e)
            {
                LoggerInstance.Error(e.ToString());
            }
        }

        private void Scan()
        {
            LoggerInstance.Msg("SCAN START");
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;
                if (!assemblyName.Contains("Assembly-CSharp") && !assemblyName.Contains("Photon")) continue;
                LoggerInstance.Msg("ASSEMBLY " + assemblyName);

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                foreach (Type type in types)
                {
                    if (type == null) continue;
                    try
                    {
                        if (type.Name.Contains("Photon"))
                        {
                            LoggerInstance.Msg("TYPE " + type.FullName);
                        }
                        foreach (MemberInfo member in type.GetMembers(All))
                        {
                            if (member.Name.Contains("AppId"))
                            {
                                LoggerInstance.Msg("MEMBER " + type.FullName + "." + member.Name);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            LoggerInstance.Msg("SCAN END");
        }
    }
}
