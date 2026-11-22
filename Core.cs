using System;
using System.Reflection;
using MelonLoader;

[assembly: MelonInfo(typeof(PhotonVRAppIdSwitch.Core), "PhotonVRAppIdSwitch", "1.0.0", "Me")]
[assembly: MelonGame(null, null)]

namespace PhotonVRAppIdSwitch
{
    public class Core : MelonMod
    {
        private const string AppId = "b0381d8e-117e-461b-915e-4c0d8dd4f1b0";
        private const string VoiceId = "838c89aa-44f7-4cf3-8e32-875636b6431f";
        private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        private Type managerType;
        private Type networkType;
        private float nextCheck;

        public override void OnInitializeMelon()
        {
            Apply();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Apply();
        }

        public override void OnUpdate()
        {
            nextCheck -= UnityEngine.Time.unscaledDeltaTime;
            if (nextCheck > 0f) return;
            nextCheck = 0.25f;
            Apply();
        }

        private void Apply()
        {
            try
            {
                managerType ??= FindType("Photon.VR.PhotonVRManager", "Il2CppPhoton.VR.PhotonVRManager");
                networkType ??= FindType("Photon.Pun.PhotonNetwork", "Il2CppPhoton.Pun.PhotonNetwork");

                if (managerType != null)
                {
                    object manager = Get(managerType, null, "Manager");
                    if (manager != null)
                    {
                        Set(manager, "AppId", AppId);
                        Set(manager, "VoiceAppId", VoiceId);
                    }
                }

                if (networkType != null)
                {
                    object serverSettings = Get(networkType, null, "PhotonServerSettings");
                    object appSettings = serverSettings == null ? null : Get(serverSettings.GetType(), serverSettings, "AppSettings");
                    if (appSettings != null)
                    {
                        Set(appSettings, "AppIdRealtime", AppId);
                        Set(appSettings, "AppIdVoice", VoiceId);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static Type FindType(params string[] names)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (string name in names)
                {
                    Type type = assembly.GetType(name, false);
                    if (type != null) return type;
                }
            }
            return null;
        }

        private static object Get(Type type, object instance, string name)
        {
            PropertyInfo property = type.GetProperty(name, All);
            if (property != null) return property.GetValue(instance, null);
            FieldInfo field = type.GetField(name, All);
            return field?.GetValue(instance);
        }

        private static void Set(object instance, string name, string value)
        {
            Type type = instance.GetType();
            FieldInfo field = type.GetField(name, All);
            if (field != null)
            {
                if ((string)field.GetValue(instance) != value) field.SetValue(instance, value);
                return;
            }
            PropertyInfo property = type.GetProperty(name, All);
            if (property != null && property.CanWrite && (string)property.GetValue(instance, null) != value)
            {
                property.SetValue(instance, value, null);
            }
        }
    }
}
