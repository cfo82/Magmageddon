using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public class PowerUpController : Property
    {

        Random rand;

        public PowerUpController()
        {
            rand = new Random(234234);
        }

        private void OnUpdate(Entity powerupEntity, GameTime gameTime)
        {
            float respawnAt = powerupEntity.GetFloat("respawn_at");
            if (respawnAt != 0
                && gameTime.TotalGameTime.TotalMilliseconds > respawnAt)
            {
                int islandNo = rand.Next(Game.Instance.Simulation.IslandManager.Count - 1);
                islandEntity = Game.Instance.Simulation.IslandManager[islandNo];

                powerupEntity.AddProperty("collision", new CollisionProperty());
                powerupEntity.AddProperty("render", new RenderProperty());
                powerupEntity.AddProperty("shadow_cast", new ShadowCastProperty());

                powerupEntity.SetFloat("respawn_at", 0);
            }
        }

        public void OnAttached(
            Entity entity
        )
        {
            this.powerupEntity = entity;
            this.islandEntity = Game.Instance.Simulation.EntityManager[entity.GetString("island_reference")];

            // initialize properties
            Debug.Assert(this.powerupEntity.HasVector3("relative_position"), "must have a relative translation attribute");
            if (!this.powerupEntity.HasAttribute("position"))
            {
                this.powerupEntity.AddVector3Attribute("position", Vector3.Zero);
            }

            // initialize position on island
            Debug.Assert(islandEntity.HasVector3("position"), "the island must have a position attribute.");
            this.powerupEntity.SetVector3("position", islandEntity.GetVector3("position"));

            // add timeout respawn attribute
            this.powerupEntity.AddFloatAttribute("respawn_at", 0);

            // register change handler
            this.islandEntity.GetVector3Attribute("position").ValueChanged += OnIslandPositionChanged;

            powerupEntity.Update += OnUpdate;
        }

        public void OnDetached(
            Entity entity
        )
        {
            powerupEntity.Update -= OnUpdate;
            this.islandEntity.GetVector3Attribute("position").ValueChanged -= OnIslandPositionChanged;
        }

        private void OnIslandPositionChanged(
            Vector3Attribute positionAttribute,
            Vector3 oldPosition,
            Vector3 newPosition
        )
        {
            this.powerupEntity.SetVector3("position", newPosition + this.powerupEntity.GetVector3("relative_position"));
        }

        private Entity powerupEntity;
        private Entity islandEntity;
    }
}
