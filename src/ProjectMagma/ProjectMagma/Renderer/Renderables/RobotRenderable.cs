using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;
using System.Collections.Generic;

namespace ProjectMagma.Renderer
{
    public class RobotRenderable : TexturedRenderable
    {
        public RobotRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model, 
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture,
            Vector3 color1, Vector3 color2
        )
            : base(scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture)
        {
            this.color1 = color1;
            this.color2 = color2;

            RenderChannel = RenderChannelType.One;
        }

        protected override void ApplyEffectsToModel()
        {
            base.ApplyEffectsToModel();
            
            // create animation component. however, it will register itself in the main 
            // game class as a drawable component. we do not like that and remove it again.
            animator = new ModelAnimator(Game.Instance, Model);
            Game.Instance.Components.RemoveAt(Game.Instance.Components.Count - 1);

            // create all the individual animation controllers as defined in the xml file
            // accompanying the player model
            //idle = new AnimationController(Game.Instance, animator.Animations["idle0"]);
            //walk = new AnimationController(Game.Instance, animator.Animations["walk"]);
            //nod = new AnimationController(Game.Instance, animator.Animations["nodHead"]);

            controllers = new Dictionary<string, AnimationController>();
            foreach(string key in animator.Animations.Keys)
            {
                controllers.Add(key, new AnimationController(Game.Instance, animator.Animations[key]));
            }

            // set the first default controller
            RunController("idle0");
        }

        private Dictionary<string, AnimationController> controllers;

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer, GameTime gameTime)
        {
            base.ApplyCustomEffectParameters(effect, renderer, gameTime);

            effect.Parameters["ToneColor"].SetValue(color1);
        }

        public override void Update(Renderer renderer, GameTime gameTime)
        {
            base.Update(renderer, gameTime);

            animator.Update(gameTime);

            BonePose head = animator.BonePoses["head"];
            head.CurrentController = controllers["nodHead"];
            head.CurrentBlendController = null;
        }

        // activatepermanentstate: idle, walk, jump, die
        // activatesinglestate: attack

        public override bool NeedsUpdate
        {
            get { return true; }
        }

        protected override void DrawMesh(Renderer renderer, GameTime gameTime, ModelMesh mesh)
        {
            animator.World = World;
            animator.Draw(gameTime);
        }

        protected override void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["AnimatedPlayer"];
        }

        protected override void SetDefaultMaterialParameters()
        {
            Alpha = 1.0f;
            SpecularPower = 16.0f;
            DiffuseColor = Vector3.One * 0.8f;
            SpecularColor = Vector3.One * 0.25f;
            EmissiveColor = Vector3.One * 0.0f;
        }

        private void RunController(string name)
        {
            foreach(BonePose p in animator.BonePoses)
            {
                p.CurrentController = controllers[name];
                p.CurrentBlendController = null;
            }
        }

        private void RunController(string name1, string name2, float blendFactor)
        {
            foreach (BonePose p in animator.BonePoses)
            {
                p.CurrentController = controllers[name1];
                p.CurrentBlendController = controllers[name2];
                p.BlendFactor = blendFactor;
            }
        }

        private ModelAnimator animator;
        //AnimationController idle, walk, jump;
        //AnimationController attack1, attack2, attack3, attack4;
        //AnimationController die;
        //AnimationController nod;
        float blendFactor;
        string state = "idle";

        private Vector3 color1;
        private Vector3 color2;
    }
}
