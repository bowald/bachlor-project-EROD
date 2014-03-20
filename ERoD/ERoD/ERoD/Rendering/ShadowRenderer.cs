using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class ShadowRenderer
    {
        /// <summary>
        /// This struct holds all information to render the shadows
        /// </summary>
        public class CascadeShadowMapEntry
        {
            public RenderTarget2D Texture;
            public Matrix[] LightViewProjectionMatrices = new Matrix[NUM_CSM_SPLITS];
            public Vector2[] LightClipPlanes = new Vector2[NUM_CSM_SPLITS];
        }

        private const int CASCADE_SHADOW_RESOLUTION = 1024;

        private const int NUM_CSM_SPLITS = 3;

        //temporary variables to help on cascade shadow maps
        float[] splitDepths = new float[NUM_CSM_SPLITS + 1];
        Vector3[] frustumCornersWS = new Vector3[8];
        Vector3[] frustumCornersVS = new Vector3[8];
        Vector3[] splitFrustumCornersVS = new Vector3[8];
        Vector2[] lightClipPlanes = new Vector2[NUM_CSM_SPLITS];
        Vector3[] farFrustumCornersVS = new Vector3[4];

        public RenderTarget2D[] shadowMaps = new RenderTarget2D[3];
        public RenderTarget2D shadowOcclusion;

        private DeferredRenderer renderer;

        private List<CascadeShadowMapEntry> cascadeShadowMaps = new List<CascadeShadowMapEntry>();
        private const int NUM_CSM_SHADOWS = 1;

        private int currentFreeCascadeShadowMap;

        public ShadowRenderer(DeferredRenderer renderer)
        {
            this.renderer = renderer;
            for (int i = 0; i < NUM_CSM_SHADOWS; i++)
            {
                CascadeShadowMapEntry entry = new CascadeShadowMapEntry();
                entry.Texture = new RenderTarget2D(renderer.GraphicsDevice, CASCADE_SHADOW_RESOLUTION * NUM_CSM_SPLITS,
                                                   CASCADE_SHADOW_RESOLUTION, false, SurfaceFormat.Single,
                                                   DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
                cascadeShadowMaps.Add(entry);

            }
            for (int i = 0; i < shadowMaps.Length; i++)
            {
                shadowMaps[i] = new RenderTarget2D(renderer.GraphicsDevice, CASCADE_SHADOW_RESOLUTION,
                                                   CASCADE_SHADOW_RESOLUTION, false, SurfaceFormat.Single,
                                                   DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);

            }

            // Create the shadow occlusion texture using the same dimensions as the backbuffer
            shadowOcclusion = new RenderTarget2D(renderer.GraphicsDevice, renderer.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                 renderer.GraphicsDevice.PresentationParameters.BackBufferHeight, false,
                                                 SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 
                                                 0, RenderTargetUsage.DiscardContents);
        }

        public void InitFrame()
        {
            currentFreeCascadeShadowMap = 0;
        }

        /// <summary>
        /// Returns an unused cascade shadow map, or null if we run out of SMs
        /// </summary>
        /// <returns></returns>
        public CascadeShadowMapEntry GetFreeCascadeShadowMap()
        {
            if (currentFreeCascadeShadowMap < cascadeShadowMaps.Count)
            {
                return cascadeShadowMaps[currentFreeCascadeShadowMap++];
            }
            return null;
        }

        /// <summary>
        /// Generate the cascade shadow map for a given directional light
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="meshes"></param>
        /// <param name="light"></param>
        /// <param name="cascadeShadowMap"></param>
        /// <param name="camera"></param>
        public void GenerateShadowTextureDirectionalLight(Game game, IDirectionalLight light, ICamera camera, GameTime gameTime)
        {
            //bind the render target
            renderer.GraphicsDevice.SetRenderTarget(light.CascadedShadowMap.Texture);
            //clear it to white, ie, far far away
            renderer.GraphicsDevice.Clear(Color.White);
            renderer.GraphicsDevice.BlendState = BlendState.Opaque;
            renderer.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Get the corners of the frustum
            camera.Frustum.GetCorners(frustumCornersWS);
            Matrix eyeTransform = camera.View;
            Vector3.Transform(frustumCornersWS, ref eyeTransform, frustumCornersVS);
            for (int i = 0; i < 4; i++)
            {
                farFrustumCornersVS[i] = frustumCornersVS[i + 4];
            }

            float near = camera.NearPlane;
            float far = MathHelper.Min(camera.FarPlane, 1000.0f);//light.ShadowDistance);

            splitDepths[0] = near;
            splitDepths[NUM_CSM_SPLITS] = far;

            //compute each distance the way you like...
            for (int i = 1; i < NUM_CSM_SPLITS; i++)
            {
                splitDepths[i] = near + (far - near) * (float)Math.Pow((i / (float)NUM_CSM_SPLITS), 2);
            }


            Viewport splitViewport = new Viewport();
            Vector3 lightDir = Vector3.Normalize(light.Direction);

            for (int i = 0; i < NUM_CSM_SPLITS; i++)
            {

                light.CascadedShadowMap.LightClipPlanes[i].X = -splitDepths[i];
                light.CascadedShadowMap.LightClipPlanes[i].Y = -splitDepths[i + 1];

                light.CascadedShadowMap.LightViewProjectionMatrices[i] = CreateLightViewProjectionMatrix(lightDir, far, camera, splitDepths[i], splitDepths[i + 1], i);
                Matrix viewProj = light.CascadedShadowMap.LightViewProjectionMatrices[i];

                renderer.DeferredShadowShader.CurrentTechnique = renderer.DeferredShadowShader.Techniques["GenerateShadowMap"];
                renderer.DeferredShadowShader.Parameters["vp"].SetValue(viewProj);

                Console.WriteLine(i);
                // Set the viewport for the current split     
                splitViewport.MinDepth = 0;
                splitViewport.MaxDepth = 1;
                splitViewport.Width = CASCADE_SHADOW_RESOLUTION;
                splitViewport.Height = CASCADE_SHADOW_RESOLUTION;
                splitViewport.X = i * CASCADE_SHADOW_RESOLUTION;
                splitViewport.Y = 0;
                renderer.GraphicsDevice.Viewport = splitViewport;

                foreach (GameComponent component in game.Components)
                {
                    if (component is IDeferredRender)
                    {
                        ((IDeferredRender)component).Draw(gameTime, renderer.DeferredShadowShader);
                    }
                }
            }
        }

        /// <summary>
        /// Renders a texture containing the final shadow occlusion
        /// </summary>
        public void RenderShadowOcclusion(ICamera camera, IDirectionalLight light)
        {
            // Set the device to render to our shadow occlusion texture
            renderer.GraphicsDevice.SetRenderTarget(shadowOcclusion);
            renderer.GraphicsDevice.Clear(Color.White);
            renderer.GraphicsDevice.BlendState = BlendState.Opaque;
            renderer.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Setup the Effect
            renderer.DeferredShadowShader.CurrentTechnique = renderer.DeferredShadowShader.Techniques["GenerateShadowOcclusion"];
            renderer.DeferredShadowShader.Parameters["lightDirection"].SetValue(light.Direction);
            renderer.DeferredShadowShader.Parameters["halfPixel"].SetValue(renderer.halfPixel);
            renderer.DeferredShadowShader.Parameters["DepthTexture"].SetValue(renderer.depthMap);

            Vector2 shadowMapPixelSize = new Vector2(0.5f / light.CascadedShadowMap.Texture.Width, 0.5f / light.CascadedShadowMap.Texture.Height);
            renderer.DeferredShadowShader.Parameters["ShadowMapPixelSize"].SetValue(shadowMapPixelSize);
            renderer.DeferredShadowShader.Parameters["ShadowMapSize"].SetValue(new Vector2(light.CascadedShadowMap.Texture.Width, light.CascadedShadowMap.Texture.Height));
            renderer.DeferredShadowShader.Parameters["ShadowMap"].SetValue(light.CascadedShadowMap.Texture);

            renderer.DeferredShadowShader.Parameters["ClipPlanes"].SetValue(light.CascadedShadowMap.LightClipPlanes);
            renderer.DeferredShadowShader.Parameters["MatLightViewProj"].SetValue(light.CascadedShadowMap.LightViewProjectionMatrices);
            renderer.DeferredShadowShader.Parameters["cameraPosition"].SetValue(camera.Position);
            renderer.DeferredShadowShader.Parameters["cameraTransform"].SetValue(camera.World);

            Vector3 cascadeDistances = Vector3.Zero;
            cascadeDistances.X = light.CascadedShadowMap.LightClipPlanes[0].X;
            cascadeDistances.Y = light.CascadedShadowMap.LightClipPlanes[1].X;
            cascadeDistances.Z = light.CascadedShadowMap.LightClipPlanes[2].X;
            renderer.DeferredShadowShader.Parameters["CascadeDistances"].SetValue(cascadeDistances);

            renderer.DeferredShadowShader.Parameters["g_vShadowMapSize"].SetValue(new Vector2(light.CascadedShadowMap.Texture.Width, light.CascadedShadowMap.Texture.Height));
            renderer.DeferredShadowShader.Parameters["g_bShowSplitColors"].SetValue(false);

            renderer.DeferredShadowShader.CurrentTechnique.Passes[0].Apply();

            // Draw the full screen quad		
            renderer.sceneQuad.Draw(-Vector2.One, Vector2.One);

        }

        /// <summary>
        /// Creates the WorldViewProjection matrix from the perspective of the 
        /// light using the cameras bounding frustum to determine what is visible 
        /// in the scene.
        /// </summary>
        /// <returns>The WorldViewProjection for the light</returns>
        Matrix CreateLightViewProjectionMatrix(Vector3 lightDir, float farPlane, ICamera camera, float minZ, float maxZ, int index)
        {
            for (int i = 0; i < 4; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i + 4] * (minZ / camera.FarPlane);

            for (int i = 4; i < 8; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i] * (maxZ / camera.FarPlane);

            Matrix cameraMat = camera.World;
            Vector3.Transform(splitFrustumCornersVS, ref cameraMat, frustumCornersWS);

            // Matrix with that will rotate in points the direction of the light
            Vector3 cameraUpVector = Vector3.Up;
            if (Math.Abs(Vector3.Dot(cameraUpVector, lightDir)) > 0.9f)
                cameraUpVector = Vector3.Forward;

            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero, -lightDir, cameraUpVector);

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

            bool fixShadowJittering = false;
            if (fixShadowJittering)
            {
                // I borrowed this code from some forum that I don't remember anymore =/
                // We snap the camera to 1 pixel increments so that moving the camera does not cause the shadows to jitter.
                // This is a matter of integer dividing by the world space size of a texel
                float diagonalLength = (frustumCornersWS[0] - frustumCornersWS[6]).Length();
                diagonalLength += 2; //Without this, the shadow map isn't big enough in the world.
                float worldsUnitsPerTexel = diagonalLength / (float)CASCADE_SHADOW_RESOLUTION;

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
            {
                boxSize = Vector3.One;
            }
            Vector3 halfBoxSize = boxSize * 0.5f;

            // The position of the light should be in the center of the back
            // pannel of the box. 
            Vector3 lightPosition = _lightBox.Min + halfBoxSize;
            lightPosition.Z = _lightBox.Min.Z;

            // We need the position back in world coordinates so we transform 
            // the light position by the inverse of the lights rotation
            lightPosition = Vector3.Transform(lightPosition, Matrix.Invert(lightRotation));

            // Create the view matrix for the light
            Matrix lightView = Matrix.CreateLookAt(lightPosition, lightPosition - lightDir, cameraUpVector);

            // Create the projection matrix for the light
            // The projection is orthographic since we are using a directional light
            Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y, -boxSize.Z, 0);


            return lightView * lightProjection;
        }
    }
}
