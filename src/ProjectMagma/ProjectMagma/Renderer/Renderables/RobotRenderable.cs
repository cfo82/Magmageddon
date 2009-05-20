using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using ProjectMagma.Shared.Model;
using ProjectMagma.MathHelpers;

namespace ProjectMagma.Renderer
{
    public class RobotRenderable : TexturedRenderable
    {
        public RobotRenderable(
            double timestamp,
            Vector3 scale, Quaternion rotation, Vector3 position, Model model, 
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture,
            Vector3 color1, Vector3 color2
        )
        :   base(timestamp, scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture)
        {
            this.color1 = color1;
            this.color2 = color2;

            RenderChannel = RenderChannelType.One;
            playerArrowColorBlend = new SineFloat(0.0f, 0.5f, 2.0f);
            frozenColorBlend = new SineFloat(0.6f, 1.0f, 2.8f);
        }


        ~RobotRenderable()
        {
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            ApplyEffectsToModel();
            InitializeControllers();
            LoadPlayerArrow();

            playerArrowColorBlend.Start(renderer.Time.At);
            frozenColorBlend.Start(renderer.Time.At);

            vertexPositionDeclaration = new VertexDeclaration(renderer.Device, VertexPositionTexture.VertexElements);
        }

        private void LoadPlayerArrow()
        {
            playerArrowEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Basic/Basic");

            playerArrowVertices = new VertexPositionTexture[4];
            Vector3 arrowDims = Scale * 0.6f;
            playerArrowVertices[0].Position = new Vector3(-arrowDims.X, 0, -arrowDims.Z);
            playerArrowVertices[0].TextureCoordinate = new Vector2(0, 0);
            playerArrowVertices[1].Position = new Vector3(-arrowDims.X, 0, arrowDims.Z);
            playerArrowVertices[1].TextureCoordinate = new Vector2(0, 1);
            playerArrowVertices[2].Position = new Vector3(arrowDims.X, 0, -arrowDims.Z);
            playerArrowVertices[2].TextureCoordinate = new Vector2(1, 0);
            playerArrowVertices[3].Position = new Vector3(arrowDims.X, 0, arrowDims.Z);
            playerArrowVertices[3].TextureCoordinate = new Vector2(1, 1);
            playerArrowTexture = Game.Instance.ContentManager.Load<Texture2D>("Textures/Visualizations/player_arrow");
        }

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

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer)
        {
            base.ApplyCustomEffectParameters(effect, renderer);

            effect.Parameters["ToneColor"].SetValue(color1);
            if(Frozen)
            {
                effect.Parameters["InvToneColor"].SetValue(new Vector3(0.2f, 0.45f, 0.8f) * frozenColorBlend.Value);
            }
        }

        public override void Update(Renderer renderer)
        {
            base.Update(renderer);
         
   
            frozenColorBlend.Update(renderer.Time.At);

            playerArrowColorBlend.Update(renderer.Time.At);
            animator.Update();
            foreach(AnimationController controller in controllers.Values)
            {
                controller.Update(renderer.Time.PausableAtGameTime);
            }

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

            //BonePose head = animator.BonePoses["head"];
            //head.CurrentController = controllers["nodHead"];
            //head.CurrentBlendController = null;
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


        public override void UpdateString(string id, double timestamp, string value)
        {
            base.UpdateString(id, timestamp, value);

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
            string requestedState = "";
            if (stateRequestString == "hit") {
                Random r = new Random();
                switch(r.Next(0,2))
                {
                    case 0: requestedState = "melee0"; break;
                    case 1: requestedState = "push"; break;
                    case 2: requestedState = "attack_spin"; break;
                    default: Debug.Assert(false); break;
                }
                //requestedState = "melee" + r.Next(0,0); // depends on how many we have
                requestedState = "push";
            }
            if (stateRequestString == "death")
            {
                requestedState = "death";
            }
            if (stateRequestString == "pushback")
            {
                requestedState = "pushback";
            }
            if (stateRequestString == "hurt_soft")
            {
                requestedState = "hurt_soft"; 
            }
            if (stateRequestString == "hurt_hard")
            {
                requestedState = "hurt_soft"; 
            }
            if (stateRequestString == "jump_end")
            {
                requestedState = "jump_end";
            }
            Debug.Assert(requestedState != "");

            blendFactor = blendIncrement;
            RunController(state, requestedState);
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
            if (stateRequestString == "run")
            {
                requestedState = "run";
            }
            if (stateRequestString == "attack_long")
            {
                requestedState = "attack_long_loop"; // todo: add start/end
            }
            if (stateRequestString == "repulsion")
            {
                requestedState = "crouch_loop"; // todo: add start/end
            }
            if (stateRequestString == "jump")
            {
                requestedState = "jump_loop";
            }
            if (stateRequestString == "win")
            {
                requestedState = "winning_loop";
            }
            if (stateRequestString == "crawl_left")
            {
                requestedState = "crawl_left_loop";
            }
            if (stateRequestString == "crawl_right")
            {
                requestedState = "crawl_right_loop";
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


        public override bool NeedsUpdate
        {
            get { return true; }
        }

        protected override void DrawMesh(Renderer renderer, ModelMesh mesh)
        {
            DrawPlayerArrow(renderer);

            animator.World = World;
            animator.Draw();

            base.DrawMesh(renderer, mesh);
        }

        private void DrawPlayerArrow(Renderer renderer)
        {
            playerArrowEffect.CurrentTechnique = playerArrowEffect.Techniques["TexturedNoCullNoDepth"];
            playerArrowEffect.Begin();
            playerArrowEffect.CurrentTechnique.Passes[0].Begin();
            ApplyWorldViewProjection(renderer, playerArrowEffect);            
            playerArrowEffect.Parameters["Local"].SetValue(Matrix.Identity);
            playerArrowEffect.Parameters["AmbientLightColor"].SetValue(Vector3.One);
            playerArrowEffect.Parameters["DiffuseColor"].SetValue(color1 * playerArrowColorBlend.Value + color2 * (1 - playerArrowColorBlend.Value));
            playerArrowEffect.Parameters["DiffuseTexture"].SetValue(playerArrowTexture);
            ApplyRenderChannel(playerArrowEffect, RenderChannelType.Three);
            ApplyEyePosition(renderer, playerArrowEffect);
            renderer.Device.VertexDeclaration = vertexPositionDeclaration;
            renderer.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, playerArrowVertices, 0, 2);
            playerArrowEffect.CurrentTechnique.Passes[0].End();
            playerArrowEffect.End();
        }

        protected override void ApplyTechnique(Effect effect)
        {
            if(Frozen)
            {
                effect.CurrentTechnique = effect.Techniques["DoublyColoredAnimatedPlayer"];
            }
            else
            {
                effect.CurrentTechnique = effect.Techniques["AnimatedPlayer"];
            }
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

        public override void UpdateBool(string id, double timestamp, bool value)
        {
            base.UpdateBool(id, timestamp, value);

            switch(id)
            {
                case "Frozen": Frozen = value; break;
            }
        }

        protected bool Frozen { get; set; }

        
        // player arrow
        VertexPositionTexture[] playerArrowVertices;
        Texture2D playerArrowTexture;
        Effect playerArrowEffect;
        SineFloat playerArrowColorBlend;

        // rendering related
        VertexDeclaration vertexPositionDeclaration;
        SineFloat frozenColorBlend;
        private Vector3 color1;
        private Vector3 color2;

        // animation related
        private static readonly float blendIncrement = 0.2f;
        private enum AnimationMode
        {
            Permanent, PermanentToPermanent, PermanentToOnce, Once, OnceToOnce, OnceToPermanent
        }
        private ModelAnimator animator;
        private AnimationMode animationMode;
        private AnimationController currentController;
        private Dictionary<string, AnimationController> controllers;
        float blendFactor;
        string state, destState, permanentState;
    }
}
