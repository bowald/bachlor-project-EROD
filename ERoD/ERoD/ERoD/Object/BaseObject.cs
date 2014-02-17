using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class BaseObject : DrawableGameComponent, IObject
    {
        Matrix IObject.World
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

        Microsoft.Xna.Framework.Graphics.Model IObject.Model
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

        Microsoft.Xna.Framework.Graphics.Texture2D IObject.Texture
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
    }
}
