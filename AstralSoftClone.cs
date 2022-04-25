using ExitGames.Client.Photon;
using MelonLoader;
using System.Reflection;
using UnityEngine;
using VRC.Core;
using VRC.DataModel;
using System.Linq;
using UnhollowerRuntimeLib.XrefScans;
using VRC;

[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(Astrum.AstralSoftClone), nameof(Astrum.AstralSoftClone), "0.2.2", downloadLink: "github.com/Astrum-Project/" + nameof(Astrum.AstralSoftClone))]
[assembly: MelonColor(System.ConsoleColor.DarkYellow)]
[assembly: MelonOptionalDependencies("AstralCore")]

namespace Astrum
{
    public class AstralSoftClone : MelonMod
    {
        private static bool _state = false;
        private static bool hasCore = false;
        private static Il2CppSystem.Object AvatarDictCache { get; set; }
        private static MethodInfo _loadAvatarMethod;

        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(
                typeof(VRCNetworkingClient).GetMethod(nameof(VRCNetworkingClient.OnEvent)),
                typeof(AstralSoftClone).GetMethod(nameof(Detour), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod()
            );

            _loadAvatarMethod =
                typeof(VRCPlayer).GetMethods()
                .First(mi =>
                    mi.Name.StartsWith("Method_Private_Void_Boolean_")
                    && mi.Name.Length < 31
                    && mi.GetParameters().Any(pi => pi.IsOptional)
                    && XrefScanner.UsedBy(mi) // Scan each method
                        .Any(instance => instance.Type == XrefType.Method
                            && instance.TryResolve() != null
                            && instance.TryResolve().Name == "ReloadAvatarNetworkedRPC"));

            if (System.AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "AstralCore"))
                hasCore = true;
        }

        public override void OnUpdate()
        {
            if (!Input.GetKey(KeyCode.Tab)) return;

            if (Input.GetKeyDown(KeyCode.T))
            {
                if (UserSelectionManager.field_Private_Static_UserSelectionManager_0.field_Private_APIUser_1 == null)
                {
                    Log("Invalid Target");
                    return;
                }

                string target = UserSelectionManager.field_Private_Static_UserSelectionManager_0.field_Private_APIUser_1.id;

                AvatarDictCache = PlayerManager.prop_PlayerManager_0
                    .field_Private_List_1_Player_0
                    .ToArray()
                    .FirstOrDefault(a => a.field_Private_APIUser_0.id == target)
                    ?.prop_Player_1.field_Private_Hashtable_0["avatarDict"];

                _loadAvatarMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { true });
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                _state ^= true;

                Log("SoftClone " + (_state ? "On" : "Off"));
            }
        }

        private static void Log(string message)
        {
            if (!hasCore)
            {
                VRCUiManager.prop_VRCUiManager_0.field_Private_List_1_String_0.Add(message);
                MelonLogger.Msg(message);
            }
            else Extern.Notif(message);
        }

        private static void Detour(ref EventData __0)
        {
            if (_state
                && __0.Code == 42
                && AvatarDictCache != null
                && __0.Sender == Player.prop_Player_0.field_Private_VRCPlayerApi_0.playerId
            ) __0.Parameters.paramDict[245].Cast<Il2CppSystem.Collections.Hashtable>()["avatarDict"] = AvatarDictCache;
        }

        internal class Extern
        {
            public static void Notif(string message) => AstralCore.Logger.Notif(message);
        }
    }
}
