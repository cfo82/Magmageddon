using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation.Collision;

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
                Game.Instance.Simulation.ApplyPerSecondSubstraction(entity, "flamethrower_burn", constants.GetInt("flamethrower_damage_per_second"),
                    entity.GetIntAttribute("health"));
                if(entity.GetString("kind") == "player")
                {
                    entity.SetInt("frozen", 0);
                }
            }
        }

        public void OnAttached(Entity entity)
        {
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];

            if (!entity.HasAttribute("burn_time"))
            {
                // todo: make constant
                entity.AddIntAttribute("burn_time", 250);
            }
            entity.AddFloatAttribute("burnt_at", -entity.GetInt("burn_time"));

            entity.Update += OnUpdate;
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
        }

    }
}
