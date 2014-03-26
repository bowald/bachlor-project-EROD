using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public interface ILight
    {
        Vector3 Position { get; set; }
        Color Color { get; set; }
        float Intensity { get; set; }
        Matrix View { get; }
        Matrix Projection { get; }
        bool CastShadow { get; set; }
        ShadowRenderer.CascadeShadowMapEntry ShadowMapEntry { get; set; }
    }

    public interface IPointLight : ILight
    {
        float Radius { get; set; }
    }

    public interface IDirectionalLight : ILight
    {
        Vector3 Target { get; set; }
        Vector3 Direction { get; }
        float ShadowDistance { get; }
    }
}
