using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Framework
{
    public class FlameControllerProperty : Property
    {
        private Entity constants;
        private Entity player;
        private Entity flame;

        public FlameControllerProperty()
        {
        }

        public void OnAttached(Entity flame)
        {
            this.constants = Game.Instance.EntityManager["island_constants"];
            this.flame = flame;
            this.player = Game.Instance.EntityManager[flame.GetString("player")];

            ((Vector3Attribute)player.Attributes["position"]).ValueChanged += new Vector3ChangeHandler(playerPositionHandler);
            ((QuaternionAttribute)player.Attributes["rotation"]).ValueChanged += new QuaternionChangeEventHandler(playerRotationHandler);

            flame.Update += OnUpdate;
        }

        public void OnDetached(Entity flame)
        {
            flame.Update -= OnUpdate;
        }

        private void OnUpdate(Entity flame, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;


           
        }

        private void playerPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = flame.GetVector3("position");
            Vector3 delta = newValue - oldValue;
            position += delta;
            flame.SetVector3("position", position);
        }

        private void playerRotationHandler(QuaternionAttribute sender, Quaternion oldValue, Quaternion newValue)
        {
            flame.SetQuaternion("rotation", newValue);
        }


    }
}
