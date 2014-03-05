using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class BaseObject : DrawableGameComponent, IObject, IDeferredRender
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
        protected Vector3 nodeRotation = Vector3.Zero;
        Matrix[] boneTransforms;

        public Effect standardEffect;

        public bool TextureEnabled
        {
            get { return textureEnabled; }
            set { textureEnabled = value; }
        }

        public Vector3 NodeRotation
        {
            get { return nodeRotation; }
            set { nodeRotation = value; }
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

        protected BaseObject(Model model, Matrix transform, Vector3 nodeRotation, Game game)
            : base(game)
        {
            this.model = model;
            this.transform = transform;
            this.nodeRotation = nodeRotation;
            boneTransforms = new Matrix[model.Bones.Count];
        }

        public virtual void Draw(GameTime gameTime, Effect effect)
        {
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * Matrix.CreateFromYawPitchRoll(nodeRotation.X, nodeRotation.Y, nodeRotation.Z) * World;
                Matrix wvp = meshWorld * Camera.View * Camera.Projection;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    if (effect.Parameters["World"] != null)
                    {
                        effect.Parameters["World"].SetValue(meshWorld);
                    }
                    if (effect.Parameters["wvp"] != null)
                    {
                        effect.Parameters["wvp"].SetValue(wvp);
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
    }
}