using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class CrepuscularRays : AdvancedPostProcess
    {
        public LightSourceMask lsMask;
        public LightSceneMask mask;
        public LightRay rays;
        public BrightPass bp;
        public SceneBlend blend;

        public string lightSourceasset
        {
            get { return lsMask.lightSourceasset; }
            set { lsMask.lightSourceasset = value; }
        }
        public Vector3 lightSource
        {
            set
            {
                lsMask.lighSourcePos = value;
                rays.lightSourcePos = value;
            }
            get
            {
                return lsMask.lighSourcePos;
            }
        }

        public float LightSourceSize
        {
            set { lsMask.lightSize = value; }
        }

        public float Density
        {
            get { return rays.Density; }
            set { rays.Density = value; }
        }

        public float Decay
        {
            get { return rays.Decay; }
            set { rays.Decay = value; }
        }

        public float Weight
        {
            get { return rays.Weight; }
            set { rays.Weight = value; }
        }

        public float Exposure
        {
            get { return rays.Exposure; }
            set { rays.Exposure = value; }
        }

        public CrepuscularRays(ERoD game, Vector3 lightSourcePos, string lightSourceImage, float lightSourceSize, float density, float decay, float weight, float exposure, float brightThreshold)
            : base(game)
        {
            lsMask = new LightSourceMask(game, lightSourcePos, lightSourceImage, lightSourceSize);
            mask = new LightSceneMask(game, lightSourcePos);
            rays = new LightRay(game, lightSourcePos, density, decay, weight, exposure);
            bp = new BrightPass(game, brightThreshold);
            blend = new SceneBlend(game);

            //AddPostProcess(lsMask);
            //AddPostProcess(mask);
            //AddPostProcess(rays);
            //AddPostProcess(bp);
            //AddPostProcess(blend);
        }

    }
}