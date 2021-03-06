﻿using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class PlayerView : GameComponent
    {
        private PlayerIndex[] indexes = new PlayerIndex[4] { PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four };
        public PlayerIndex Index;
        private int index;
        private Ship ship;
        public Ship Ship
        {
            get
            {
                return ship;
            }
        }

        public HUD UserInterface;

        public PostProcessingManager Manager;

        public GamePadState GamePadState { get; set; }
        public GamePadState LastGamePadState { get; set; }
        
        public bool FreeCameraActive = false;
        private FreeCamera freeCamera;
        private ChaseCamera chaseCamera;
        public ICamera Camera
        {
            get
            {
                if (FreeCameraActive)
                {
                    return freeCamera;
                }
                return chaseCamera;
            }
        }

        private Viewport viewport;
        public Viewport Viewport
        {
            get { return viewport; }
            set 
            { 
                viewport = value;
                if (freeCamera != null)
                {
                    freeCamera.Initialize(value);
                }
                if (chaseCamera != null)
                {
                    chaseCamera.Initialize(value);
                }
            }
        }

        public PlayerView(Game game, Viewport viewport, int index)
            : base(game)
        {
            this.Index = indexes[index];
            this.index = index;
            this.viewport = viewport;
            freeCamera = new FreeCamera(game, 0.1f, 7000.0f, new Vector3(25f, 150.0f, 25f), 270.0f);
            freeCamera.Initialize(Viewport);
            FreeCameraActive = true;
        }

        public void initializeHUD(Player player)
        {
            UserInterface = new HUD(Game, player, viewport);
        }

        public void SetChaseTarget(Ship ship)
        {
            this.ship = ship;
            chaseCamera = new ChaseCamera(ship.Entity,
                new BEPUutilities.Vector3(0.0f, 3.0f, 0.0f), true, 25.0f, 0.1f, 3000.0f, Game);
            chaseCamera.Initialize(Viewport);
            FreeCameraActive = false;
        }

        public override void Update(GameTime gameTime)
        {
            GamePadState = GamePad.GetState(Index);

            // Only listen to controllers in game state.
            if ((Game as ERoD).CurrentState == ERoD.GameState.GAME_OVER)
            {
                GamePadState = new GamePadState();
            }

            // chaseCamera must be updated after the ship


            ship.Update(gameTime, GamePadState, index);
            chaseCamera.Update(gameTime);
            freeCamera.Update(gameTime, GamePadState);

            LastGamePadState = GamePadState;
        }
    }
}
