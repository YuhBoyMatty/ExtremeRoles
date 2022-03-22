﻿using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Yandere : SingleRoleBase, IRoleUpdate, IRoleMurderPlayerHock, IRoleResetMeeting, IRoleSpecialSetUp
    {
        private PlayerControl oneSidedLover = null;

        private bool hasOneSidedArrow = false;
        private Arrow oneSidedArrow = null;

        private int targetKillReduceRate = 0;
        private float noneTargetKillMultiplier = 0;
        private float defaultKillCool;
        private float prevKillCool;

        private bool isRunawayNextMeetingEnd;
        private bool isRunaway;

        private float timeLimit = 0f;
        private float timer = 0f;

        private float setTargetRange;

        private KillTarget target;

        public class KillTarget
        {
            private bool isUseArrow;

            private Dictionary<byte, Arrow> targetArrow = new Dictionary<byte, Arrow>();
            private Dictionary<byte, PlayerControl> targetPlayer = new Dictionary<byte, PlayerControl>();

            public KillTarget(
                bool isUseArrow)
            {
                this.isUseArrow = isUseArrow;
                this.targetArrow.Clear();
                this.targetPlayer.Clear();
            }

            public void Add(byte playerId)
            {
                Add(Helper.Player.GetPlayerControlById(playerId));
            }
            public void Add(PlayerControl player)
            {

                this.targetPlayer.Add(player.PlayerId, player);
                if (this.isUseArrow)
                {
                    this.targetArrow.Add(
                        player.PlayerId, new Arrow(
                            new Color32(
                                byte.MaxValue,
                                byte.MaxValue,
                                byte.MaxValue,
                                byte.MinValue)));
                }
                else
                {
                    this.targetArrow.Add(
                        player.PlayerId, null);
                }
            }

            public bool IsContain(byte playerId) => targetPlayer.ContainsKey(playerId);

            public int Count() => this.targetPlayer.Count;

            public void Remove(byte playerId)
            {
                this.targetPlayer.Remove(playerId);
                this.targetArrow.Remove(playerId);
            }

            public void Update()
            {
                List<byte> remove = new List<byte>();

                foreach (var (playerId, playerControl) in this.targetPlayer)
                {
                    if (playerControl.Data.Disconnected || playerControl.Data.IsDead)
                    {
                        remove.Add(playerId);
                    }

                    if (this.targetArrow[playerId] != null && this.isUseArrow)
                    {
                        this.targetArrow[playerId].UpdateTarget(
                            playerControl.GetTruePosition());
                    }
                }

                foreach (var playerId in remove)
                {
                    this.Remove(playerId);
                }
            }
        }

        public enum YandereOption
        {
            TargetKilledKillCoolReduceRate,
            NoneTargetKilledKillCoolMultiplier,
            SetTargetRange,
            RunawayTime,
            HasOneSidedArrow,
            HasTargetArrow,
        }

        public Yandere(): base(
            ExtremeRoleId.Yandere,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Yandere.ToString(),
            ColorPalette.YandereVioletRed,
            false, false, true, false)
        { }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            if (this.target.IsContain(targetPlayer.PlayerId))
            {
                this.KillCoolTime = this.KillCoolTime * (
                    (100f - this.targetKillReduceRate) / 100f);
            }
            else
            {
                this.KillCoolTime = this.KillCoolTime * this.noneTargetKillMultiplier;
            }

            return true;
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
            if (OptionHolder.Ship.IsSameNeutralSameWin)
            {
                return this.Id == targetRole.Id;
            }
            else
            {
                return (this.Id == targetRole.Id) && this.IsSameControlId(targetRole);
            }
        }

        public override string GetIntroDescription() => string.Format(
            base.GetIntroDescription(),
            this.oneSidedLover.Data.PlayerName);


        public void Update(PlayerControl rolePlayer)
        {

            var playerInfo = GameData.Instance.GetPlayerById(
               rolePlayer.PlayerId);
            if (playerInfo.IsDead || playerInfo.Disconnected) { return; }

            Vector2 oneSideLoverPos = this.oneSidedLover.GetTruePosition();

            // 片思いびとが生きてる時の処理
            if (!this.oneSidedLover.Data.Disconnected && !this.oneSidedLover.Data.IsDead)
            {
                searchTarget(rolePlayer, oneSideLoverPos);
            }
            else
            {
                this.isRunaway = true;
            }
            
            updateOneSideLoverArrow(oneSideLoverPos);
            this.target.Update();

            updateCanKill();

            checkRunawayNextMeeting();
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            if (this.target.IsContain(target.PlayerId))
            {
                this.target.Remove(target.PlayerId);
            }
        }

        public void IntroBeginSetUp()
        {

            int playerIndex;

            do
            {
                playerIndex = UnityEngine.Random.RandomRange(
                    0, PlayerControl.AllPlayerControls.Count - 1);

                this.oneSidedLover = PlayerControl.AllPlayerControls[playerIndex];

                var role = ExtremeRoleManager.GameRole[this.oneSidedLover.PlayerId];
                if (role.Id != ExtremeRoleId.Yandere)
                {
                    var multiAssignRole = role as MultiAssignRoleBase;
                    if (multiAssignRole != null)
                    {
                        if (multiAssignRole.AnotherRole != null)
                        {

                            if (multiAssignRole.AnotherRole.Id != ExtremeRoleId.Yandere)
                            {
                                break;
                            }
                        }
                    }
                }

            } while (true);

        }

        public void IntroEndSetUp()
        {
            return;
        }

        public void ResetOnMeetingEnd()
        {
            if(this.isRunawayNextMeetingEnd)
            {
                this.CanKill = true;
                this.isRunaway = true;
                this.isRunawayNextMeetingEnd = false;
            }

        }

        public void ResetOnMeetingStart()
        {
            this.KillCoolTime = this.defaultKillCool;
            this.CanKill = false;
            this.isRunaway = false;
            this.timer = 0f;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.TargetKilledKillCoolReduceRate),
                string.Concat(
                    this.RoleName,
                    YandereOption.TargetKilledKillCoolReduceRate.ToString()),
                85, 50, 99, 0.5f,
                parentOps, format: OptionUnit.Percentage);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.NoneTargetKilledKillCoolMultiplier),
                string.Concat(
                    this.RoleName,
                    YandereOption.NoneTargetKilledKillCoolMultiplier.ToString()),
                1.2f, 1.0f, 2.0f, 0.1f,
                parentOps, format: OptionUnit.Multiplier);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.SetTargetRange),
                string.Concat(
                    this.RoleName,
                    YandereOption.SetTargetRange.ToString()),
                1.8f, 0.5f, 5.0f, 0.1f,
                parentOps, format: OptionUnit.Second);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.RunawayTime),
                string.Concat(
                    this.RoleName,
                    YandereOption.RunawayTime.ToString()),
                60.0f, 25.0f, 120.0f, 0.25f,
                parentOps, format: OptionUnit.Second);

            CustomOption.Create(
                 GetRoleOptionId((int)YandereOption.HasOneSidedArrow),
                 string.Concat(
                     this.RoleName,
                     YandereOption.HasOneSidedArrow.ToString()),
                 true, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.HasTargetArrow),
                 string.Concat(
                     this.RoleName,
                     YandereOption.HasTargetArrow.ToString()),
                 true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;


            this.setTargetRange = allOption[
                GetRoleOptionId((int)YandereOption.SetTargetRange)].GetValue();

            this.targetKillReduceRate = allOption[
                GetRoleOptionId((int)YandereOption.TargetKilledKillCoolReduceRate)].GetValue();
            this.noneTargetKillMultiplier = allOption[
                GetRoleOptionId((int)YandereOption.NoneTargetKilledKillCoolMultiplier)].GetValue();

            this.timeLimit = allOption[
                GetRoleOptionId((int)YandereOption.RunawayTime)].GetValue();

            this.hasOneSidedArrow = allOption[
                GetRoleOptionId((int)YandereOption.HasOneSidedArrow)].GetValue();
            this.target = new KillTarget(
                allOption[GetRoleOptionId((int)YandereOption.ta)].GetValue());
        }

        private void checkRunawayNextMeeting()
        {
            if (this.isRunaway || this.isRunawayNextMeetingEnd) { return; }

            if (this.target.Count() == 0)
            {
                this.timer += Time.fixedDeltaTime;
                if (this.timer >= this.timeLimit)
                {
                    this.isRunawayNextMeetingEnd = true;
                }
            }
            else
            {
                this.timer = 0.0f;
            }
        }

        private void searchTarget(
            PlayerControl rolePlayer,
            Vector2 pos)
        {
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];

                if (!playerInfo.Disconnected &&
                    !ExtremeRoleManager.GameRole[playerInfo.PlayerId].IsImpostor() &&
                    !playerInfo.IsDead && 
                    rolePlayer.PlayerId != playerInfo.PlayerId &&
                    !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - pos;
                        float magnitude = vector.magnitude;
                        if (magnitude <= this.setTargetRange &&
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                pos, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            // 邪魔者追加処理
                        }
                    }
                }
            }
        }

        private void updateCanKill()
        {
            if (this.isRunaway) { return; }

            this.CanKill = this.target.Count() != 0;
        }

        private void updateOneSideLoverArrow(Vector2 pos)
        {

            if (!this.hasOneSidedArrow) { return; }

            if (this.oneSidedArrow != null)
            {
                this.oneSidedArrow = new Arrow(
                    this.NameColor);
            }
            this.oneSidedArrow.UpdateTarget(pos);
        }
    }
}
