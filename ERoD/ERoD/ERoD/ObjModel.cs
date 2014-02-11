using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class ObjModel
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Model Model { get; private set; }
        private Matrix[] modelTransforms;
        private GraphicsDevice graphicsDevice;
        
        public Boolean DiffuseExists { get; private set; }
        private Texture2D diffuseTexture;
        public Texture2D DiffuseTexture
        {
            get { return diffuseTexture; }
            set
            {
                diffuseTexture = value;
                DiffuseExists = true;
            }
        }

        public Boolean NormalExists { get; private set; }
        private Texture2D normalTexture;
        public Texture2D NormalTexture
        {
            get { return normalTexture; }
            set
            {
                normalTexture = value;
                NormalExists = true;
            }
        }

        public Boolean SpecularExists { get; private set; }
        private Texture2D specularTexture;
        public Texture2D SpecularTexture
        {
            get { return specularTexture; }
            set
            {
                specularTexture = value;
                SpecularExists = true;
            }
        }

        public ObjModel(Model Model, Vector3 Position, Vector3 Rotation,
        Vector3 Scale, GraphicsDevice graphicsDevice)
        {
            this.Model = Model;
            modelTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);


            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
            this.graphicsDevice = graphicsDevice;

        }

        public void SetModelEffect(Effect effect, bool CopyEffect)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Effect toSet = effect;
                    // Copy the effect if necessary
                    if (CopyEffect)
                    {
                        toSet = effect.Clone();
                    }
                    // If this ModelMeshPart has a texture, set it to the effect
                    if (DiffuseExists)
                    {
                        setEffectParameter(toSet, "BasicTexture", DiffuseTexture);
                        setEffectParameter(toSet, "TextureEnabled", true);
                    }
                    else
                    {
                        setEffectParameter(toSet, "TextureEnabled", false);
                    }
                    // Set our remaining parameters to the effect
                    setEffectParameter(toSet, "DiffuseColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    setEffectParameter(toSet, "SpecularPower", 1.0f);
                    part.Effect = toSet;
                }
            }
        }
        // Sets the specified effect parameter to the given effect, if it
        // has that parameter
        void setEffectParameter(Effect effect, string paramName, object val)
        {
            if (effect.Parameters[paramName] == null)
            {
                return;
            }
            if (val is Vector3)
            {
                effect.Parameters[paramName].SetValue((Vector3)val);
            }
            if (val is Vector4)
            {
                effect.Parameters[paramName].SetValue((Vector4)val);
            }
            else if (val is bool)
            {
                effect.Parameters[paramName].SetValue((bool)val);
            }
            else if (val is Matrix)
            {
                effect.Parameters[paramName].SetValue((Matrix)val);
            }
            else if (val is Texture2D)
            {
                effect.Parameters[paramName].SetValue((Texture2D)val);
            }
        }


        public void Draw(Effect shader, Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            Matrix baseWorld = Matrix.CreateScale(Scale)
            * Matrix.CreateFromYawPitchRoll(
            Rotation.Y, Rotation.X, Rotation.Z)
            * Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]
                * baseWorld;
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Effect effect = part.Effect;

                    setEffectParameter(effect, "World", localWorld * mesh.ParentBone.Transform);
                    setEffectParameter(effect, "View", view);
                    setEffectParameter(effect, "Projection", projection);
                    Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform * localWorld));
                    setEffectParameter(effect, "WorldInverseTranspose", worldInverseTransposeMatrix);
                    setEffectParameter(effect, "CameraPosition", cameraPosition);
                    setEffectParameter(effect, "ModelTexture", DiffuseTexture);
                    setEffectParameter(effect, "NormalMap", NormalTexture);

                }
                mesh.Draw();
            }
        }
    }
}
