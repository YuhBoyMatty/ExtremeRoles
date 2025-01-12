﻿using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Module;
using UnityEngine;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles
{
    public sealed class ExtremeRolePluginBehavior : MonoBehaviour
    {
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                Logging.Dump();
            }
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F9) &&
                ExtremeRolesPlugin.DebugMode.Value &&
                RoleAssignState.Instance.IsRoleSetUpEnd)
            {
                Logging.Debug($"{PlayerStatistics.Create()}");
            }
            if (Input.GetKeyDown(KeyCode.F10) &&
                ExtremeRolesPlugin.DebugMode.Value)
            {
                foreach(PetData pet in FastDestroyableSingleton<HatManager>.Instance.allPets)
                {
                    Logging.Debug($"Cosmic Id:{pet.ProdId}");
                }
            }
            if (Input.GetKeyDown(KeyCode.F12))
            {
                var ghostRole = GhostRoles.ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
                if (ghostRole != null)
                {
                    Logging.Debug("---- Ghost Role Button Info ----");
                    Logging.Debug($"Cool Time:{ghostRole.Button.CoolTime}");
                    Logging.Debug($"Active Time:{ghostRole.Button.ActiveTime}");
                    Logging.Debug($"Button State:{ghostRole.Button.State}");
                }
            }
#endif
        }
    }
}
