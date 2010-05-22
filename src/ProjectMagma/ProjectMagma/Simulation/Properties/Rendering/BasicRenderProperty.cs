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
    public class BasicRenderProperty : ModelRenderProperty
    {
        protected override ModelRenderable CreateRenderable(Entity entity, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            return new BasicRenderable(Game.Instance.Simulation.Time.At, renderPriority, scale, rotation, position, model);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            if (entity.HasFloat(CommonNames.LavaLightStrength))
            {
                ChangeFloat("LavaLightStrength", entity.GetFloat(CommonNames.LavaLightStrength));
            }
            if (entity.HasFloat(CommonNames.SkyLightStrength))
            {
                ChangeFloat("SkyLightStrength", entity.GetFloat(CommonNames.SkyLightStrength));
            }
            if (entity.HasFloat(CommonNames.SpotLightStrength))
            {
                ChangeFloat("SpotLightStrength", entity.GetFloat(CommonNames.SpotLightStrength));
            }
            if (entity.HasVector3(CommonNames.DiffuseColor))
            {
                ChangeVector3("DiffuseColor", entity.GetVector3(CommonNames.DiffuseColor));
            }
            if (entity.HasVector3(CommonNames.EmissiveColor))
            {
                ChangeVector3("EmissiveColor", entity.GetVector3(CommonNames.EmissiveColor));
            }
            if (entity.HasVector3(CommonNames.SpecularColor))
            {
                ChangeVector3("SpecularColor", entity.GetVector3(CommonNames.SpecularColor));
            }
            if (entity.HasFloat(CommonNames.SpecularPower))
            {
                ChangeFloat("SpecularPower", entity.GetFloat(CommonNames.SpecularPower));
            }
            if (entity.HasFloat(CommonNames.Alpha))
            {
                ChangeFloat("Alpha", entity.GetFloat(CommonNames.Alpha));
            }
            if (entity.HasVector2(CommonNames.PersistentSquash))
            {
                ChangeVector2("SquashParams", entity.GetVector2(CommonNames.PersistentSquash));
                ChangeBool("PersistentSquash", true);
            }
        }

        public void Squash()
        {
            ChangeBool("Squash", true);
        }

        public void Blink()
        {
            ChangeBool("Blink", true);
        }

        public Vector2 SquashParams 
        {
            set
            {
                ChangeVector2("SquashParams", value);
            }
        }

        public bool PersistentSquash
        {
            set
            {
                ChangeBool("PersistentSquash", value);
            }
        }

    }
}
