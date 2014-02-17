using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class BaseObject : DrawableGameComponent, IObject
    {
        
        //Entity entity;
        protected Matrix world;
        protected Model model;
        protected Texture2D texture;
        Matrix[] boneTransforms;

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
            get { return texture; }
            set { texture = value; }
        }

        protected BaseObject(Model model, Matrix world, Game game) : base(game)
        {
            this.model = model;
            this.world = world;
            boneTransforms = new Matrix[model.Bones.Count];
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                }
            }
           
        }
        public override void Draw(GameTime gameTime)
        {
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * world;
                    effect.View = (Game as ERoD).Camera.View;
                    effect.Projection = (Game as ERoD).Camera.Projection;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
