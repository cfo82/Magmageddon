using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public class PowerUpControllerRepulsion : PowerUpControllerBase
    {

        protected override void GivePower(Entity player)
        {
            player.SetFloat("repulsion_seconds", player.GetFloat("repulsion_seconds") + powerup.GetFloat("value"));
            player.SetInt("jumps", 0);
        }

        protected override string NotificationString
        {
            get { return "REPULSION"; }
        }

    }
}
