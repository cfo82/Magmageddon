using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class HUDProperty : RendererUpdatableProperty
    {
        public HUDProperty()
        {
        }

        public override void OnAttached(AbstractEntity entity)
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
            if (entity.HasFloat("energy"))
            {
                entity.GetFloatAttribute("energy").ValueChanged += EnergyChanged;
            }
            if (playerConstants.HasFloat("max_energy"))
            {
                playerConstants.GetFloatAttribute("max_energy").ValueChanged += MaxEnergyChanged;
            }
            if (entity.HasInt("frozen"))
            {
                entity.GetIntAttribute("frozen").ValueChanged += FrozenChanged;
            }
            entity.GetIntAttribute("lives").ValueChanged += LivesChanged;

            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new AddRenderableUpdate((Renderable)Updatable));
        }

        protected override ProjectMagma.Renderer.Interface.RendererUpdatable CreateUpdatable(Entity entity)
        {
            Entity playerConstants = Game.Instance.Simulation.EntityManager["player_constants"];

            return new HUDRenderable(
                0,
                entity.GetString("player_name"), entity.GetInt("game_pad_index"),
                entity.GetInt("health"), playerConstants.GetInt("max_health"),
                entity.GetFloat("energy"), playerConstants.GetFloat("max_energy"),
                entity.GetInt("lives"), entity.GetInt("frozen"),
                entity.GetVector3("color1"), entity.GetVector3("color2")
            );
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
        }

        public void NotifyPowerupPickup(Vector3 worldPosition, string notification)
        {
            ChangeVector3("NextPowerupPickupPosition", worldPosition);
            ChangeString("NextPowerupNotification", notification);
        }

        public override void OnDetached(AbstractEntity entity)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new RemoveRenderableUpdate((Renderable)Updatable));

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
            if (entity.HasFloat("energy"))
            {
                entity.GetFloatAttribute("energy").ValueChanged -= EnergyChanged;
            }
            if (playerConstants != null && playerConstants.HasFloat("max_energy"))
            {
                playerConstants.GetFloatAttribute("max_energy").ValueChanged -= MaxEnergyChanged;
            }
            if (entity.HasInt("frozen"))
            {
                entity.GetIntAttribute("frozen").ValueChanged -= FrozenChanged;
            }
            entity.GetIntAttribute("lives").ValueChanged -= LivesChanged;

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
            FloatAttribute sender,
            float oldValue,
            float newValue
        )
        {
            ChangeFloat("Energy", newValue);
        }

        private void MaxEnergyChanged(
            FloatAttribute sender,
            float oldValue,
            float newValue
        )
        {
            ChangeFloat("MaxEnergy", newValue);
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

        private void LivesChanged(
            IntAttribute sender,
            int oldValue,
            int newValue
        )
        {
            ChangeInt("Lives", newValue);
        }

        public bool RepulsionUsable
        {
            set
            {
                ChangeBool("RepulsionUsable", value);
            }
        }

        public bool JetpackUsable
        {
            set
            {
                ChangeBool("JetpackUsable", value);
            }
        }

    }
}
