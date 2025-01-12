﻿using System;
using UnityEngine;

namespace ExtremeRoles.Module.AbilityButton.Refacted.Roles
{
    public sealed class ReusableAbilityButton : RoleAbilityButtonBase
    {
        public ReusableAbilityButton(
            string buttonText,
            Func<bool> ability,
            Func<bool> canUse,
            Sprite sprite,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F) : base(
                buttonText,
                ability,
                canUse,
                sprite,
                abilityCleanUp,
                abilityCheck,
                hotkey)
        { }

        protected override void UpdateAbility()
        {
            return;
        }

        protected override bool IsEnable() => this.CanUse.Invoke();

        protected override void DoClick()
        {
            if(this.IsEnable() &&
               this.Timer <= 0f &&
               this.State == AbilityState.Ready &&
               this.UseAbility.Invoke())
            {
                this.SetStatus(
                    this.HasCleanUp() ?
                    AbilityState.Activating :
                    AbilityState.CoolDown);
            }
        }
    }
}
