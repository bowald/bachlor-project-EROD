using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BVector3 = BEPUutilities.Vector3;
using BQuaternion = BEPUutilities.Quaternion;
using BMatrix = BEPUutilities.Matrix;
using BMatrix3x3 = BEPUutilities.Matrix3x3;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Microsoft.Xna.Framework;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionRuleManagement;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    class Checkpoint : Trigger, IDeferredRender
    {
        private int ID;
        private static int sID = 1;

        #region Drawable

        private Model model;
        public Model Model 
        {
            get { return model;  }
            set 
            {
                model = value;
                boneTransforms = new Matrix[value.Bones.Count]; 
            } 
        }
        public Effect BasicEffect { get; set; }
        Matrix[] boneTransforms;

        Matrix Transform;

        #endregion

        public ICamera Camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        public Checkpoint(Game game, GameLogic gameLogic, BVector3 size, BVector3 position, BQuaternion rotation, CollisionGroup collisionGroup) 
            : base(game, gameLogic, size, position, rotation, collisionGroup)
        {
            Transform = Matrix.CreateScale(size.X, size.Y, size.Z);
            this.ID = sID;
            sID++;
            Entity.CollisionInformation.CollisionRules.Group = collisionGroup;
        }

        public Checkpoint(Game game, GameLogic gameLogic, BVector3 size, BVector3 position, float rotation, CollisionGroup collisionGroup)
            : this(game, gameLogic, size, position, BQuaternion.CreateFromAxisAngle(BVector3.Up, rotation), collisionGroup)
        {
        }

        public override void PairCreated(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
        {
            if(other.Tag is Ship)
            {
                Player p = GameLogic.GetPlayer((Ship)other.Tag);
                GameLogic.CheckpointPassed(ID, p);
            }
            base.PairCreated(sender, other, pair);
        }

        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, BasicEffect);
        }

        public void Draw(GameTime gameTime, Effect effect)
        {
            if (Model == null)
            {
                return;
            }

            Model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * Transform * ConversionHelper.MathConverter.Convert(Entity.WorldTransform);
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
                        effect.Parameters["textureEnabled"].SetValue(false);
                    }
                    if (effect.Parameters["diffuseTexture"] != null)
                    {
                        effect.Parameters["diffuseTexture"].SetValue((Texture2D)null);
                    }
                    if (effect.Parameters["specularMap"] != null)
                    {
                        effect.Parameters["specularMap"].SetValue((Texture2D)null);
                    }
                    if (effect.Parameters["bumpMap"] != null)
                    {
                        effect.Parameters["bumpMap"].SetValue((Texture2D)null);
                    }
                }
                mesh.Draw();
            }
        }
    }
}
