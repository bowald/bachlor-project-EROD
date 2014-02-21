//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using BEPUphysics;
//using BEPUutilities;
//using BEPUphysics.BroadPhaseEntries;

//using BVector3 = BEPUutilities.Vector3;

//namespace ERoD
//{
//    class Ground : StaticObject 
//    {
//        StaticMesh mesh = null;
//        public Ground(Model model, AffineTransform transform, Game game) 
//        {
//            BVector3[] vertices;
//            int[] indices;
//            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
//            var mesh = new StaticMesh(vertices, indices, transform);
//            space.Add(mesh);
//            Components.Add(new StaticObject(model, MathConverter.Convert(mesh.WorldTransform.Matrix), this));
//        }

//        //public float getDistance(Ray ray, out RayCastResult hit)
//        //{

//        //    if (space.RayCast(ray, 100, out hit))
//        //    {
//        //        float distance = hit.HitData.T;
//        //        Debug.WriteLine("distance: " + distance);
//        //        return (9.8f - Math.Max(0, distance * (9.8f - f_max) / IDEAL_HEIGHT + f_max)) * entity.Mass;
//        //    }
//        //    Debug.WriteLine("no hit");
//        //}
//    }
//}
