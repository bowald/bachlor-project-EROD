using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class ResultScreen : DrawableGameComponent
    {
        public enum MenuState
        {
            SHOW_RESULT,
            RETURN_TO_START_MENU
        }

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
            private Vector2 fontOrigin;
            private string message;
            public string Message
            {
                get { return message; }
                set
                {
                    message = value;
                    fontOrigin = Font.MeasureString(message) / 2;
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

        public SpriteBatch spriteBatch;

        private MenuState currentState = MenuState.SHOW_RESULT;

        private ImageItem Background;
        private TextItem WinnerName;
        private TextItem WinnerTime;
        private TextItem BackMessage;

        private PlayerIndex[] PlayerIndexes = new PlayerIndex[4] { PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four };
        private PlayerIndex WinnerPlayerIndex;

        // Controller input
        private GamePadState CurrentGamePadState, LastGamePadState;

        public ResultScreen(Game game, int winnerIndex, float winnerTime)
            : base(game)
        {
            WinnerPlayerIndex = PlayerIndexes[winnerIndex];

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            int width = Game.GraphicsDevice.Viewport.Width;
            int height = Game.GraphicsDevice.Viewport.Height;

            Background = new ImageItem((int)(width * 0.25f), (int)(height * 0.25f), (int)(width * 0.5f), (int)(height * 0.5f), Game.Content.Load<Texture2D>("Textures/ResultScreen/background"));

            WinnerName = new TextItem((int)(width * 0.50f), (int)(height * 0.4f), 1.0f, Game.Content.Load<SpriteFont>("Sprites/Lap1"));
            WinnerName.Message = GameConstants.PlayerNames[winnerIndex].ToUpper() + " WON!";
            WinnerName.Color = GameConstants.PlayerColors[winnerIndex];

            WinnerTime = new TextItem((int)(width * 0.50f), (int)(height * 0.55f), 0.4f, Game.Content.Load<SpriteFont>("Sprites/Lap1"));
            WinnerTime.Message = "Total time: " + winnerTime.ToString("0.0") + " seconds!";
            WinnerTime.Color = GameConstants.PlayerColors[winnerIndex];

            BackMessage = new TextItem((int)(width * 0.55f), (int)(height * 0.65f), 0.3f, Game.Content.Load<SpriteFont>("Sprites/Lap1"));
            BackMessage.Message = "Press B to go to the Start Menu";
            BackMessage.Color = GameConstants.PlayerColors[winnerIndex];
        }

        public new MenuState Update(GameTime gameTime)
        {
            CurrentGamePadState = GamePad.GetState(WinnerPlayerIndex);
            if (LastGamePadState == null)
            {
                LastGamePadState = CurrentGamePadState;
            }

            if (CurrentGamePadState.IsButtonDown(Buttons.B) && !LastGamePadState.IsButtonDown(Buttons.B))
            {
                // Return to start menu
                currentState = MenuState.RETURN_TO_START_MENU;
            }

            LastGamePadState = CurrentGamePadState;
            return currentState;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // Draw "background"
            spriteBatch.Draw(Background.Texture, Background.Rect, Color.White);

            spriteBatch.End();
            // Draw winners name
            WinnerName.Draw(spriteBatch);
            
            // Draw winners time
            WinnerTime.Draw(spriteBatch);

            // Draw "back to menu text"
            BackMessage.Draw(spriteBatch);
        }
    }
}
