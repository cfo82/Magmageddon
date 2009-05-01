using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class HUDProperty : RendererUpdatableProperty
    {
        public HUDProperty()
        {
        }

        public override void OnAttached(Entity entity)
        {
            Debug.Assert(entity.HasAttribute("kind") && entity.GetString("kind") == "player");

            base.OnAttached(entity);

            Entity playerConstants = Game.Instance.Simulation.EntityManager["player_constants"];

            if (entity.HasString("player_name"))
            {
                entity.GetStringAttribute("player_name").ValueChanged += PlayerNameChanged;
            }
            if (entity.HasInt("game_pad_index"))
            {
                entity.GetIntAttribute("game_pad_index").ValueChanged += GamePadIndexChanged;
            }
            if (entity.HasInt("health"))
            {
                entity.GetIntAttribute("health").ValueChanged += HealthChanged;
            }
            if (playerConstants.HasInt("max_health"))
            {
                playerConstants.GetIntAttribute("max_health").ValueChanged += MaxHealthChanged;
            }
            if (entity.HasInt("energy"))
            {
                entity.GetIntAttribute("energy").ValueChanged += EnergyChanged;
            }
            if (playerConstants.HasInt("max_energy"))
            {
                playerConstants.GetIntAttribute("max_energy").ValueChanged += MaxEnergyChanged;
            }
            if (entity.HasInt("fuel"))
            {
                entity.GetIntAttribute("fuel").ValueChanged += FuelChanged;
            }
            if (playerConstants.HasInt("max_fuel"))
            {
                playerConstants.GetIntAttribute("max_fuel").ValueChanged += MaxFuelChanged;
            }
            if (entity.HasInt("frozen"))
            {
                entity.GetIntAttribute("frozen").ValueChanged += FrozenChanged;
            }
            if (entity.HasInt("jumps"))
            {
                entity.GetIntAttribute("jumps").ValueChanged += JumpsChanged;
            }

            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new AddRenderableUpdate((Renderable)Updatable));
        }

        protected override ProjectMagma.Renderer.Interface.RendererUpdatable CreateUpdatable(Entity entity)
        {
            Entity playerConstants = Game.Instance.Simulation.EntityManager["player_constants"];

            return new HUDRenderable(
                entity.GetString("player_name"),
                entity.GetInt("game_pad_index"),
                entity.GetInt("health"),
                playerConstants.GetInt("max_health"),
                entity.GetInt("energy"),
                playerConstants.GetInt("max_energy"),
                entity.GetInt("fuel"),
                playerConstants.GetInt("max_fuel"),
                entity.GetInt("frozen"),
                entity.GetInt("jumps"));
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
        }

        public override void OnDetached(Entity entity)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new RemoveRenderableUpdate((Renderable)Updatable));

            Entity playerConstants = Game.Instance.Simulation.EntityManager["player_constants"];

            if (entity.HasString("player_name"))
            {
                entity.GetStringAttribute("player_name").ValueChanged -= PlayerNameChanged;
            }
            if (entity.HasInt("game_pad_index"))
            {
                entity.GetIntAttribute("game_pad_index").ValueChanged -= GamePadIndexChanged;
            }
            if (entity.HasInt("health"))
            {
                entity.GetIntAttribute("health").ValueChanged -= HealthChanged;
            }
            if (playerConstants != null && playerConstants.HasInt("max_health"))
            {
                playerConstants.GetIntAttribute("max_health").ValueChanged -= MaxHealthChanged;
            }
            if (entity.HasInt("energy"))
            {
                entity.GetIntAttribute("energy").ValueChanged -= EnergyChanged;
            }
            if (playerConstants != null && playerConstants.HasInt("max_energy"))
            {
                playerConstants.GetIntAttribute("max_energy").ValueChanged -= MaxEnergyChanged;
            }
            if (entity.HasInt("fuel"))
            {
                entity.GetIntAttribute("fuel").ValueChanged -= FuelChanged;
            }
            if (playerConstants != null && playerConstants.HasInt("max_fuel"))
            {
                playerConstants.GetIntAttribute("max_fuel").ValueChanged -= MaxFuelChanged;
            }
            if (entity.HasInt("frozen"))
            {
                entity.GetIntAttribute("frozen").ValueChanged -= FrozenChanged;
            }
            if (entity.HasInt("jumps"))
            {
                entity.GetIntAttribute("jumps").ValueChanged -= JumpsChanged;
            }

            base.OnDetached(entity);
        }

        private void PlayerNameChanged(
            StringAttribute sender,
            string oldValue,
            string newValue
        )
        {
            ChangeString("PlayerName", newValue);
        }

        private void GamePadIndexChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("GamePadIndex", newValue);
        }

        private void HealthChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("Health", newValue);
        }

        private void MaxHealthChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("MaxHealth", newValue);
        }

        private void EnergyChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("Energy", newValue);
        }

        private void MaxEnergyChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("MaxEnergy", newValue);
        }

        private void FuelChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("Fuel", newValue);
        }

        private void MaxFuelChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("MaxFuel", newValue);
        }

        private void FrozenChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("Frozen", newValue);
        }

        private void JumpsChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("Jumps", newValue);
        }

        private HUDRenderable renderable;
    }
}
