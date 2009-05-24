using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class ArrowRenderProperty : BasicRenderProperty
    {
        protected override ModelRenderable CreateRenderable(Entity entity, Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            scale *= 1.5f;
            return new ArrowRenderable(Game.Instance.Simulation.Time.At, scale, rotation, position, model);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            base.SetUpdatableParameters(entity);

            //Debug.Assert(entity.HasVector3("color1"));
            //Debug.Assert(entity.HasVector3("color2"));
            ChangeVector3("DiffuseColor", entity.GetVector3("color1"));
            ChangeVector3("SpecularColor", entity.GetVector3("color2"));
            ChangeVector3("EmissiveColor", entity.GetVector3("color1"));
            //ChangeFloat("SpecularPower", 16.0f);
            
            //}
            //if (entity.HasVector2("persistent_squash"))
            //{
            //    ChangeVector2("SquashParams", entity.GetVector2("persistent_squash"));
            //    ChangeBool("PersistentSquash", true);
            //}

            //arrow.AddVector3Attribute("diffuse_color", player.GetVector3("color2"));
            //arrow.AddVector3Attribute("specular_color", Vector3.One);
            //arrow.AddFloatAttribute("alpha", 0.6f);
            //arrow.AddFloatAttribute("specular_power", 0.3f);
            //arrow.AddVector2Attribute("persistent_squash", new Vector2(1000, 0.8f));
            //ChangeFloat("Alpha", 0.8f);
            PersistentSquash = false;
            JumpPossible = false;
        }

        public bool JumpPossible
        {
            set {
                if(value==true)
                {
                    SquashParams = new Vector2(100f, 1f);
                }
                else
                {
                    SquashParams = new Vector2(1000f, 0.8f);
                }
            }
        }
    }
}
