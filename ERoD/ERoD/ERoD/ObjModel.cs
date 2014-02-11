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

        public void DrawOld(Matrix View, Matrix Projection)
        {
            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            Matrix baseWorld = Matrix.CreateScale(Scale)
            * Matrix.CreateFromYawPitchRoll(
            Rotation.Y, Rotation.X, Rotation.Z)
            * Matrix.CreateTranslation(Position);
            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]
                * baseWorld;
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BasicEffect effect = (BasicEffect)meshPart.Effect;
                    effect.World = localWorld;
                    effect.View = View;
                    effect.Projection = Projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }

        public void Draw(Effect shader, Matrix view, Matrix projection, Vector3 viewVector)
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
                    // Set the part to use the shader effect.
                    part.Effect = shader;
                    // Set the parameters of the shader.
                    shader.Parameters["World"].SetValue(localWorld * mesh.ParentBone.Transform);
                    shader.Parameters["View"].SetValue(view);
                    shader.Parameters["Projection"].SetValue(projection);
                    Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform * localWorld));
                    shader.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);
                    shader.Parameters["ViewVector"].SetValue(viewVector);
                    shader.Parameters["ModelTexture"].SetValue(DiffuseTexture);
                    // TODO: Make a workaround that makes this check obselete
                    if (NormalExists)
                    {
                        shader.Parameters["NormalMap"].SetValue(NormalTexture);
                    }
                }
                mesh.Draw();
            }
        }
    }
}
