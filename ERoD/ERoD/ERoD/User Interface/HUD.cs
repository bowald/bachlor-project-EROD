using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class HUD : DrawableGameComponent
    {
        public class ImageItem
        {
            public ImageItem(int x, int y, int width, int height, Texture2D texture)
            {
                this.texture = texture;
                Rect = new Rectangle(x, y, width, height);
            }

            public Rectangle Rect;

            private Texture2D texture;
            public Texture2D Texture
            {
                get
                {
                    return texture;
                }
            }
        }

        public class TextItem
        {
            public TextItem(int x, int y, float scale, SpriteFont font)
            {
                Font = font;
                Scale = scale;
                Position = new Vector2(x, y);
                Color = Color.Black;
            }

            public Vector2 Position;
            public float Scale;
            public SpriteFont Font;
            protected Vector2 fontOrigin = Vector2.Zero;
            protected string message;
            public virtual string Message
            {
                get { return message; }
                set
                {
                    message = value;
                }
            }
            public Color Color;

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(Font, Message, Position, Color, 0, fontOrigin, Scale, SpriteEffects.None, 0.5f);
                spriteBatch.End();
            }
        }

        public class BarComponent{
            private SpriteBatch spriteBatch;
            private GraphicsDevice graphicsDevice;
 
            private Vector2 position;
            private Vector2 dimension;
 
            private float valueMax;
            private float valueCurrent;
 
            private bool enabled;

            public BarComponent(int x, int y, Vector2 dimension, float valueMax, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
               {
               this.position = new Vector2(x,y);
               this.dimension = dimension;
               this.valueMax = valueMax;
               this.spriteBatch = spriteBatch;
               this.graphicsDevice = graphicsDevice;
               this.enabled = true;
               }
            public void enable(bool enabled)
               {
               this.enabled = enabled;
               }
            public void update(float valueCurrent)
               {
               this.valueCurrent = valueCurrent;
               }
            public void Draw()
               {
               if (enabled)
                  {
                  float percent = valueCurrent / valueMax;
 
                  Color backgroundColor = new Color(0, 0, 0, 192);
                  Color barColor = new Color(0, 255, 255, 200);
                   if (percent < 0.40)
                  {
                      barColor = Color.Lerp(new Color(255, 255, 0, 200), new Color(255, 0, 0, 200), (1f - (percent / 0.40f)));
                  }
                  else if (percent < 0.75)
                  {
                      barColor = Color.Lerp(barColor, new Color(255, 255, 0, 200), (1f - ((percent - 0.40f) / 0.35f)));
                  }

                  // set size of background and draw it
                  Rectangle backgroundRectangle = new Rectangle();
                  backgroundRectangle.Width = (int)dimension.X;
                  backgroundRectangle.Height = (int)dimension.Y;
                  backgroundRectangle.X = (int)position.X;
                  backgroundRectangle.Y = (int)position.Y;
 
                  Texture2D dummyTexture = new Texture2D(graphicsDevice, 1, 1);
                  dummyTexture.SetData(new Color[] { backgroundColor });
 
                  spriteBatch.Draw(dummyTexture, backgroundRectangle, backgroundColor);
    
                // set size of bar anb draw it
                  backgroundRectangle.Width = (int)(dimension.X) - 2;
                  backgroundRectangle.Height = (int)(dimension.Y * percent) - 2;
                  backgroundRectangle.X = (int)position.X + 1;
                  backgroundRectangle.Y = (int)position.Y + (int)(dimension.Y * (1 -percent) + 1);
 
                  dummyTexture = new Texture2D(graphicsDevice, 1, 1);
                  dummyTexture.SetData(new Color[] { barColor });
 
                  spriteBatch.Draw(dummyTexture, backgroundRectangle, barColor);
                  }
               }
        }

        public class CenteredTextItem : TextItem
        {
            public CenteredTextItem(int x, int y, float scale, SpriteFont font)
                : base(x, y, scale, font)
            {
            }

            public override string Message
            {
                get { return message; }
                set
                {
                    message = value;
                    fontOrigin = Font.MeasureString(message) / 2;
                }
            }
        }

        public SpriteBatch spriteBatch;

        // Text
        private TextItem Laps;
        private TextItem Checkpoints;
        private BarComponent Boost;
        private Player player;

        public HUD(Game game, Player player, Viewport viewport)
            : base(game)
        {
            this.player = player;

            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            int x = viewport.X;
            int y = viewport.Y;
            int width = viewport.Width;
            int height = viewport.Height;
            float scaleX = (float)width / GameConstants.WindowWidth;
            float scaleY = (float)height / GameConstants.WindowHeight;

            SpriteFont Lap1 = Game.Content.Load<SpriteFont>("Sprites/Lap1");

            Vector2 lapstrln = 0.8f * scaleY * Lap1.MeasureString("Lap 1/1");
            Laps = new TextItem(x + (int)(width * 0.99f) - (int)lapstrln.X, y, 0.8f * scaleY, Lap1);
            Laps.Message = "";
            Laps.Color = Color.Firebrick;

            Vector2 checkstrln = 0.4f * scaleY * Lap1.MeasureString("Checkpoint 1/1"); // up to 9
            Checkpoints = new TextItem(x + (int)(width * 0.99f) - (int)checkstrln.X, y + (int)lapstrln.Y, 0.4f * scaleY, Lap1);
            Checkpoints.Message = "";
            Checkpoints.Color = Color.Firebrick;

            Boost = new BarComponent(x + (int)(width * 0.05f), y + (int)(height * 0.60f), new Vector2(width * 0.01f, height * 0.3f), GameConstants.BoostMaxTime, spriteBatch, game.GraphicsDevice);
            
        }

        public override void Update(GameTime gameTime)
        {
            Laps.Message = "Lap " + player.Lap.ToString() + "/" + GameConstants.NumberOfLaps;
            Checkpoints.Message = "Checkpoint " + player.LastCheckpoint.ToString() + "/" + (GameConstants.NumberOfCheckpoints + 1);
            Boost.update(player.Boost);

        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            Boost.Draw();
            //spriteBatch.Draw(Background.Texture, Background.Rect, Color.White);

            spriteBatch.End();

            Laps.Draw(spriteBatch);
            Checkpoints.Draw(spriteBatch);
        }
    }
}
