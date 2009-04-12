using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class HUDProperty : Property
    {
        public HUDProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            Debug.Assert(entity.HasAttribute("kind") && entity.GetString("kind") == "player");

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

            renderable = new HUDRenderable(
                entity.Name,
                entity.GetString("player_name"),
                entity.GetInt("game_pad_index"),
                entity.GetInt("health"),
                playerConstants.GetInt("max_health"),
                entity.GetInt("energy"),
                playerConstants.GetInt("max_energy"),
                entity.GetInt("fuel"),
                playerConstants.GetInt("max_fuel"));

            Game.Instance.Renderer.AddRenderable(renderable);
        }

        public void OnDetached(Entity entity)
        {
            Game.Instance.Renderer.RemoveRenderable(renderable);

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
        }

        private void PlayerNameChanged(
            StringAttribute sender,
            string oldValue,
            string newValue
        )
        {
            renderable.PlayerName = newValue;
        }

        private void GamePadIndexChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            renderable.GamePadIndex = newValue;
        }

        private void HealthChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            renderable.Health = newValue;
        }

        private void MaxHealthChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            renderable.MaxHealth = newValue;
        }

        private void EnergyChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            renderable.Energy = newValue;
        }

        private void MaxEnergyChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            renderable.MaxEnergy = newValue;
        }

        private void FuelChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            renderable.Fuel = newValue;
        }

        private void MaxFuelChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            renderable.MaxFuel = newValue;
        }

        private HUDRenderable renderable;
    }
}
