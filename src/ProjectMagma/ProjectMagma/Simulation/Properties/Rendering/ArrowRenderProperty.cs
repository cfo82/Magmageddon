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
        protected override ModelRenderable CreateRenderable(Entity entity, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            scale *= 1.5f;
            return new ArrowRenderable(Game.Instance.Simulation.Time.At, renderPriority, scale, rotation, position, model);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            base.SetUpdatableParameters(entity);

            //Debug.Assert(entity.HasVector3(CommonNames.Color1));
            //Debug.Assert(entity.HasVector3(CommonNames.Color2));
            ChangeVector3("DiffuseColor", entity.GetVector3(CommonNames.Color1));
            ChangeVector3("SpecularColor", entity.GetVector3(CommonNames.Color2));
            ChangeVector3("EmissiveColor", entity.GetVector3(CommonNames.Color1));
            //ChangeFloat("SpecularPower", 16.0f);
            
            //}
            //if (entity.HasVector2(CommonNames.PersistentSquash))
            //{
            //    ChangeVector2("SquashParams", entity.GetVector2(CommonNames.PersistentSquash));
            //    ChangeBool("PersistentSquash", true);
            //}

            //arrow.AddVector3Attribute(CommonNames.DiffuseColor, player.GetVector3(CommonNames.Color2));
            //arrow.AddVector3Attribute(CommonNames.SpecularColor, Vector3.One);
            //arrow.AddFloatAttribute(CommonNames.Alpha, 0.6f);
            //arrow.AddFloatAttribute(CommonNames.SpecularPower, 0.3f);
            //arrow.AddVector2Attribute(CommonNames.PersistentSquash, new Vector2(1000, 0.8f));
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
