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
            EXIT_GAME,
            START_GAME_1,
            START_GAME_2,
            START_GAME_3,
            START_GAME_4
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

        // Curently selected item
        private int selectedIndex = 0;
        private int numberOfChoices;
        private MenuItem[] selectableItems;

        // Start menu items
        private const int numberOfChoicesMenu = 2;
        private MenuItem[] selectableItemsMenu = new MenuItem[numberOfChoicesMenu];

        // Select Players items
        private const int numberOfChoicesPlayers = 5;
        private MenuItem[] selectableItemsPlayers = new MenuItem[numberOfChoicesPlayers];

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

            MenuItem addItem;

            #region StartMenu

            addItem = new MenuItem(centerX
                , centerY - (int)(0.07f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/play")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/play_hilight")
                );
            addItem.pressFunction = (() => MenuState.SELECT_PLAYERS);
            addItem.Selected = true;
            selectableItemsMenu[0] = addItem;

            addItem = new MenuItem(centerX
                , centerY + (int)(0.07f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/exit")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/exit_hilight")
                );
            addItem.pressFunction = (() => MenuState.EXIT_GAME);
            selectableItemsMenu[1] = addItem;

            #endregion

            itemSizeX = width / 6;
            itemSizeY = height / 7;

            #region Select Players

            addItem = new MenuItem(centerX
                , centerY - (int)(0.25f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/exit")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/exit_hilight")
                );
            addItem.Selected = true;
            addItem.pressFunction = (() => MenuState.MENU);
            selectableItemsPlayers[0] = addItem;

            addItem = new MenuItem(centerX
                , centerY - (int)(0.125f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/players_one")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/players_one_hilight")
                );
            addItem.pressFunction = (() => MenuState.START_GAME_1);
            selectableItemsPlayers[1] = addItem;

            addItem = new MenuItem(centerX
                , centerY - (int)(0.0f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/players_two")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/players_two_hilight")
                );
            addItem.pressFunction = (() => MenuState.START_GAME_2);
            selectableItemsPlayers[2] = addItem;

            addItem = new MenuItem(centerX
                , centerY + (int)(0.125f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/players_three")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/players_three_hilight")
                );
            addItem.pressFunction = (() => MenuState.START_GAME_3);
            selectableItemsPlayers[3] = addItem;

            addItem = new MenuItem(centerX
                , centerY + (int)(0.25f * height)
                , itemSizeX
                , itemSizeY
                , Game.Content.Load<Texture2D>("Textures/StartMenu/players_four")
                , Game.Content.Load<Texture2D>("Textures/StartMenu/players_four_hilight")
                );
            addItem.pressFunction = (() => MenuState.START_GAME_4);
            selectableItemsPlayers[4] = addItem;

            #endregion

            numberOfChoices = numberOfChoicesMenu;
            selectableItems = selectableItemsMenu;
        }

        public new MenuState Update(GameTime gameTime)
        {
            CurrentGamePadState = GamePad.GetState(PlayerIndex.One);

            bool pressed = false;
            if (CurrentGamePadState.IsButtonDown(Buttons.DPadUp) && !LastGamePadState.IsButtonDown(Buttons.DPadUp))
            {
                selectableItems[selectedIndex].Selected = false;
                selectedIndex--;
                //selectedIndex = selectedIndex >= 0 ? selectedIndex : numberOfChoices - 1; // Looping
                selectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
                selectableItems[selectedIndex].Selected = true;
                pressed = true;
            }

            if (CurrentGamePadState.IsButtonDown(Buttons.DPadDown) && !LastGamePadState.IsButtonDown(Buttons.DPadDown))
            {
                selectableItems[selectedIndex].Selected = false;
                selectedIndex++;
                //selectedIndex = selectedIndex < numberOfChoices ? selectedIndex : 0; // Looping
                selectedIndex = selectedIndex < numberOfChoices ? selectedIndex : numberOfChoices - 1;
                selectableItems[selectedIndex].Selected = true;
                pressed = true;
            }

            if (CurrentGamePadState.IsButtonDown(Buttons.A) && !LastGamePadState.IsButtonDown(Buttons.A))
            {
                currentState = selectableItems[selectedIndex].Press();
                
                if (currentState == MenuState.MENU)
                {
                    // Show start menu
                    numberOfChoices = numberOfChoicesMenu;
                    selectableItems = selectableItemsMenu;
                }
                else if (currentState == MenuState.SELECT_PLAYERS)
                {
                    // Show select players menu
                    numberOfChoices = numberOfChoicesPlayers;
                    selectableItems = selectableItemsPlayers;
                }
                pressed = true;
            }

            if (pressed)
            {
                SoundManager.MenuSelection.Play();
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
            for (int i = 0; i < selectableItems.Length; i++)
            {
                spriteBatch.Draw(selectableItems[i].Texture, selectableItems[i].Rect, Color.White);
            }
            spriteBatch.End();
        }
    }
}
