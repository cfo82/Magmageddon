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
            player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) + (float)powerup.GetInt("value"));
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.PowerupEnergyTaken);
        }

        protected override string NotificationString
        {
            get { return "ENERGY REFILL"; }
        }

    }
}
