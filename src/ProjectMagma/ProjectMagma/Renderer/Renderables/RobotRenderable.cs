using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using ProjectMagma.Shared.Model;

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

        ~RobotRenderable()
        {
            //Console.WriteLine("blah");
        }

        //protected override void ApplyEffectsToModel()
        //{
        //    base.ApplyEffectsToModel();
            
        //    // create animation component. however, it will register itself in the main 
        //    // game class as a drawable component. we do not like that and remove it again.

        //    // create all the individual animation controllers as defined in the xml file
        //    // accompanying the player model
        //    //idle = new AnimationController(Game.Instance, animator.Animations["idle0"]);
        //    //walk = new AnimationController(Game.Instance, animator.Animations["walk"]);
        //    //nod = new AnimationController(Game.Instance, animator.Animations["nodHead"]);

        //}

        VertexPositionTexture[] vpt;
        VertexDeclaration vd;

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            ApplyEffectsToModel();
            InitializeControllers();

            eff = new BasicEffect(renderer.Device, null);

            vpt = new VertexPositionTexture[4];
            Vector3 arrowDims = Scale * 0.4f;
            vpt[0].Position = new Vector3(-arrowDims.X, 0, -arrowDims.Z);
            vpt[0].TextureCoordinate = new Vector2(0, 0);
            vpt[1].Position = new Vector3(-arrowDims.X, 0, arrowDims.Z);
            vpt[1].TextureCoordinate = new Vector2(0, 1);
            vpt[2].Position = new Vector3(arrowDims.X, 0, -arrowDims.Z);
            vpt[2].TextureCoordinate = new Vector2(1, 0);
            vpt[3].Position = new Vector3(arrowDims.X, 0, arrowDims.Z);
            vpt[3].TextureCoordinate = new Vector2(1, 1);

            vd = new VertexDeclaration(renderer.Device, VertexPositionTexture.VertexElements);

            rect = Game.Instance.ContentManager.Load<MagmaModel>("Models/Primitives/lava_primitive").XnaModel;
            playerArrowTexture = Game.Instance.ContentManager.Load<Texture2D>("Textures/Visualizations/player_arrow");
        }

        Model rect;
        Texture2D playerArrowTexture;

        private void InitializeControllers()
        {
            animator = new ModelAnimator(Model);

            controllers = new Dictionary<string, AnimationController>();
            foreach (string key in animator.Animations.Keys)
            {
                controllers.Add(key, new AnimationController(Game.Instance, animator.Animations[key]));

                // each controller registers itself in the game. we don't like this.
                Game.Instance.Components.RemoveAt(Game.Instance.Components.Count - 1);
            }

//            Console.WriteLine("components: " + Game.Instance.Components.Count);           

            // set the first default controller
            RunController("idle0");
            animationMode = AnimationMode.Permanent;
            permanentState = "idle0";
        }

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer, GameTime gameTime)
        {
            base.ApplyCustomEffectParameters(effect, renderer, gameTime);

            effect.Parameters["ToneColor"].SetValue(color1);
        }

        public override void Update(Renderer renderer, GameTime gameTime)
        {
            base.Update(renderer, gameTime);

            animator.Update(gameTime);

            if (blendFactor > 0.0f)
            {
                blendFactor += blendIncrement;
                //currentController.ElapsedTime = 0; // test
            }
            if (blendFactor >= 1.0f)
            {
                blendFactor = 0.0f;
                state = destState;
                destState = null;
                RunController(state);
                if(animationMode == AnimationMode.PermanentToOnce)
                    animationMode = AnimationMode.Once;
                if(animationMode == AnimationMode.OnceToPermanent)
                    animationMode = AnimationMode.Permanent;
            }

            BonePose head = animator.BonePoses["head"];
            head.CurrentController = controllers["nodHead"];
            head.CurrentBlendController = null;
//            Console.WriteLine("elapsed time: " + currentController.ElapsedTime);
        }

        public void OnceAnimEndedHandler(object sender, EventArgs args)
        {
            animationMode = AnimationMode.OnceToPermanent;
            blendFactor = blendIncrement;
            destState = permanentState;
            RunController(state, destState);

            (sender as AnimationController).AnimationEnded -= OnceAnimEndedHandler;
        }


        //public string NextPermanentState
        //{
        //    set { ActivateOneTimeState(value); }
        //}

        //public string NextOneTimeState {
        //    set { ActivateOneTimeState(value); }
        //}

        public override void UpdateString(string id, string value)
        {
            base.UpdateString(id, value);

            if(id=="NextOnceState")
            {
                ActivateOnceState(value);
            }
            if(id=="NextPermanentState")
            {
                ActivatePermanentState(value);
            }
        }

        // activatepermanentstate: idle, walk, jump, die
        // activatesinglestate: attack
        public void ActivateOnceState(string stateRequestString)
        {
            // update animation
            //Console.WriteLine("activateonetimestate call");
            if(stateRequestString=="hit") {
                Random r = new Random();
                destState = "attack" + r.Next(0,4);
            }
            blendFactor = blendIncrement;
            RunController(state, destState);
            currentController.IsLooping = false;
            currentController.ElapsedTime = 0;
            currentController.AnimationEnded += OnceAnimEndedHandler;

            // update mode
            animationMode = AnimationMode.PermanentToOnce;
        }

        public void ActivatePermanentState(string stateRequestString)
        {
            string requestedState = "";
            if (stateRequestString == "idle")
            {
                requestedState = "idle0";
            }
            if (stateRequestString == "walk")
            {
                requestedState = "walk";
            }
            Debug.Assert(requestedState != "");

            // update animation
            if(animationMode==AnimationMode.Permanent ||
                animationMode==AnimationMode.OnceToPermanent ||
                animationMode==AnimationMode.PermanentToPermanent)
            {
                blendFactor = blendIncrement;
                RunController(state, requestedState);
                if (animationMode == AnimationMode.Permanent)
                    animationMode = AnimationMode.PermanentToPermanent;
            }

            // update mode
            //animationMode = AnimationMode.OnceToPermanent;
            permanentState = requestedState;
        }

        private readonly float blendIncrement = 0.2f;

        public override bool NeedsUpdate
        {
            get { return true; }
        }


        BasicEffect eff;

        protected override void DrawMesh(Renderer renderer, GameTime gameTime, ModelMesh mesh)
        {

            eff.Begin();
            eff.CurrentTechnique.Passes[0].Begin();
            eff.World = World;
            eff.View = renderer.Camera.View;
            eff.Projection = renderer.Camera.Projection;
            eff.DiffuseColor = color1;
            //eff.VertexColorEnabled = true;
            eff.TextureEnabled = true;
            eff.Texture = playerArrowTexture;

            renderer.Device.VertexDeclaration = vd;
            CullMode oldCullMode = renderer.Device.RenderState.CullMode;

            renderer.Device.RenderState.CullMode = CullMode.None;
            renderer.Device.RenderState.DepthBufferEnable = false;
            renderer.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vpt, 0, 2);
            renderer.Device.RenderState.CullMode = oldCullMode;
            //rect.Meshes[0].Draw();
            //Model.Meshes[0].Draw();
            eff.CurrentTechnique.Passes[0].End();
            eff.End();

            animator.World = World;
            animator.Draw(gameTime);

            base.DrawMesh(renderer, gameTime, mesh);
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
            state = name;
            currentController = controllers[name];
            foreach(BonePose p in animator.BonePoses)
            {
                p.CurrentController = currentController;
                p.CurrentBlendController = null;
                p.BlendFactor = 0.0f;
            }
            //Console.WriteLine("Running controller " + name);
                //animationController
        }

        private void RunController(string name1, string name2)
        {
            state = name1;
            destState = name2;
            currentController = controllers[name1];
            //Console.WriteLine("Running controller " + name1 + " to " + name2);
            foreach (BonePose p in animator.BonePoses)
            {
                p.CurrentController = currentController;
                p.CurrentBlendController = controllers[name2];
                p.BlendFactor = blendFactor;
            }
        }

        private ModelAnimator animator;
        //AnimationController idle, walk, jump;
        //AnimationController attack1, attack2, attack3, attack4;
        //AnimationController die;
        //AnimationController nod;
        private Dictionary<string, AnimationController> controllers;

        private AnimationController currentController;
        float blendFactor;
        string state, destState, permanentState;//= "idle";

        private Vector3 color1;
        private Vector3 color2;

        private enum AnimationMode
        {
            Permanent, PermanentToPermanent, PermanentToOnce, Once, OnceToOnce, OnceToPermanent
        }
        private AnimationMode animationMode;
    }
}
