﻿using System.Runtime.CompilerServices;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Impostor;


namespace ExtremeRoles.Module
{
    public class VisonComputer
    {
        public enum Modifier
        {
            None,
            LastWolfLightOff,
            WispLightOff,
        }

        public static VisonComputer Instance => instance;
        private static VisonComputer instance = new VisonComputer();

        private static float crewLightVison => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            FloatOptionNames.CrewLightMod);

        private static float impLightVison => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            FloatOptionNames.ImpostorLightMod);

        private const SystemTypes electrical = SystemTypes.Electrical;

        public Modifier CurrentModifier => modifier;
        private Modifier modifier = Modifier.None;

        public void SetModifier(Modifier newVison)
        {
            this.modifier = newVison;
        }
        public void ResetModifier()
        {
            this.modifier = Modifier.None;
        }
        public bool IsModifierResetted() => this.modifier == Modifier.None;

        public bool IsVanillaVisonAndGetVison(
            ShipStatus shipStatus, GameData.PlayerInfo playerInfo, out float vison)
        {
            vison = shipStatus.MaxLightRadius;

            switch (this.modifier)
            {
                case Modifier.LastWolfLightOff:
                    if (ExtremeRoleManager.GetSafeCastedLocalPlayerRole<LastWolf>() == null)
                    {
                        vison = 0.15f;
                        return false;
                    }
                    break;
                case Modifier.WispLightOff:
                    if (!Wisp.HasTorch(playerInfo.PlayerId))
                    {
                        vison = shipStatus.MinLightRadius * crewLightVison;
                        return false;
                    }
                    break;
                default:
                    break;
            }
            bool isRequireCustomVison = requireCustomCustomCalculateLightRadius();

            if (!RoleAssignState.Instance.IsRoleSetUpEnd)
            {
                return checkNormalOrCustomCalculateLightRadius(isRequireCustomVison, playerInfo, ref vison);
            }
            var systems = shipStatus.Systems;
            ISystemType systemType = systems.ContainsKey(electrical) ? systems[electrical] : null;
            if (systemType == null)
            {
                return checkNormalOrCustomCalculateLightRadius(isRequireCustomVison, playerInfo, ref vison);
            }

            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null)
            {
                return checkNormalOrCustomCalculateLightRadius(isRequireCustomVison, playerInfo, ref vison);
            }

            var allRole = ExtremeRoleManager.GameRole;

            if (allRole.Count == 0)
            {
                if (isRequireCustomVison)
                {
                    vison = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                        playerInfo, false, playerInfo.Role.IsImpostor);
                    return false;
                }
                return true;
            }

            SingleRoleBase role = allRole[playerInfo.PlayerId];

            if (isRequireCustomVison)
            {
                float visonMulti;
                bool applayVisonEffects = !role.IsImpostor();

                if (role.TryGetVisonMod(out visonMulti, out bool isApplyEnvironmentVision))
                {
                    applayVisonEffects = isApplyEnvironmentVision;
                }
                else if (playerInfo.Role.IsImpostor)
                {
                    visonMulti = impLightVison;
                }
                else
                {
                    visonMulti = crewLightVison;
                }

                vison = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                    playerInfo, visonMulti, applayVisonEffects);

                return false;
            }

            float num = (float)switchSystem.Value / 255f;
            float switchVisonMulti = Mathf.Lerp(
                shipStatus.MinLightRadius,
                shipStatus.MaxLightRadius, num);

            float baseVison = shipStatus.MaxLightRadius;

            if (playerInfo == null || playerInfo.IsDead) // IsDead
            {
                vison = baseVison;
            }
            else if (role.TryGetVisonMod(out float visonMulti, out bool isApplyEnvironmentVision))
            {
                if (isApplyEnvironmentVision)
                {
                    baseVison = switchVisonMulti;
                }
                vison = baseVison * visonMulti;
            }
            else if (playerInfo.Role.IsImpostor)
            {
                vison = baseVison * impLightVison;
            }
            else
            {
                vison = switchVisonMulti * crewLightVison;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool requireCustomCustomCalculateLightRadius() =>
            ExtremeRolesPlugin.Compat.IsModMap &&
            ExtremeRolesPlugin.Compat.ModMap.IsCustomCalculateLightRadius;

        private static bool checkNormalOrCustomCalculateLightRadius(
            bool isRequireCustomVison, GameData.PlayerInfo player, ref float result)
        {
            if (isRequireCustomVison)
            {
                result = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                    player, false, player.Role.IsImpostor);
                return false;
            }
            return true;
        }
    }
}
