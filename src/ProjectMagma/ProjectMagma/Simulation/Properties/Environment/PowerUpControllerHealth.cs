﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public class PowerUpControllerHealth : PowerUpControllerBase
    {

        protected override void GivePower(Entity player)
        {
            player.SetInt("health", player.GetInt("health") + powerup.GetInt("value"));
        }


        protected override string NotificationString
        {
            get { return "HEALTH REFILL"; }
        }

    }
}
