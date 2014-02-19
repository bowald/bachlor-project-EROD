using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class BaseLight : ILight
    {
        Vector3 ILight.Position
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

        Color ILight.Color
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

        Matrix ILight.View
        {
            get { throw new NotImplementedException(); }
        }

        Matrix ILight.Projection
        {
            get { throw new NotImplementedException(); }
        }
    }
}
