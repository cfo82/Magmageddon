using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public class PowerUpControllerLives : PowerUpControllerBase
    {

        protected override void GivePower(Entity player)
        {
            player.SetInt("lives", player.GetInt("lives") + powerup.GetInt("value"));
        }

        protected override string NotificationString
        {
            get { return "EXTRA LIVES"; }
        }

    }
}
