using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Shared.Math.Primitives;
using System.Diagnostics;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma.Simulation
{
    public abstract class PlayerBaseProperty : RobotBaseProperty
    {

        public PlayerBaseProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            base.OnAttached(player);
        }

        public override void OnDetached(AbstractEntity player)
        {
            ResetVibration();
            base.OnDetached(player);
        }

        protected override void OnUpdate(Entity player, SimulationTime simTime)
        {
            // stop vibration
            if (simTime.At > player.GetFloat("vibrateStartetAt") + 330) // todo: extract constant
            {
                ResetVibration();
            }

            OnPlaying(player, simTime);
            CheckPlayerAttributeRanges(player);
        }

        protected abstract void OnPlaying(Entity player, SimulationTime simTime);

        public void CheckPlayerAttributeRanges(Entity player)
        {
            float health = player.GetFloat(CommonNames.Health);
            if (health < 0)
            {
                player.SetFloat(CommonNames.Health, 0);
            }
            else
            {
                if (health > constants.GetFloat(CommonNames.MaxHealth))
                {
                    player.SetFloat(CommonNames.Health, constants.GetFloat(CommonNames.MaxHealth));
                }
            }

            float energy = player.GetFloat(CommonNames.Energy);
            if (energy < 0)
                player.SetFloat(CommonNames.Energy, 0);
            else
                if (energy > constants.GetFloat(CommonNames.MaxEnergy))
                    player.SetFloat(CommonNames.Energy, constants.GetFloat(CommonNames.MaxEnergy));
        }

        protected void Vibrate(float left, float right)
        {
            GamePad.SetVibration(playerIndex, left, right);
        }

        protected void ResetVibration()
        {
            GamePad.SetVibration(playerIndex, 0, 0);
        }


    }
}

