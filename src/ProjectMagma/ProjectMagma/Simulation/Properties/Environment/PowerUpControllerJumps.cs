﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public class PowerUpControllerJumps : PowerUpControllerBase
    {

        protected override void GivePower(Entity player)
        {
            player.SetInt("jumps", player.GetInt("jumps") + powerup.GetInt("value"));
            player.SetFloat("repulsion_seconds", 0);
        }


        protected override string NotificationString
        {
            get { return "FAR JUMPS"; }
        }

    }
}
