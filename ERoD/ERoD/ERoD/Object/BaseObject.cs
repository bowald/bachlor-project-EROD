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
        protected Boolean textureEnabled;
        Matrix[] boneTransforms;

        public Effect Effect;

        public bool TextureEnabled
        {//TODO add to shader code
            get { return textureEnabled; }
            set { textureEnabled = value; }
        }

        public Matrix Transform
        {
            get { return transform;  }
            set { transform = value;  }
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

        protected BaseObject(Model model, Matrix transform, Game game) : base(game)
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
                Matrix wvp = meshWorld * Camera.View * Camera.Projection;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(meshWorld);
                    effect.Parameters["wvp"].SetValue(wvp);
                    effect.Parameters["color"].SetValue(Color.White.ToVector3());
                    effect.Parameters["diffuseTexture"].SetValue(diffuseTexture);
                }
                mesh.Draw();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, Effect);
        }


        
    }
}
