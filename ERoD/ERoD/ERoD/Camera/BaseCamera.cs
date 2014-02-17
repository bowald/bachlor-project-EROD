using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class BaseCamera : GameComponent, ICamera
    {

        Matrix ICamera.View
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        Matrix ICamera.Projection
        {
            get { throw new NotImplementedException(); }
        }

        Vector3 ICamera.Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        Quaternion ICamera.Rotation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        Matrix ICamera.World
        {
            get { throw new NotImplementedException(); }
        }
    }
}
