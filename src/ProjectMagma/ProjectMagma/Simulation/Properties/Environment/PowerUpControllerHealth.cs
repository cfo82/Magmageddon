﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public class PowerUpControllerHealth : PowerUpControllerBase
    {

        protected override void GivePower(Entity player)
        {
            player.SetFloat(CommonNames.Health, player.GetFloat(CommonNames.Health) + (float)powerup.GetInt("value"));
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.PowerupHealthTaken);
        }


        protected override string NotificationString
        {
            get { return "HEALTH REFILL"; }
        }

    }
}
