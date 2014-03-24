using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{

    public class QuadTreeNode
    {
        public QuadTreeNode Parent { get; internal set; }
        public List<QuadTreeNode> Children { get; private set; }

        public int Level { get; set; }

        public float Height { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public QuadTreeNode()
        {
            Level = 0;
            Height = 1f;
            BoundingBox = new BoundingBox(new Vector3(-0.5f, float.MaxValue, -0.5f), new Vector3(0.5f, float.MinValue, 0.5f));
            Children = new List<QuadTreeNode>();
        }

        public void AddToList(List<QuadTreeNode> nodes)
        {
            nodes.Add(this);
        }

        public void SetHeights(float min, float max)
        {
            var bounds = BoundingBox;
            var update = false;

            if (update |= min < bounds.Min.Y)
                bounds.Min = new Vector3(bounds.Min.X, min, bounds.Min.Z);

            if (update |= max > bounds.Max.Y)
                bounds.Max = new Vector3(bounds.Max.X, max, bounds.Max.Z);

            BoundingBox = bounds;

            if (Parent != null && update)
                Parent.SetHeights(min, max);

        }

        public bool LodSelect(Vector3 eyePosition, ref BoundingFrustum frustum, List<BoundingSphere> spheres, List<QuadTreeNode> nodes)
        {
            /// grab a copy to avoid hitting the _get/_set accessors too many times at runtime 
            var boundingBox = BoundingBox;

            if (!boundingBox.Intersects(spheres[Level]))
                return false;

            /*
            if (!boundingBox.Intersects(frustum))
                return true;
            */
            /// if this is already the most detailed level available
            if (Children.Count == 0 || Level == 0)
            {

                nodes.Add(this);

                return true;
            }
            else
            {
                if (!boundingBox.Intersects(spheres[Level - 1]))
                {
                    nodes.Add(this);
                }
                else
                {
                    var tempNodes = new List<QuadTreeNode>();
                    var allHandled = true;

                    foreach (var child in Children)
                    {
                        allHandled &= child.LodSelect(eyePosition, ref frustum, spheres, tempNodes);
                    }

                    if (allHandled)
                        nodes.AddRange(tempNodes);
                    else
                        nodes.Add(this);

                }

            }

            return true;
        }

        public Vector3 GetCenter()
        {
            return (BoundingBox.Min + BoundingBox.Max) / 2f;
        }

    }

    public class QuadTree
    {
        public QuadTreeNode Root { get; private set; }

        public QuadTree()
        {
        }

        public static QuadTree Build(ref float[] heights, int mapWidth, int mapHeight, ref float[] morphRanges)
        {

            var root = new QuadTreeNode();
            root.Level = morphRanges.Length - 1;

            var stack = new Stack<QuadTreeNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var node = stack.Pop();

                if (node.Level > 0)
                {
                    var boundsCorners = node.BoundingBox.GetCorners();
                    var center = (node.BoundingBox.Min + node.BoundingBox.Max) / 2f;

                    foreach (var cornerIndex in new[] { 4, 5, 1, 0 })
                    {
                        var child = new QuadTreeNode();
                        node.Children.Add(child);
                        child.Parent = node;
                        child.Level = node.Level - 1;

                        var min = Vector3.Min(boundsCorners[cornerIndex], center);
                        min.Y = float.MaxValue;

                        var max = Vector3.Max(boundsCorners[cornerIndex], center);
                        max.Y = float.MinValue;

                        child.BoundingBox = new BoundingBox(min, max);

                        stack.Push(child);
                    }

                }
                else
                {
                    var bounds = node.BoundingBox;
                    var min3 = (bounds.Min + Vector3.One / 2f) * new Vector3(mapWidth, 0, mapHeight);
                    var max3 = (bounds.Max + Vector3.One / 2f) * new Vector3(mapWidth, 0, mapHeight);

                    /// go ahead and calculate the height ranges for this leaf
                    var min = new Point((int)min3.X, (int)min3.Z);
                    var max = new Point((int)max3.X, (int)max3.Z);

                    var minHeight = float.MaxValue;
                    var maxHeight = float.MinValue;

                    for (var j = min.Y; j < max.Y; j++)
                        for (var i = min.X; i < max.X; i++)
                        {
                            var height = heights[j * mapWidth + i];

                            minHeight = MathHelper.Min(minHeight, height);
                            maxHeight = MathHelper.Max(maxHeight, height);
                        }

                    node.SetHeights(minHeight, maxHeight);
                }

            }

            var tree = new QuadTree();
            tree.Root = root;

            return tree;
        }

        public bool LodSelect(Vector3 eyePosition, ref float[] ranges, ref BoundingFrustum frustum, List<QuadTreeNode> nodes)
        {
            var spheres = new List<BoundingSphere>();

            for (var i = 0; i < ranges.Length; i++)
            {
                var sphere = new BoundingSphere(eyePosition, ranges[i]);
                spheres.Add(sphere);
            }
            return Root.LodSelect(eyePosition, ref frustum, spheres, nodes);
        }
    }

}
