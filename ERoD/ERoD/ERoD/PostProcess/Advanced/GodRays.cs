using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class GodRays : AdvancedPostProcess
    {

        //http://http.developer.nvidia.com/GPUGems3/gpugems3_ch13.html
        //http://xnauk-randomchaosblogarchive.blogspot.se/2012/07/crepuscular-god-rays-and-web-ui-sample.html

        //God Ray Constants
        //Density permit control over the separation between samples
        //Decay (for the range [0, 1]) dissipates each sample's contribution as the ray progresses away from the light source.
        //Weight controls the intensity of each sample.
        //Exposure controls the overall intensity.

        //128 Samples, defined in LightRays-shader

        //Basic Post Processes
        private LightSource Source;
        private SceneMask SceneMask;
        private LightRays Rays;
        //private BloomMask BloomMask; //I think its looks better without the bloomMask
        private Blend Blend;


        public GodRays(ERoD game, Vector3 sunPosition, float sunSize, float density, float decay, float weight, float exposure) :
            base(game)
        {

            Source = new LightSource(game, sunPosition, sunSize);
            SceneMask = new SceneMask(game, sunPosition);
            Blend = new Blend(game, 1.0f, 1.6f, 1.0f, 1.3f);
            Rays = new LightRays(game, sunPosition, density, decay, weight, exposure);
            //BloomMask = new BloomMask(game, 0.5f);

            AddPostProcess(Source);
            AddPostProcess(SceneMask);
            AddPostProcess(Rays);
            //AddPostProcess(BloomMask);
            AddPostProcess(Blend);


        }
    }
}
