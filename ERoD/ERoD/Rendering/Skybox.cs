using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ERoD
{
    public class Skybox
    {
        // Cube model to draw the skybox to.
        private Model skyBox;

        // Texture of the skybox
        private TextureCube skyBoxTexture;

        // Skybox shader
        private Effect shader;

        // Size of the cube
        private float size = 1f;

        public Skybox(string skyboxTexture, ContentManager Content)
        {
            skyBox = Content.Load<Model>("Skyboxes/cube");
            skyBoxTexture = Content.Load<TextureCube>(skyboxTexture);
            shader = Content.Load<Effect>("Shaders/Skybox");
        }

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            foreach (EffectPass pass in shader.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in skyBox.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = shader;
                        shader.Parameters["World"].SetValue(
                            Matrix.CreateScale(size) * Matrix.CreateTranslation(cameraPosition));
                        shader.Parameters["View"].SetValue(view);
                        shader.Parameters["Projection"].SetValue(projection);
                        shader.Parameters["SkyBoxTexture"].SetValue(skyBoxTexture);
                        shader.Parameters["CameraPosition"].SetValue(cameraPosition);
                    }
                    mesh.Draw();
                }
            }
        }
    }
}