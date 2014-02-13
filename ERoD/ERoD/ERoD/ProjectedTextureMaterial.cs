using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class ProjectedTextureMaterial : Material
    {
        public Vector3 ProjectorPosition { get; set; }
        public Vector3 ProjectorTarget { get; set; }
        public Texture2D ProjectedTexture { get; set; }
        public bool ProjectorEnabled { get; set; }
        public float Scale { get; set; }
        float halfWidth, halfHeight;

        public ProjectedTextureMaterial(Texture2D Texture, GraphicsDevice graphicsDevice)
        {
            ProjectorPosition = new Vector3(1500, 1500, 1500);
            ProjectorTarget = new Vector3(0, 150, 0);
            ProjectorEnabled = true;
            ProjectedTexture = Texture;
            // We determine how large the texture will be based on the
            // texture dimensions and a scaling value
            halfWidth = Texture.Width / 2.0f;
            halfHeight = Texture.Height / 2.0f;
            Scale = 1;
        }

        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["ProjectorEnabled"] != null)
            {
                effect.Parameters["ProjectorEnabled"].SetValue(ProjectorEnabled);
            }
            if (!ProjectorEnabled)
            {
                return;
            }
            // Calculate an orthographic projection matrix for the
            // projector "camera"
            Matrix projection = Matrix.CreateOrthographicOffCenter(
                -halfWidth * Scale, halfWidth * Scale,
                -halfHeight * Scale, halfHeight * Scale,
                0, 100);
            // Calculate view matrix as usual
            Matrix view = Matrix.CreateLookAt(ProjectorPosition, ProjectorTarget, Vector3.Up);

            if (effect.Parameters["ProjectorViewProjection"] != null)
            {
                effect.Parameters["ProjectorViewProjection"].SetValue(view * projection);
            }
            if (effect.Parameters["ProjectedTexture"] != null)
            {
                effect.Parameters["ProjectedTexture"].SetValue(ProjectedTexture);
            }
        }
    }
}
