using BEPUphysics.BroadPhaseEntries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class StaticObject : BaseObject
    {
        protected StaticMesh mesh;

        public StaticObject(Model model, StaticMesh mesh, Game game) 
            : base(model, game)
        {
            this.mesh = mesh;
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix transform = ConversionHelper.MathConverter.Convert(mesh.WorldTransform.Matrix);
            transform.Decompose(out scale, out rotation, out position);
            base.Draw(gameTime);
        }
    }
}
