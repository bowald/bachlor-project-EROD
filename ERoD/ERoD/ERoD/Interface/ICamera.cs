using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    interface ICamera
    {
        Matrix View { get; set; }
        Matrix Projection { get; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Matrix World { get; }
        Viewport Viewport { get; set; }
        void Dispose();
    }
}
