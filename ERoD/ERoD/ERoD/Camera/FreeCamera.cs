using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class FreeCamera : BaseCamera
    {
        public float Speed { set; get; }

        public FreeCamera(ERoD game, float nearPlane, float farPlane, Vector3 position, float speed) 
            : base(game, nearPlane, farPlane)
        {
            Position = position;
            Speed = speed;
        }

        /// <summary>
        /// Moves the Camera forward relative to
        /// the current view, use negative deltaTime
        /// to move backwards.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void MoveForward(float deltaTime)
        {
            Position += World.Forward * (deltaTime * Speed);
        }

        /// <summary>
        /// Moves the Camera to the Right relative to
        /// the current view, use negative deltaTime
        /// to move to the left.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void MoveRight(float deltaTime)
        {
            Position += World.Right * (deltaTime * Speed);
        }

        /// <summary>
        /// Moves the Camera Up relative to
        /// the current view, use negative deltaTime
        /// to move down.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void MoveUp(float deltaTime)
        {
            Position += World.Up * (deltaTime * Speed);
        }

        /// <summary>
        /// Moves the Camera Up relative to
        /// the world, use negative deltaTime
        /// to move down.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void MoveUpR(float deltaTime)
        {
            Position += new Vector3(0, (deltaTime * Speed), 0);
        }

        float yaw;
        float pitch;

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            GamePadState gamePadState = ((ERoD)Game).GamePadState;

            yaw += gamePadState.ThumbSticks.Right.X * -1.5f * deltaTime;
            pitch += gamePadState.ThumbSticks.Right.Y * 1.5f * deltaTime;
            //Turn based on gamepad input.
            Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

            //Move based on gamepad input.
            MoveForward(gamePadState.ThumbSticks.Left.Y * deltaTime);
            MoveRight(gamePadState.ThumbSticks.Left.X * deltaTime);
            if (gamePadState.IsButtonDown(Buttons.LeftStick))
            {
                MoveUpR(deltaTime);
            }
            if (gamePadState.IsButtonDown(Buttons.RightStick))
            {
                MoveUpR(-deltaTime);
            }
            world = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            view = Matrix.Invert(World);

            base.Update(gameTime);
        }
    }
}
