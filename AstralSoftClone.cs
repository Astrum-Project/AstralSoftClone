using ExitGames.Client.Photon;
using MelonLoader;
using System.Reflection;
using UnityEngine;
using VRC.Core;
using VRC.DataModel;
using System.Linq; 

[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(Astrum.AstralSoftClone), "AstralSoftClone", "0.1.0", downloadLink: "github.com/Astrum-Project/AstralSoftClone")]
[assembly: MelonColor(System.ConsoleColor.DarkYellow)]

namespace Astrum
{
    public class AstralSoftClone : MelonMod
    {
        private static bool State = false;
        private static Il2CppSystem.Object avatarDictCache { get; set; }
        private static void Log(string message)
        {
            VRCUiManager.prop_VRCUiManager_0.field_Private_List_1_String_0.Add(message);
            MelonLogger.Msg(message);
        }
        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(
                typeof(VRCNetworkingClient)
                .GetMethod("OnEvent")
                , new HarmonyLib.HarmonyMethod(typeof(AstralSoftClone)
                .GetMethod(nameof(Detour), BindingFlags.NonPublic | BindingFlags.Static)), null, null, null, null);
        }
        private static bool Detour(ref EventData __0) 
        {
            if(__0.Code == 253 && __0.Sender == VRC.Player.prop_Player_0
                .field_Private_VRCPlayerApi_0
                .playerId && State && avatarDictCache != null)
                __0.Parameters[251].Cast<Il2CppSystem.Collections.Hashtable>()["avatarDict"] = avatarDictCache;
            return true;
        }
        public override void OnUpdate()
        {
            if(Input.GetKey(KeyCode.Tab))
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    string target = string.Empty;
                    if (UserSelectionManager.field_Private_Static_UserSelectionManager_0.field_Private_APIUser_1 == null)
                    {
                        Log("Invalid Target");
                        return;
                    }
                    else target = UserSelectionManager.field_Private_Static_UserSelectionManager_0.field_Private_APIUser_1.id;
                    avatarDictCache = VRC.PlayerManager.prop_PlayerManager_0
                        .field_Private_List_1_Player_0
                        .ToArray()
                            .Where(a => a.field_Private_APIUser_0.id == target)
                            .FirstOrDefault()
                                .prop_Player_1.field_Private_Hashtable_0["avatarDict"];
                }
                if(Input.GetKeyDown(KeyCode.A))
                {
                    State = !State;
                    if (State) Log("SoftClone On");
                    else Log("SoftClone Off");
                }
            }
            base.OnUpdate();
        }
    }
}
