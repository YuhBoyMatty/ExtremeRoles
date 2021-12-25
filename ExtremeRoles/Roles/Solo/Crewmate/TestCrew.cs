﻿using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class TestCrew : SingleRoleBase
    {
        public TestCrew(
        ): base(
            ExtremeRoleId.NormalCrew,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.NormalCrew.ToString(),
            new Color(200f / 255f, 200f / 255f, 0, 1f),
            true, true, true, false)
        {}

        protected override void CreateSpecificOption(CustomOption parentOps)
        {
            return;
        }
        
        protected override void RoleSpecificInit()
        {
            return;
        }

    }
}
