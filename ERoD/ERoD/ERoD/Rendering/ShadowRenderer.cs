using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class ShadowRenderer
    {

        public class CascadeShadowMapEntry
        {
            public RenderTarget2D ShadowMap;
            public LightViewProjection[] LightViewProjectionMatrices = new LightViewProjection[NumSplits];
            public Vector2[] LightClipPlanes = new Vector2[NumSplits];
        }

        public struct LightViewProjection
        {
            public Matrix LightView;
            public Matrix LightProjection;
        }

        public const int ShadowMapSize = 1024;
        public const int NumSplits = 3;

        DeferredRenderer renderer;
        Game game;

        Vector3[] frustumCornersVS = new Vector3[8];
        Vector3[] frustumCornersWS = new Vector3[8];
        Vector3[] frustumCornersLS = new Vector3[8];
        Vector3[] splitFrustumCornersVS = new Vector3[8];
        Vector2[] lightClipPlanes = new Vector2[NumSplits];
        float[] splitDepths = new float[NumSplits + 1];

        /// <summary>
		/// Creates the renderer
		/// </summary>
		/// <param name="graphicsDevice">The GraphicsDevice to use for rendering</param>
        /// <param name="contentManager">The ContentManager to use for loading content</param>
        public ShadowRenderer(DeferredRenderer renderer, Game game)
        {
            this.renderer = renderer;
            this.game = game;
        }

        public RenderTarget2D AssignShadowMap()
        {
            RenderTarget2D shadowMap = new RenderTarget2D(renderer.GraphicsDevice, ShadowMapSize * NumSplits,
                                                   ShadowMapSize, false, SurfaceFormat.Single,
                                                   DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            return shadowMap;
        }

        public void RenderCascadedShadowMap(IDirectionalLight light, ICamera camera, GameTime gameTime)
        {

            renderer.GraphicsDevice.SetRenderTarget(light.ShadowMapEntry.ShadowMap);
            //clear it to white, ie, far far away
            renderer.GraphicsDevice.Clear(Color.Black);
            renderer.GraphicsDevice.BlendState = BlendState.Opaque;
            renderer.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Get the corners of the frustum
            camera.Frustum.GetCorners(frustumCornersWS);
            Matrix eyeTransform = camera.View;
            Vector3.Transform(frustumCornersWS, ref eyeTransform, frustumCornersVS);

            // Calculate the cascade splits.  We calculate these so that each successive
            // split is larger than the previous, giving the closest split the most amount
            // of shadow detail.  
            float near = camera.NearPlane;
            float far = MathHelper.Min(camera.FarPlane, light.ShadowDistance);
            splitDepths[0] = near;
            splitDepths[NumSplits] = far;
            const float splitConstant = 0.95f;
            for (int i = 1; i < splitDepths.Length - 1; i++)
            {
                splitDepths[i] = near + (far - near) * (float)Math.Pow((i / ((float)NumSplits)), 2);
                //splitDepths[i] = splitConstant * near * (float)Math.Pow(far / near, i / NumSplits) + (1.0f - splitConstant) * ((near + (i / NumSplits)) * (far - near));
            }

            Viewport splitViewport = new Viewport();
            Vector3 lightDir = -Vector3.Normalize(light.Direction);

            for (int i = 0; i < NumSplits; i++)
            {
                light.ShadowMapEntry.LightClipPlanes[i].X = -splitDepths[i];
                light.ShadowMapEntry.LightClipPlanes[i].Y = -splitDepths[i + 1];

                light.ShadowMapEntry.LightViewProjectionMatrices[i] = CreateLightViewProjectionMatrix(lightDir, far, camera, splitDepths[i], splitDepths[i + 1], i);

                // Set the viewport for the current split     
                splitViewport.MinDepth = 0;
                splitViewport.MaxDepth = 1;
                splitViewport.Width = ShadowMapSize;
                splitViewport.Height = ShadowMapSize;
                splitViewport.X = i * ShadowMapSize;
                splitViewport.Y = 0;
                renderer.GraphicsDevice.Viewport = splitViewport;

                foreach (GameComponent component in game.Components)
                {
                    if (component is ICastShadow)
                    {
                        ((ICastShadow)component).DrawShadow(gameTime, light.ShadowMapEntry.LightViewProjectionMatrices[i].LightView, light.ShadowMapEntry.LightViewProjectionMatrices[i].LightProjection);
                    }
                }

            }
        }

        /// <summary>
        /// Creates the WorldViewProjection matrix from the perspective of the 
        /// light using the cameras bounding frustum to determine what is visible 
        /// in the scene.
        /// </summary>
        /// <returns>The WorldViewProjection for the light</returns>
        LightViewProjection CreateLightViewProjectionMatrix(Vector3 lightDir, float farPlane, ICamera camera, float minZ, float maxZ, int index)
        {
            for (int i = 0; i < 4; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i + 4] * (minZ / camera.FarPlane);

            for (int i = 4; i < 8; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i] * (maxZ / camera.FarPlane);

            Matrix cameraMat = camera.World;
            Vector3.Transform(splitFrustumCornersVS, ref cameraMat, frustumCornersWS);

            // Matrix with that will rotate in points the direction of the light
            Vector3 cameraUpVector = camera.View.Up;
            if (Math.Abs(Vector3.Dot(cameraUpVector, lightDir)) > 0.9f)
                cameraUpVector = Vector3.Forward;

            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
                                                       -lightDir,
                                                       cameraUpVector);


            // Transform the positions of the corners into the direction of the light
            for (int i = 0; i < frustumCornersWS.Length; i++)
            {
                frustumCornersWS[i] = Vector3.Transform(frustumCornersWS[i], lightRotation);
            }


            // Find the smallest box around the points
            Vector3 mins = frustumCornersWS[0], maxes = frustumCornersWS[0];
            for (int i = 1; i < frustumCornersWS.Length; i++)
            {
                Vector3 p = frustumCornersWS[i];
                if (p.X < mins.X) mins.X = p.X;
                if (p.Y < mins.Y) mins.Y = p.Y;
                if (p.Z < mins.Z) mins.Z = p.Z;
                if (p.X > maxes.X) maxes.X = p.X;
                if (p.Y > maxes.Y) maxes.Y = p.Y;
                if (p.Z > maxes.Z) maxes.Z = p.Z;
            }


            // Find the smallest box around the points in view space
            Vector3 minsVS = splitFrustumCornersVS[0], maxesVS = splitFrustumCornersVS[0];
            for (int i = 1; i < splitFrustumCornersVS.Length; i++)
            {
                Vector3 p = splitFrustumCornersVS[i];
                if (p.X < minsVS.X) minsVS.X = p.X;
                if (p.Y < minsVS.Y) minsVS.Y = p.Y;
                if (p.Z < minsVS.Z) minsVS.Z = p.Z;
                if (p.X > maxesVS.X) maxesVS.X = p.X;
                if (p.Y > maxesVS.Y) maxesVS.Y = p.Y;
                if (p.Z > maxesVS.Z) maxesVS.Z = p.Z;
            }
            BoundingBox _lightBox = new BoundingBox(mins, maxes);

            bool fixShadowJittering = true;
            if (fixShadowJittering)
            {
                // I borrowed this code from some forum that I don't remember anymore =/
                // We snap the camera to 1 pixel increments so that moving the camera does not cause the shadows to jitter.
                // This is a matter of integer dividing by the world space size of a texel
                float diagonalLength = (frustumCornersWS[0] - frustumCornersWS[6]).Length();
                diagonalLength += 2; //Without this, the shadow map isn't big enough in the world.
                float worldsUnitsPerTexel = diagonalLength / (float)ShadowMapSize;

                Vector3 vBorderOffset = (new Vector3(diagonalLength, diagonalLength, diagonalLength) -
                                         (_lightBox.Max - _lightBox.Min)) * 0.5f;
                _lightBox.Max += vBorderOffset;
                _lightBox.Min -= vBorderOffset;

                _lightBox.Min /= worldsUnitsPerTexel;
                _lightBox.Min.X = (float)Math.Floor(_lightBox.Min.X);
                _lightBox.Min.Y = (float)Math.Floor(_lightBox.Min.Y);
                _lightBox.Min.Z = (float)Math.Floor(_lightBox.Min.Z);
                _lightBox.Min *= worldsUnitsPerTexel;

                _lightBox.Max /= worldsUnitsPerTexel;
                _lightBox.Max.X = (float)Math.Floor(_lightBox.Max.X);
                _lightBox.Max.Y = (float)Math.Floor(_lightBox.Max.Y);
                _lightBox.Max.Z = (float)Math.Floor(_lightBox.Max.Z);
                _lightBox.Max *= worldsUnitsPerTexel;
            }

            Vector3 boxSize = _lightBox.Max - _lightBox.Min;
            if (boxSize.X == 0 || boxSize.Y == 0 || boxSize.Z == 0)
                boxSize = Vector3.One;
            Vector3 halfBoxSize = boxSize * 0.5f;

            // The position of the light should be in the center of the back
            // pannel of the box. 
            Vector3 lightPosition = _lightBox.Min + halfBoxSize;
            //if (index != 0)
            //{
            //    lightPosition.Z = _lightBox.Min.Z;
            //}
            

            // We need the position back in world coordinates so we transform 
            // the light position by the inverse of the lights rotation
            lightPosition = Vector3.Transform(lightPosition,
                                              Matrix.Invert(lightRotation));

            // Create the view matrix for the light
            Matrix lightView = Matrix.CreateLookAt(lightPosition,
                                                   lightPosition - lightDir,
                                                   cameraUpVector);

            // Create the projection matrix for the light
            // The projection is orthographic since we are using a directional light
            Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y,
                                                               -boxSize.Z, boxSize.Z);

            LightViewProjection returnValue;
            returnValue.LightView = lightView;
            returnValue.LightProjection = lightProjection;

            return returnValue;
        }
    }
}
