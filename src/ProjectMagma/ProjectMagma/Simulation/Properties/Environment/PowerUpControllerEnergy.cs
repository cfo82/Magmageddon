using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public class PowerUpControllerEnergy : PowerUpControllerBase
    {

        protected override void GivePower(Entity player)
        {
            player.SetFloat("energy", player.GetFloat("energy") + (float)powerup.GetInt("value"));
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.PowerupEnergyTaken);
        }

        protected override string NotificationString
        {
            get { return "ENERGY REFILL"; }
        }

    }
}
