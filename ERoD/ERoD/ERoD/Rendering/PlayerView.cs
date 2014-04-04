using BEPUphysics.Entities;
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

        private Ship ship;

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
            set { viewport = value; }
        }

        public PlayerView(Game game, Viewport viewport, int index)
            : base(game)
        {
            this.Index = indexes[index];
            this.viewport = viewport;
            freeCamera = new FreeCamera(game, 0.1f, 7000.0f, new Vector3(25f, 150.0f, 25f), 270.0f);
            freeCamera.Initialize(Viewport);
            FreeCameraActive = true;
        }

        public void SetChaseTarget(Ship ship)
        {
            this.ship = ship;
            chaseCamera = new ChaseCamera(ship.Entity,
                new BEPUutilities.Vector3(0.0f, 0.0f, 0.0f), true, 25.0f, 0.1f, 3000.0f, Game);
            chaseCamera.Initialize(Viewport);
            FreeCameraActive = false;
        }

        public override void Update(GameTime gameTime)
        {
            GamePadState = GamePad.GetState(Index);

            ship.Update(gameTime, GamePadState);
            chaseCamera.Update(gameTime);
            freeCamera.Update(gameTime, GamePadState);

            LastGamePadState = GamePadState;
        }
    }
}
