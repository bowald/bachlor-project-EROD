using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public interface ICamera
    {
        Matrix View { get; set; }
        Matrix Projection { get; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Matrix World { get; }
        BoundingFrustum Frustum { get; }
        Viewport Viewport { get; set; }
        float NearPlane { get; set; }
        float FarPlane { get; set; }
    }
}
