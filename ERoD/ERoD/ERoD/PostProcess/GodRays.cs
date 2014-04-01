using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class GodRays : AdvancedPostProcess
    {

        public LightRay LightRay;
        public Blend Blend;
        public SceneMask SceneMask;
            

        public GodRays(ERoD game, Vector3 lightPos, float density, float decay, float weight, float exposure)
            : base(game)
        {
            Blend = new Blend(game);
            Blend.AditiveBlend = true;
            LightRay = new LightRay(game, lightPos, density, decay, weight, exposure);
            SceneMask = new SceneMask(game);


            AddPostProcess(SceneMask);
            AddPostProcess(LightRay);
            AddPostProcess(Blend);
        }
    }
}
