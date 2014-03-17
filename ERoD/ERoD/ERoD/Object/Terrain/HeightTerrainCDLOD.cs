using BEPUphysics.BroadPhaseEntries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class HeightTerrainCDLOD : DrawableGameComponent, IDeferredRender, ITerrain
    {
        #region Patch buffers

        // Contains the vertices for one patch. (A "plane" of triangles)
        private VertexBuffer patchVertexBuffer;

        // Contains the indices for one patch.
        private IndexBuffer patchIndexBuffer;

        // Two different patch instance buffers.
        // Index of the active patch buffer.
        // Used to avoid drawing to the patchbuffer which is currently being drawn.
        private int activePatchBufferIndex = 0;
        private DynamicVertexBuffer[] patchLists;

        #endregion

        #region Local data structures

        Matrix WorldMatrix = Matrix.CreateScale(ObjectConstants.WorldSize) * Matrix.CreateTranslation(0, 0, 0);

        // Height data as a texture. in float format.
        private Texture2D heightMap;

        // color texture of the ground.
        private Texture2D texture;

        // normals for the ground stored as a texture.
        private Texture2D normalMap;

        // Number of levels of detail.
        private int levels;

        // ranges used for morphing
        private float[] morphRanges;

        public QuadTree QuadTree { get; private set; }

        public int ActivePatchCount { get; private set; }

        #endregion

        #region Patch model generation

        // Generate a Model (Plane of 2 triangles) used to draw a Patch
        private void GeneratePatchModel(out VertexBuffer vertexBuffer, out IndexBuffer indexBuffer)
        {
            /// define a quad from two triangles
            var vectors = new List<Vector3>(
                new[] {
                    new Vector3(-0.5f, 0, -0.5f)
                    ,
                    new Vector3(0.5f, 0, -0.5f)
                    ,
                    new Vector3(0.5f, 0, 0.5f)
                    ,
                    new Vector3(-0.5f, 0, 0.5f)
                    ,
                }
                );

            var indices = new List<int>(new[] { 0, 1, 3, 1, 2, 3 });

            //GeometryProvider.Subdivide(vectors, indices, 3, true);

            // Halfvector used to get Texture coordinates from vertex data.
            var halfVector = new Vector2(0.5f, 0.5f);

            // Get a list of VertexPositionNormalTexture from the vectors
            // Normal not interesting as it will be calculated later.
            var vertices = vectors.Select(
                v =>
                new VertexPositionNormalTexture(v, v, new Vector2(v.X, v.Z) + halfVector)
                )
                .ToList();


            // The patch can be subdivided any number of times to get more triangles.
            /// subdivide the patch from 2 triangles to 8
            Subdivide(vertices, indices, true);
            Subdivide(vertices, indices, true);
            Subdivide(vertices, indices, true);


            vertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(Game.GraphicsDevice, indices[0].GetType(), indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }

        #endregion

        #region Subdivision

        private int GetMidpointIndex(Dictionary<string, int> midpointIndices, List<VertexPositionNormalTexture> vertices, int i0, int i1)
        {

            var edgeKey = string.Format("{0}_{1}", Math.Min(i0, i1), Math.Max(i0, i1));

            var midpointIndex = -1;

            if (!midpointIndices.TryGetValue(edgeKey, out midpointIndex))
            {
                var halfVector = new Vector2(0.5f);
                var v0 = vertices[i0];
                var v1 = vertices[i1];

                var midpoint = new VertexPositionNormalTexture();

                /// midpoint position
                midpoint.Position = (v0.Position + v1.Position) / 2f;
                /// morph target
                midpoint.Normal = Vector3.Min(v0.Position, v1.Position);
                midpoint.TextureCoordinate = new Vector2(midpoint.Position.X, midpoint.Position.Z) + halfVector;

                midpointIndex = vertices.Count;

                midpointIndices.Add(edgeKey, midpointIndex);

                vertices.Add(midpoint);
            }


            return midpointIndex;

        }

        public void Subdivide(List<VertexPositionNormalTexture> vertices, List<int> indices, bool removeSourceTriangles)
        {
            var midpointIndices = new Dictionary<string, int>();

            var newIndices = new List<int>(indices.Count * 4);

            if (!removeSourceTriangles)
                newIndices.AddRange(indices);

            for (var i = 0; i < indices.Count - 2; i += 3)
            {
                var i0 = indices[i];
                var i1 = indices[i + 1];
                var i2 = indices[i + 2];

                var m01 = GetMidpointIndex(midpointIndices, vertices, i0, i1);
                var m12 = GetMidpointIndex(midpointIndices, vertices, i1, i2);
                var m02 = GetMidpointIndex(midpointIndices, vertices, i2, i0);

                newIndices.AddRange(
                    new[] {
                        i0,m01,m02
                        ,
                        i1,m12,m01
                        ,
                        i2,m02,m12
                        ,
                        m02,m01,m12
                    }
                    );

            }

            indices.Clear();
            indices.AddRange(newIndices);
        }

        #endregion

        #region Update view

        private Terrain cTerrain;
        public StaticCollidable PhysicTerrain
        {
            get { return cTerrain; }
        }

        Vector3 EyePos
        {
            get { return Vector3.Transform(Camera.Position, Matrix.Invert(WorldMatrix)); }
        }

        // Update the currently selected patchList to include different patches based on
        // Camera location.
        private int UpdatePatchList(GameTime gameTime, ref DynamicVertexBuffer patchList)
        {
            BoundingFrustum frustum = new BoundingFrustum(Camera.View * Camera.Projection);

            var selectedNodes = new List<QuadTreeNode>();

            

            // Select nodes and return if none are selected.
            bool noneSelected = !QuadTree.LodSelect(EyePos, ref morphRanges, ref frustum, selectedNodes)
                || selectedNodes.Count <= 0;
            if (noneSelected)
            {
                return 0;
            }

            var instances = new List<VertexMatrixNormal>();

            foreach (QuadTreeNode node in selectedNodes)
            {
                Vector3 boundsMin = node.BoundingBox.Min;
                Vector3 boundsMax = node.BoundingBox.Max;

                // find the size of the selected node.
                float size = Math.Min(boundsMax.X - boundsMin.X, boundsMax.Z - boundsMin.Z);
                //Console.WriteLine(node.BoundingBox);
                //Console.WriteLine(size);
                
                // Set morph info to send to shader.
                float rangeStart = morphRanges[node.Level - 1];
                float rangeEnd = morphRanges[node.Level];
                Vector3 position = node.GetCenter();

                // Send data to the shaders, Matrix as the patches are 1x1 in size and at 0, 0 in world.
                var patch = new VertexMatrixNormal(
                    Matrix.CreateScale(size) * Matrix.CreateTranslation(position)
                    , new Vector3(rangeStart, rangeEnd, node.Level)
                    );

                instances.Add(patch);
            }

            /// if necessary, recreate/resize the current patch instance buffer. 
            /// Recreating the buffer per frame incurs a serious performance hit, so this is only done
            /// if we need more space in the buffer than is already allocated. 
            if (patchList.IsDisposed || patchList.VertexCount < instances.Count)
            {
                if (!patchList.IsDisposed)
                    patchList.Dispose();

                patchList = new DynamicVertexBuffer(GraphicsDevice, VertexMatrixNormal.VertexDeclaration, instances.Count, BufferUsage.WriteOnly);
            }


            patchList.SetData(instances.ToArray(), 0, instances.Count, SetDataOptions.NoOverwrite);
            
            return instances.Count;
        }


        #endregion

        public ICamera Camera
        {
            get { return (ICamera)Game.Services.GetService(typeof(ICamera));  }
        }
        private Effect baseEffect;

        public HeightTerrainCDLOD(Game game, int levels)
            : base(game)
        {
            this.levels = levels;
        }

        public override void Initialize()
        {
            GeneratePatchModel(out patchVertexBuffer, out patchIndexBuffer);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            baseEffect = Game.Content.Load<Effect>("Shaders/CDLODTerrain");

            // Pair of patch instance buffers.
            patchLists = new DynamicVertexBuffer[2];

            patchLists[0] = new DynamicVertexBuffer(GraphicsDevice, VertexMatrixNormal.VertexDeclaration, 
                1, BufferUsage.None);
            patchLists[1] = new DynamicVertexBuffer(GraphicsDevice, VertexMatrixNormal.VertexDeclaration, 
                1, BufferUsage.None);


            // Load heightdata
            Texture2D tempHeight = Game.Content.Load<Texture2D>("HeightMap/heightNC");

            Microsoft.Xna.Framework.Graphics.PackedVector.Rgba64[] heightValues = new Microsoft.Xna.Framework.Graphics.PackedVector.Rgba64[tempHeight.Width * tempHeight.Height];
            tempHeight.GetData(heightValues);

            // Get float height values in [-0.125, 0.125] from the colors.
            float[] heights = heightValues.Select(c => (c.ToVector4().X - 0.5f) * 0.25f).ToArray();

            float[,] physHeightData = new float[tempHeight.Width, tempHeight.Height];
            for (int x = 0; x < tempHeight.Width; x++)
            {
                for (int y = 0; y < tempHeight.Height; y++)
                {
                    physHeightData[x, y] = heights[x + y * tempHeight.Width] * ObjectConstants.WorldSize;
                }
            }

            // Scale from 2048*2048 to 512*512 ingame coordinates
            float scaleX = ObjectConstants.WorldSize / (float)tempHeight.Width;
            float scaleZ = ObjectConstants.WorldSize / (float)tempHeight.Height;

            Console.WriteLine(scaleX);

            cTerrain = new Terrain(physHeightData, new BEPUutilities.AffineTransform(
                        new BEPUutilities.Vector3(scaleX, 1, scaleZ),
                        BEPUutilities.Quaternion.Identity,
                        new BEPUutilities.Vector3((-tempHeight.Width / 2) * scaleX, 0, (-tempHeight.Height / 2) * scaleZ))
                    );

            heightMap = new Texture2D(Game.GraphicsDevice, tempHeight.Width, tempHeight.Height, false, SurfaceFormat.Single);
            heightMap.SetData(heights);

            normalMap = Game.Content.Load<Texture2D>("HeightMap/normal");
            texture = Game.Content.Load<Texture2D>("HeightMap/color");

            // generate morph ranges (can be done in shaders??)
            var ranges = Enumerable.Range(0, levels + 1).Select(i => (float)Math.Pow(2, i - 1)/1.5f).ToList();
            ranges.Add(0);
            ranges.Sort((a, b) => a.CompareTo(b));
            
            morphRanges = ranges.ToArray();

            // Build the QuadTree witht the height data and morph ranges
            QuadTree = QuadTree.Build(ref heights, heightMap.Width, heightMap.Height, ref morphRanges);
        }

        public void SetEffectParameters(GameTime gameTime, Effect effect)
        {
            effect.Parameters["World"].SetValue(WorldMatrix);

            effect.Parameters["View"].SetValue(Camera.View);
            effect.Parameters["Projection"].SetValue(Camera.Projection);
            effect.Parameters["WorldViewProjection"].SetValue(WorldMatrix * Camera.View * Camera.Projection);

            effect.Parameters["HeightMap"].SetValue(heightMap);
            effect.Parameters["NormalMap"].SetValue(normalMap);
            effect.Parameters["Texture"].SetValue(texture);
            effect.Parameters["EyePosition"].SetValue(EyePos);
        }

        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, baseEffect);
        }

        public void Draw(GameTime gameTime, Effect effect)
        {
            SetEffectParameters(gameTime, effect);

            var activePatchBuffer = patchLists[activePatchBufferIndex];

            int activePatchCount = UpdatePatchList(gameTime, ref activePatchBuffer);
            ActivePatchCount = activePatchCount;

            // Swap active patchList buffer
            activePatchBufferIndex = 1 - activePatchBufferIndex;

            if (activePatchCount > 0)
            {
                GraphicsDevice.SetVertexBuffers(
                    new[] 
                    { 
                        new VertexBufferBinding(patchVertexBuffer, 0, 0),
                        new VertexBufferBinding(activePatchBuffer, 0, 1) 
                    }
                );
                GraphicsDevice.Indices = patchIndexBuffer;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList
                        , 0
                        , 0
                        , patchVertexBuffer.VertexCount
                        , 0
                        , patchIndexBuffer.IndexCount / 3
                        , activePatchCount
                        );
                }
            }

            base.Draw(gameTime);
        }
    }
}
