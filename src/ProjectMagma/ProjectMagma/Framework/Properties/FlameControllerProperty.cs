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

        private float scaleFactor = 1;
        private Vector3 fullScale;

        public FlameControllerProperty()
        {
        }

        public void OnAttached(Entity flame)
        {
            this.constants = Game.Instance.EntityManager["player_constants"];
            this.flame = flame;
            this.player = Game.Instance.EntityManager[flame.GetString("player")];

            player.GetVector3Attribute("position").ValueChanged += new Vector3ChangeHandler(playerPositionHandler);
            player.GetQuaternionAttribute("rotation").ValueChanged += new QuaternionChangeEventHandler(playerRotationHandler);

            flame.GetBoolAttribute("active").ValueChanged += new BoolChangeHandler(flameActivationHandler);

            flame.Update += OnUpdate;
        }

        public void OnDetached(Entity flame)
        {
            flame.Update -= OnUpdate;
        }

        private void OnUpdate(Entity flame, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            if (flame.GetBool("active"))
            {
                if (scaleFactor < 1)
                    scaleFactor += constants.GetFloat("flamethrower_turn_flame_scale_deduction");
                flame.SetVector3("scale", fullScale * scaleFactor);
            }
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

            if (flame.GetBool("active"))
            {
                if (scaleFactor > constants.GetFloat("flamethrower_turn_min_scale"))
                    scaleFactor -= constants.GetFloat("flamethrower_turn_flame_scale_deduction");
            }
        }

        private void flameActivationHandler(BoolAttribute sender, bool oldValue, bool newValue)
        {
            this.fullScale = flame.GetVector3("scale");
        }

    }
}
