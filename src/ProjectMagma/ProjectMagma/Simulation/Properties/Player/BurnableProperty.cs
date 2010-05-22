using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;

namespace ProjectMagma.Simulation
{
    public class BurnableProperty : Property
    {
        Entity constants;

        public BurnableProperty()
        {
        }

        private void OnUpdate(Entity entity, SimulationTime simTime)
        {
            float burntAt = entity.GetFloat("burnt_at");
            if (simTime.At < burntAt + entity.GetInt("burn_time"))
            {
                entity.SetFloat(CommonNames.Health, entity.GetFloat(CommonNames.Health) - simTime.Dt * constants.GetFloat("flamethrower_damage_per_second"));
                if (entity.GetString(CommonNames.Kind) == "player")
                {
                    entity.SetInt(CommonNames.Frozen, 0);
                    entity.GetProperty<PlayerControllerProperty>("controller").CheckPlayerAttributeRanges(entity);
                }
            }
        }

        public void OnAttached(AbstractEntity entity)
        {
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];

            if (!entity.HasAttribute("burn_time"))
            {
                entity.AddIntAttribute("burn_time", constants.GetInt("flamethrower_after_burn_time"));
            }
            entity.AddFloatAttribute("burnt_at", -entity.GetInt("burn_time"));

            (entity as Entity).Update += OnUpdate;
        }

        public void OnDetached(AbstractEntity entity)
        {
            (entity as Entity).Update -= OnUpdate;
        }

    }
}
