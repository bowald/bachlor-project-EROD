using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class StartMenu : DrawableGameComponent
    {
        public enum MenuState
        {
            MENU,
            SELECT_PLAYERS,
            START_GAME,
            EXIT_GAME
        }

        public class MenuItem
        {
            public MenuItem(int x, int y, int width, int height, Texture2D texture, Texture2D hilight)
            {
                normal = texture;
                this.hilight = hilight;
                Rect = new Rectangle(x, y, width, height);
            }

            public bool Selected = false;
            public Rectangle Rect;

            private Texture2D hilight, normal;
            public Texture2D Texture
            {
                get
                {
                    return Selected ? hilight : normal;
                }
            }

            public delegate MenuState PressButton();
            public PressButton pressFunction;
            public MenuState Press() 
            {
                return pressFunction();
            }
        }

        public SpriteBatch spriteBatch;

        private MenuState currentState;

        private MenuItem playItem;
        private MenuItem exitItem;

        // Curently selected item
        private int selectedIndex = 0;
        private const int numberOfChoices = 2;
        private MenuItem[] selectableItems = new MenuItem[numberOfChoices];

        // Controller input
        private GamePadState CurrentGamePadState, LastGamePadState;

        public StartMenu(Game game) : base(game)
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            int width = Game.GraphicsDevice.Viewport.Width;
            int height = Game.GraphicsDevice.Viewport.Height;

            int itemSizeX = width / 6;
            int itemSizeY = height / 8;


            int centerX = width / 2 - itemSizeX / 2;
            int centerY = height / 2 - itemSizeY / 2;
            
            playItem = new MenuItem(centerX
                , centerY - (int)(0.07f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/play")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/play_hilight")
                );
            playItem.pressFunction = (() => MenuState.START_GAME);

            exitItem = new MenuItem(centerX
                , centerY + (int)(0.07f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/exit")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/exit_hilight")
                );
            exitItem.pressFunction = (() => MenuState.EXIT_GAME);

            playItem.Selected = true;
            selectableItems[0] = playItem;
            selectableItems[1] = exitItem;
        }

        public MenuState Update(GameTime gameTime)
        {
            CurrentGamePadState = GamePad.GetState(PlayerIndex.One);

            if (CurrentGamePadState.IsButtonDown(Buttons.DPadUp) && !LastGamePadState.IsButtonDown(Buttons.DPadUp))
            {
                selectableItems[selectedIndex].Selected = false;
                selectedIndex--;
                //selectedIndex = selectedIndex >= 0 ? selectedIndex : numberOfChoices - 1; // Looping
                selectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
                selectableItems[selectedIndex].Selected = true;
            }

            if (CurrentGamePadState.IsButtonDown(Buttons.DPadDown) && !LastGamePadState.IsButtonDown(Buttons.DPadDown))
            {
                selectableItems[selectedIndex].Selected = false;
                selectedIndex++;
                //selectedIndex = selectedIndex < numberOfChoices ? selectedIndex : 0; // Looping
                selectedIndex = selectedIndex < numberOfChoices ? selectedIndex : numberOfChoices - 1;
                selectableItems[selectedIndex].Selected = true;
            }

            if (CurrentGamePadState.IsButtonDown(Buttons.A) && !LastGamePadState.IsButtonDown(Buttons.A))
            {
                currentState = selectableItems[selectedIndex].Press();
                
                if (currentState == MenuState.MENU)
                {
                    // Show start menu
                }
                else if (currentState == MenuState.SELECT_PLAYERS)
                {
                    // Show select players menu
                }
            }

            if (CurrentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                currentState = MenuState.EXIT_GAME;
            }

            LastGamePadState = CurrentGamePadState;
            return currentState;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(playItem.Texture, playItem.Rect, Color.White);
            spriteBatch.Draw(exitItem.Texture, exitItem.Rect, Color.White);
            spriteBatch.End();
        }
    }
}
