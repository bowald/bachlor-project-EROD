using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class PostProcessor
    {
        // Pixel shader
        public Effect Effect { get; protected set; }
        // Texture to process
        public Texture2D Input { get; set; }

        // GraphicsDevice and SpriteBatch for drawing
        protected GraphicsDevice graphicsDevice;
        protected static SpriteBatch spriteBatch;


        public PostProcessor(Effect Effect, GraphicsDevice graphicsDevice)
        {
            this.Effect = Effect;
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(graphicsDevice);
            this.graphicsDevice = graphicsDevice;
        }

        // Draws the input texture using the pixel shader postprocessor
        public virtual void Draw()
        {
            // Set effect parameters if necessary
            if (Effect.Parameters["ScreenWidth"] != null)
            {
                Effect.Parameters["ScreenWidth"].SetValue(graphicsDevice.Viewport.Width);
            }

            if (Effect.Parameters["ScreenHeight"] != null)
            {
                Effect.Parameters["ScreenHeight"].SetValue(graphicsDevice.Viewport.Height);
            }
            // Initialize the spritebatch and effect
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            Effect.CurrentTechnique.Passes[0].Apply();
            // Draw the input texture
            spriteBatch.Draw(Input, Vector2.Zero, Color.White);
            // End the spritebatch and effect
            spriteBatch.End();
            // Clean up render states changed by the spritebatch
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;
        }
    }
}
