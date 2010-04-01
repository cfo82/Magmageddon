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
    public abstract class PlayerBaseProperty : Property
    {
        private double respawnStartedAt = 0;

        internal Entity player;
        internal Entity constants;
        internal LevelData templates;
        internal ControllerInput controllerInput;
        internal PlayerIndex playerIndex;

        public PlayerBaseProperty()
        {
        }

        public virtual void OnAttached(AbstractEntity player)
        {
            (player as Entity).Update += OnUpdate;

            this.player = player as Entity;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
            this.templates = Game.Instance.ContentManager.Load<LevelData>("Level/Common/DynamicTemplates");
            this.playerIndex = (PlayerIndex)player.GetInt("game_pad_index");
            this.controllerInput = player.GetProperty<InputProperty>("input").ControllerInput;
        }

        public virtual void OnDetached(AbstractEntity player)
        {
            ResetVibration();

            (player as Entity).Update -= OnUpdate;
        }

        private void OnUpdate(Entity player, SimulationTime simTime)
        {
            // stop vibration
            if (simTime.At > player.GetFloat("vibrateStartetAt") + 330) // todo: extract constant
            {
                ResetVibration();
            }

            if (Game.Instance.Simulation.Phase == SimulationPhase.Intro
                || player.GetBool("isRespawning"))
            {
                OnRespawn(player, simTime);
            }
            else
            {
                OnPlaying(player, simTime);
            }

            CheckPlayerAttributeRanges(player);
        }

        protected abstract void OnRespawn(Entity player, SimulationTime simTime);
        protected abstract void OnPlaying(Entity player, SimulationTime simTime);

        public void CheckPlayerAttributeRanges(Entity player)
        {
            if (player.GetBool("isRespawning"))
            {
                // we cannot take damage on respawn
                player.SetFloat("health", constants.GetFloat("max_health"));
                player.SetFloat("energy", constants.GetFloat("max_energy"));
                player.SetInt("frozen", 0);
                return;
            }

            float health = player.GetFloat("health");
            if (health < 0)
            {
                player.SetFloat("health", 0);
            }
            else
            {
                if (health > constants.GetFloat("max_health"))
                {
                    player.SetFloat("health", constants.GetFloat("max_health"));
                }
            }

            float energy = player.GetFloat("energy");
            if (energy < 0)
                player.SetFloat("energy", 0);
            else
                if (energy > constants.GetFloat("max_energy"))
                    player.SetFloat("energy", constants.GetFloat("max_energy"));
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

