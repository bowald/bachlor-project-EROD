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

        protected Vector3 position;
        protected Vector3 scale;
        protected Quaternion rotation;
        
        protected Model model;
        protected Texture2D diffuseTexture;
        protected Texture2D specularMap;
        protected Texture2D bumpMap;
        protected Boolean textureEnabled;
        protected float bumpConstant = 0f;
        Matrix[] boneTransforms;

        public Effect standardEffect;
        public Effect shadowEffect;
        
        /// <summary>
        ///  Increases the "size" of tiled textures.
        /// </summary>
        private float texMult = 1f;
        public float TexMult
        { 
            get { return texMult; } 
            set { texMult = value;  } 
        }

        public bool TextureEnabled
        {
            get { return textureEnabled; }
            set { textureEnabled = value; }
        }

        public float BumpConstant
        {
            get { return bumpConstant; }
            set { bumpConstant = value; }
        }

        public Matrix World
        {
            get { return Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position); }
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

        protected BaseObject(Model model, Game game)
            : base(game)
        {
            this.model = model;
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
                    if (effect.Parameters["FarPlane"] != null)
                    {
                        effect.Parameters["FarPlane"].SetValue(Camera.FarPlane);
                    }
                    if (effect.Parameters["Color"] != null)
                    {
                        effect.Parameters["Color"].SetValue(Color.White.ToVector3());
                    }
                    if (effect.Parameters["TextureEnabled"] != null)
                    {
                        effect.Parameters["TextureEnabled"].SetValue(textureEnabled);
                    }
                    if (effect.Parameters["DiffuseTexture"] != null)
                    {
                        effect.Parameters["DiffuseTexture"].SetValue(diffuseTexture);
                    }
                    if (effect.Parameters["SpecularMap"] != null)
                    {
                        effect.Parameters["SpecularMap"].SetValue(specularMap);
                    }
                    if (effect.Parameters["BumpMap"] != null)
                    {
                        effect.Parameters["BumpMap"].SetValue(bumpMap);
                    }
                    if (effect.Parameters["bumpConstant"] != null)
                    {
                        effect.Parameters["bumpConstant"].SetValue(bumpConstant);
                    }
                    if (effect.Parameters["texMult"] != null)
                    {
                        effect.Parameters["texMult"].SetValue(TexMult);
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
                    if (shadowEffect.Parameters["LightView"] != null)
                    {
                        shadowEffect.Parameters["LightView"].SetValue(lightView);
                    }
                    if (shadowEffect.Parameters["LightProjection"] != null)
                    {
                        shadowEffect.Parameters["LightProjection"].SetValue(lightProjection);
                    }
                    if (shadowEffect.Parameters["FarPlane"] != null)
                    {
                        shadowEffect.Parameters["FarPlane"].SetValue(Camera.FarPlane);
                    }
                }
                mesh.Draw();
            }
        }
    }
}
