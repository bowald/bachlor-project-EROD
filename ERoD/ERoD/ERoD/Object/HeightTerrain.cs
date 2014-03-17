using BEPUphysics.BroadPhaseEntries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class HeightTerrain : DrawableGameComponent, IDeferredRender, ITerrain
    {

        public ICamera Camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        VertexBuffer myVertexBuffer;
        IndexBuffer myIndexBuffer;
        VertexPositionNormalTexture[] vertices;
        int[] indices;

        Texture2D texture;
        
        private int terrainWidth = 4;
        private int terrainHeight = 3;

        private float[,] heightData;

        private Terrain cTerrain;
        public StaticCollidable PhysicTerrain
        {
            get { return cTerrain; }
        }

        Effect StandardEffect;

        Matrix worldMatrix;

        public HeightTerrain(Game game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            StandardEffect = Game.Content.Load<Effect>("Shaders/HeightTerrainShader");
            Texture2D heightMap = Game.Content.Load<Texture2D>("HeightMap/height");
            Texture2D heightMapLowRes = Game.Content.Load<Texture2D>("HeightMap_lowres/height");
            texture = Game.Content.Load<Texture2D>("HeightMap/color");


            LoadHeightData(heightMap);

            SetUpVertices();
            SetUpIndices();

            CreatePhysicsTerrain();

            // Load lower res
            LoadHeightData(heightMapLowRes);

            worldMatrix = Matrix.CreateTranslation(new Vector3(-terrainWidth/2, 0, terrainHeight/2));

            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            CopyToBuffers();
        }

        private void CreatePhysicsTerrain()
        {
            cTerrain = new Terrain(heightData, new BEPUutilities.AffineTransform(
                        new BEPUutilities.Vector3(0.25f, 1, 0.25f),
                        BEPUutilities.Quaternion.Identity,
                        new BEPUutilities.Vector3(-terrainWidth / 8, 0, -terrainHeight / 8))
                    );
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, StandardEffect);
        }

        public void Draw(GameTime gameTime, Effect effect)
        {

            if (effect.Parameters["Texture"] != null) 
            {
                effect.Parameters["Texture"].SetValue(texture);
            }
            if (effect.Parameters["View"] != null) 
            {
                effect.Parameters["View"].SetValue(Camera.View);
            }
            if (effect.Parameters["Projection"] != null) 
            {
                effect.Parameters["Projection"].SetValue(Camera.Projection);
            }
            if (effect.Parameters["World"] != null) 
            {
                effect.Parameters["World"].SetValue(worldMatrix);
            }

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.Indices = myIndexBuffer;
                GraphicsDevice.SetVertexBuffer(myVertexBuffer);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
            }
        }

        private void CopyToBuffers()
        {
            myVertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            myVertexBuffer.SetData(vertices);

            myIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
            myIndexBuffer.SetData(indices);
        }

        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width;
            terrainHeight = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 2.0f;
                }
            }
        }

        private void CalculateNormals()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal = new Vector3(0, 0, 0);
            }

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal.Normalize();
            }
        }

        private void SetUpVertices()
        {
            vertices = new VertexPositionNormalTexture[terrainWidth * terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    vertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
                    vertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / terrainWidth;
                    vertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / terrainHeight;
                }
            }
        }

        private void SetUpIndices()
        {
            indices = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }
    }

    public struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }
}
