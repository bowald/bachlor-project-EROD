using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class BasicSSAO_SC2 : BasicPostProcess
    {
        private float OcclusionRadious;
        private float FullOcclusionThreshold;
        private float NoOcclusionThreshold;
        private float OcclusionPower;
        //private Vector3[] SamplePoints = { new Vector3(0.53812504f, 0.18565957f, -0.43192f), new Vector3(0.13790712f, 0.24864247f, 0.44301823f), new Vector3(0.33715037f, 0.56794053f, -0.005789503f), new Vector3(-0.6999805f, -0.04511441f, -0.0019965635f), new Vector3(0.06896307f, -0.15983082f, -0.85477847f), new Vector3(0.056099437f, 0.006954967f, -0.1843352f), new Vector3(-0.014653638f, 0.14027752f, 0.0762037f), new Vector3(0.010019933f, -0.1924225f, -0.034443386f), new Vector3(-0.35775623f, -0.5301969f, -0.43581226f), new Vector3(-0.3169221f, 0.106360726f, 0.015860917f), new Vector3(0.010350345f, -0.58698344f, 0.0046293875f), new Vector3(-0.08972908f, -0.49408212f, 0.3287904f), new Vector3(0.7119986f, -0.0154690035f, -0.09183723f), new Vector3(-0.053382345f, 0.059675813f, -0.5411899f), new Vector3(0.035267662f, -0.063188605f, 0.54602677f), new Vector3(-0.47761092f, 0.2847911f, -0.0271716f) };
        private Vector3[] SamplePoints = { new Vector3(-0.13657719f, 0.30651027f, 0.16118456f), new Vector3(-0.14714938f, 0.33245975f, -0.113095455f), new Vector3(0.030659059f, 0.27887347f, -0.7332209f), new Vector3(0.009913514f, -0.89884496f, 0.07381549f), new Vector3(0.040318526f, 0.40091f, 0.6847858f), new Vector3(0.22311053f, -0.3039437f, -0.19340435f), new Vector3(0.36235332f, 0.21894878f, -0.05407306f), new Vector3(-0.15198798f, -0.38409665f, -0.46785462f), new Vector3(-0.013492276f, -0.5345803f, 0.11307949f), new Vector3(-0.4972847f, 0.037064247f, -0.4381323f), new Vector3(-0.024175806f, -0.008928787f, 0.17719103f), new Vector3(0.694014f, -0.122672155f, 0.33098832f) };

  

        private Texture2D randomTexture;

        public BasicSSAO_SC2(ERoD game, float OcclusionRadious, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower)
            : base(game)
        {
            this.OcclusionRadious = OcclusionRadious;
            this.FullOcclusionThreshold = FullOcclusionThreshold;
            this.NoOcclusionThreshold = NoOcclusionThreshold;
            this.OcclusionPower = OcclusionPower;

            UsesVertexShader = true;
            newSceneSurfaceFormat = SurfaceFormat.Vector4; //kolla
        }


        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/SSAO_SC2");

            }

            effect.CurrentTechnique = effect.Techniques["SSAO"];

            effect.Parameters["HalfPixel"].SetValue(HalfPixel);
            effect.Parameters["OcclusionRadious"].SetValue(OcclusionRadious);
            effect.Parameters["FullOcclusionThreshold"].SetValue(FullOcclusionThreshold);
            effect.Parameters["NoOcclusionThreshold"].SetValue(NoOcclusionThreshold);
            effect.Parameters["OcclusionPower"].SetValue(OcclusionPower);
            effect.Parameters["CameraSize"].SetValue(new Vector2(camera.Viewport.Width, camera.Viewport.Height));
            effect.Parameters["SSAOSamplePoints"].SetValue(SamplePoints);
            effect.Parameters["DepthMap"].SetValue(DepthBuffer);
            effect.Parameters["ViewInverse"].SetValue(Matrix.Invert(camera.View));
            effect.Parameters["SidesLengthVS"].SetValue(new Vector2(camera.TanFovy * camera.AspectRatio, -camera.TanFovy));
            effect.Parameters["FarPlane"].SetValue(camera.FarPlane);

            //Game.GraphicsDevice.BlendState = BlendState.Opaque;
            base.Draw(gameTime);

        }
    }
}
