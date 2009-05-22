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
            player.SetInt("energy", player.GetInt("energy") + powerup.GetInt("value"));
        }

        protected override string NotificationString
        {
            get { return "ENERGY REFILL"; }
        }

    }
}
