using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class BaseObject : DrawableGameComponent, IObject, IDeferredRender, ICastShadow
    {

        public ICamera Camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        protected Matrix world;
        protected Matrix transform;
        protected Model model;
        protected Texture2D diffuseTexture;
        protected Texture2D specularMap;
        protected Texture2D bumpMap;
        protected Boolean textureEnabled;
        Matrix[] boneTransforms;

        public Effect standardEffect;
        public Effect shadowEffect;

        public bool TextureEnabled
        {
            get { return textureEnabled; }
            set { textureEnabled = value; }
        }

        public Matrix Transform
        {
            get { return transform; }
            set { transform = value; }
        }

        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }

        public Model Model
        {
            get { return model; }
            set { model = value; }
        }

        public Texture2D Texture
        {
            get { return diffuseTexture; }
            set { diffuseTexture = value; }
        }

        public Texture2D SpecularMap
        {
            get { return specularMap; }
            set { specularMap = value; }
        }

        public Texture2D BumpMap
        {
            get { return bumpMap; }
            set { bumpMap = value; }
        }

        protected BaseObject(Model model, Matrix transform, Game game)
            : base(game)
        {
            this.model = model;
            this.transform = transform;
            boneTransforms = new Matrix[model.Bones.Count];
        }

        public virtual void Draw(GameTime gameTime, Effect effect)
        {
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * World;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    if (effect.Parameters["World"] != null)
                    {
                        effect.Parameters["World"].SetValue(meshWorld);
                    }
                    if (effect.Parameters["View"] != null)
                    {
                        effect.Parameters["View"].SetValue(Camera.View);
                    }
                    if (effect.Parameters["Projection"] != null)
                    {
                        effect.Parameters["Projection"].SetValue(Camera.Projection);
                    }
                    if (effect.Parameters["farPlane"] != null)
                    {
                        effect.Parameters["farPlane"].SetValue(Camera.FarPlane);
                    }
                    if (effect.Parameters["color"] != null)
                    {
                        effect.Parameters["color"].SetValue(Color.White.ToVector3());
                    }
                    if (effect.Parameters["textureEnabled"] != null)
                    {
                        effect.Parameters["textureEnabled"].SetValue(textureEnabled);
                    }
                    if (effect.Parameters["diffuseTexture"] != null)
                    {
                        effect.Parameters["diffuseTexture"].SetValue(diffuseTexture);
                    }
                    if (effect.Parameters["specularMap"] != null)
                    {
                        effect.Parameters["specularMap"].SetValue(specularMap);
                    }
                    if (effect.Parameters["bumpMap"] != null)
                    {
                        effect.Parameters["bumpMap"].SetValue(bumpMap);
                    }
                }
                mesh.Draw();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, standardEffect);
        }

        public void DrawShadow(GameTime gameTime, Matrix lightView, Matrix lightProjection)
        {
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * World;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = shadowEffect;
                    if (shadowEffect.Parameters["World"] != null)
                    {
                        shadowEffect.Parameters["World"].SetValue(meshWorld);
                    }
                    if (shadowEffect.Parameters["lightView"] != null)
                    {
                        shadowEffect.Parameters["lightView"].SetValue(lightView);
                    }
                    if (shadowEffect.Parameters["lightProjection"] != null)
                    {
                        shadowEffect.Parameters["lightProjection"].SetValue(lightProjection);
                    }
                }
                mesh.Draw();
            }
        }
    }
}
