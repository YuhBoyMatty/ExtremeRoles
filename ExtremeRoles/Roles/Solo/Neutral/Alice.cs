﻿using System.Collections.Generic;
using System.Linq;

using Hazel;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Alice : SingleRoleBase, IRoleAbility
    {

        public enum AliceOption
        {
            RevartCommonTaskNum,
            RevartLongTaskNum,
            RevartNormalTaskNum,
        }

        public RoleAbilityButton Button
        { 
            get => this.aliceShipBroken;
            set
            {
                this.aliceShipBroken = value;
            }
        }

        public int RevartLongTask = 0;
        public int RevartNormalTask = 0;
        public int RevartCommonTask = 0;

        private RoleAbilityButton aliceShipBroken;

        public Alice(): base(
            ExtremeRoleId.Alice,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Alice.ToString(),
            ColorPalette.AliceGold,
            true, false, true, true)
        {}

        public void CreateAbility()
        {
            this.CreateAbilityButton(
                Helper.Translation.GetString("shipBroken"),
                Helper.Resources.LoadSpriteFromResources(
                    Resources.ResourcesPaths.AliceShipBroken, 115f),
                abilityNum:10);
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(multiAssignRole.AnotherRole);
                }
            }
            if (OptionsHolder.Ship.IsSameNeutralSameWin)
            {
                return this.Id == targetRole.Id;
            }
            else
            {
                return (this.Id == targetRole.Id) && this.IsSameControlId(targetRole);
            }
        }

        public bool IsAbilityUse()
        {
            return this.IsCommonUse();
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
           if (ExtremeRoleManager.GameRole[killerPlayer.PlayerId].IsImposter())
           {
                this.IsWin = true;
           }
        }

        public bool UseAbility()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.SetNormalRole,
                Hazel.SendOption.Reliable, -1);

            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.AliceAbility(
                PlayerControl.LocalPlayer.PlayerId);

            return true;
        }

        public static void ShipBroken(byte callerId)
        {
            var alice = (Alice)ExtremeRoleManager.GameRole[callerId];
            var player = PlayerControl.LocalPlayer;

            List<byte> addTaskId = new List<byte> ();
            
            for (int i = 0; i < alice.RevartLongTask; ++i)
            {
                addTaskId.Add(Helper.Task.GetRandomLongTask());
            }
            for (int i = 0; i < alice.RevartCommonTask; ++i)
            {
                addTaskId.Add(Helper.Task.GetRandomCommonTaskId());
            }
            for (int i = 0; i < alice.RevartNormalTask; ++i)
            {
                addTaskId.Add(Helper.Task.GetRandomNormalTaskId());
            }

            var shuffled = addTaskId.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();
            
            for (int i = 0; i < player.myTasks.Count; ++i)
            {
                if (shuffled.Count == 0) { break; }

                bool isTaskComp = player.myTasks[i].IsComplete;
                if (isTaskComp)
                {
                    byte taskId = shuffled[0];
                    shuffled.RemoveAt(0);

                    NormalPlayerTask normalPlayerTask = 
                        UnityEngine.Object.Instantiate<NormalPlayerTask>(
                            ShipStatus.Instance.GetTaskById(taskId),
                            player.transform);
                    normalPlayerTask.Id = taskId;
                    normalPlayerTask.Owner = player;
                    normalPlayerTask.Initialize();

                    player.myTasks[i] = normalPlayerTask;
                }
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateRoleAbilityOption(
                parentOps, maxAbilityCount:100);

            CustomOption.Create(
                this.GetRoleOptionId((int)AliceOption.RevartLongTaskNum),
                Helper.Design.ConcatString(
                    this.RoleName,
                    AliceOption.RevartLongTaskNum.ToString()),
                1, 0, 15, 1, parentOps);
            CustomOption.Create(
                this.GetRoleOptionId((int)AliceOption.RevartCommonTaskNum),
                Helper.Design.ConcatString(
                    this.RoleName,
                    AliceOption.RevartCommonTaskNum.ToString()),
                1, 0, 15, 1, parentOps);
            CustomOption.Create(
                this.GetRoleOptionId((int)AliceOption.RevartNormalTaskNum),
                Helper.Design.ConcatString(
                    this.RoleName,
                    AliceOption.RevartNormalTaskNum.ToString()),
                1, 0, 15, 1, parentOps);

        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionsHolder.AllOption;
            this.RevartNormalTask = allOption[
                GetRoleOptionId((int)AliceOption.RevartNormalTaskNum)].GetValue();
            this.RevartLongTask = allOption[
                GetRoleOptionId((int)AliceOption.RevartLongTaskNum)].GetValue();
            this.RevartCommonTask = allOption[
                GetRoleOptionId((int)AliceOption.RevartCommonTaskNum)].GetValue();

            this.RoleAbilityInit();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }
    }
}
