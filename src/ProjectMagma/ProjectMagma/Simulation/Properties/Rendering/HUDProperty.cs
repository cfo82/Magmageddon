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
            Debug.Assert(entity.HasAttribute(CommonNames.Kind) && entity.GetString(CommonNames.Kind) == "player");

            base.OnAttached(entity);

            Entity playerConstants = Game.Instance.Simulation.EntityManager["player_constants"];

            if (entity.HasString(CommonNames.PlayerName))
            {
                entity.GetStringAttribute(CommonNames.PlayerName).ValueChanged += PlayerNameChanged;
            }
            if (entity.HasInt(CommonNames.GamePadIndex))
            {
                entity.GetIntAttribute(CommonNames.GamePadIndex).ValueChanged += GamePadIndexChanged;
            }
            if (entity.HasFloat(CommonNames.Health))
            {
                entity.GetFloatAttribute(CommonNames.Health).ValueChanged += HealthChanged;
            }
            if (playerConstants.HasFloat(CommonNames.MaxHealth))
            {
                playerConstants.GetFloatAttribute(CommonNames.MaxHealth).ValueChanged += MaxHealthChanged;
            }
            if (entity.HasFloat(CommonNames.Energy))
            {
                entity.GetFloatAttribute(CommonNames.Energy).ValueChanged += EnergyChanged;
            }
            if (playerConstants.HasFloat(CommonNames.MaxEnergy))
            {
                playerConstants.GetFloatAttribute(CommonNames.MaxEnergy).ValueChanged += MaxEnergyChanged;
            }
            if (entity.HasInt(CommonNames.Frozen))
            {
                entity.GetIntAttribute(CommonNames.Frozen).ValueChanged += FrozenChanged;
            }
            entity.GetIntAttribute(CommonNames.Lives).ValueChanged += LivesChanged;

            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new AddRenderableUpdate((Renderable)Updatable));
        }

        protected override ProjectMagma.Renderer.Interface.RendererUpdatable CreateUpdatable(Entity entity)
        {
            Entity playerConstants = Game.Instance.Simulation.EntityManager["player_constants"];

            return new HUDRenderable(
                0,
                entity.GetString(CommonNames.PlayerName), entity.GetInt(CommonNames.GamePadIndex),
                entity.GetFloat(CommonNames.Health), playerConstants.GetFloat(CommonNames.MaxHealth),
                entity.GetFloat(CommonNames.Energy), playerConstants.GetFloat(CommonNames.MaxEnergy),
                entity.GetInt(CommonNames.Lives), entity.GetInt(CommonNames.Frozen),
                entity.GetVector3(CommonNames.Color1), entity.GetVector3(CommonNames.Color2)
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

            if (entity.HasString(CommonNames.PlayerName))
            {
                entity.GetStringAttribute(CommonNames.PlayerName).ValueChanged -= PlayerNameChanged;
            }
            if (entity.HasInt(CommonNames.GamePadIndex))
            {
                entity.GetIntAttribute(CommonNames.GamePadIndex).ValueChanged -= GamePadIndexChanged;
            }
            if (entity.HasFloat(CommonNames.Health))
            {
                entity.GetFloatAttribute(CommonNames.Health).ValueChanged -= HealthChanged;
            }
            if (playerConstants != null && playerConstants.HasFloat(CommonNames.MaxHealth))
            {
                playerConstants.GetFloatAttribute(CommonNames.MaxHealth).ValueChanged -= MaxHealthChanged;
            }
            if (entity.HasFloat(CommonNames.Energy))
            {
                entity.GetFloatAttribute(CommonNames.Energy).ValueChanged -= EnergyChanged;
            }
            if (playerConstants != null && playerConstants.HasFloat(CommonNames.MaxEnergy))
            {
                playerConstants.GetFloatAttribute(CommonNames.MaxEnergy).ValueChanged -= MaxEnergyChanged;
            }
            if (entity.HasInt(CommonNames.Frozen))
            {
                entity.GetIntAttribute(CommonNames.Frozen).ValueChanged -= FrozenChanged;
            }
            entity.GetIntAttribute(CommonNames.Lives).ValueChanged -= LivesChanged;

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
            FloatAttribute sender,
            float oldValue,
            float newValue
        )
        {
            ChangeFloat("Health", newValue);
        }

        private void MaxHealthChanged(
            FloatAttribute sender,
            float oldValue,
            float newValue
        )
        {
            ChangeFloat("MaxHealth", newValue);
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
