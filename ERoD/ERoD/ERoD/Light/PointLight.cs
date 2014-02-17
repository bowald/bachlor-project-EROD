using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD.Light
{
    class PointLight : BaseLight, IPointLight
    {
        float IPointLight.Radius
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

        Microsoft.Xna.Framework.Vector3 ILight.Position
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

        Microsoft.Xna.Framework.Color ILight.Color
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

        float ILight.Intensity
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

        Microsoft.Xna.Framework.Matrix ILight.View
        {
            get { throw new NotImplementedException(); }
        }

        Microsoft.Xna.Framework.Matrix ILight.Projection
        {
            get { throw new NotImplementedException(); }
        }
    }
}
