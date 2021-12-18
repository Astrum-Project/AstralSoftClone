using ExitGames.Client.Photon;
using MelonLoader;
using System.Reflection;
using VRC.Core;
using System.Linq;
using UnhollowerRuntimeLib.XrefScans;
using VRC;
using System;
using Astrum.AstralCore.UI.Attributes;
using Astrum.AstralCore.Managers;

[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(Astrum.AstralSoftClone), nameof(Astrum.AstralSoftClone), "0.3.1", downloadLink: "github.com/Astrum-Project/" + nameof(Astrum.AstralSoftClone))]
[assembly: MelonColor(ConsoleColor.DarkYellow)]
[assembly: MelonOptionalDependencies("AstralCore")]

namespace Astrum
{
    public class AstralSoftClone : MelonMod
    {
        private static bool _state = false;
        [UIProperty<bool>("SoftClone", "Enabled")]
        public static bool State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;
                if (!value)
                    ReloadAvatar();
            }
        }

        private static Il2CppSystem.Object AvatarDictCache { get; set; }
        private static MethodInfo _loadAvatarMethod;

        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(
                typeof(VRCNetworkingClient).GetMethod(nameof(VRCNetworkingClient.OnEvent)),
                typeof(AstralSoftClone).GetMethod(nameof(HookOnEvent), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod()
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
        }

        [UIButton("SoftClone", "Reload")]
        public static void ReloadAvatar() => _loadAvatarMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { true });
        [UIButton("SoftClone", "CloneSelected")]
        public static void SoftCloneSelected() => SoftCloneUser(SelectionManager.SelectedPlayer.id);

        public static void SoftCloneUser(string userID)
        {
            _state = true;
            AvatarDictCache = PlayerManager.prop_PlayerManager_0
                .field_Private_List_1_Player_0
                .ToArray()
                .FirstOrDefault(a => a.field_Private_APIUser_0.id == userID)?
                .prop_Player_1.field_Private_Hashtable_0["avatarDict"];
            ReloadAvatar();
        }

        private static void HookOnEvent(ref EventData __0)
        {
            if (_state
                && __0.Code == 42
                && AvatarDictCache != null
                && __0.Sender == VRC.SDKBase.Networking.LocalPlayer.playerId
            ) __0.Parameters[251].Cast<Il2CppSystem.Collections.Hashtable>()["avatarDict"] = AvatarDictCache;
        }
    }
}
