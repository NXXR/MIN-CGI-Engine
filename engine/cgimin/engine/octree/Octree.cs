using cgimin.engine.camera;
using cgimin.engine.helpers;
using OpenTK;
using System;
using System.Collections.Generic;
using cgimin.engine.material;

namespace Engine.cgimin.engine.octree
{
    public class Octree
    {
        private static int drawCountStatistic;

        private const int MaxIterationDepth = 5;

        private readonly List<Octree> children;

        internal Vector3 bMin;
        internal Vector3 bMax;

        internal Vector3 mid;
        internal float midRadius;

        internal List<OctreeEntity> entities;

        private int iteration;

        public Octree(Vector3 boundsMin, Vector3 boundsMax, int iterationDepth = 1)
        {
            iteration = iterationDepth;

            bMin = boundsMin;
            bMax = boundsMax;

            mid = (bMin + bMax) / 2;
            midRadius = (mid - bMax).Length;

            children = new List<Octree>();
            for (int i = 0; i < 8; i++) children.Add(null);
        }


        public void AddEntity(OctreeEntity entity)
        {

            if (iteration == 1)
            {
                if (entities == null) entities = new List<OctreeEntity>();
                entities.Add(entity);
            }

            // extracting position from object transform matrix
            var pos = entity.Transform.ExtractTranslation();
            float radius = entity.Object3d.radius;

            if (Helpers.SphereAARectangleIntersect(pos, radius, bMin, bMax))
            {

                if (iteration == MaxIterationDepth && iteration != 1)
                {
                    // only add entity when at max iteration depth
                    if (entities == null) entities = new List<OctreeEntity>();
                    entities.Add(entity);
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        // i is the index of the sub-bounding box
                        Vector3 dif = (bMax - bMin) / 2;
                        Vector3 bMinSub = bMin;
                        Vector3 bMaxSub = (bMin + bMax) / 2;

                        if (i % 2 == 1) { bMinSub.X += dif.X; bMaxSub.X += dif.X; }
                        if ((i / 2) % 2 == 1) { bMinSub.Y += dif.Y; bMaxSub.Y += dif.Y; }
                        if (i >= 4) { bMinSub.Z += dif.Z; bMaxSub.Z += dif.Z; }

                        // if object is inside the sub-boundary..
                        if (Helpers.SphereAARectangleIntersect(pos, radius, bMinSub, bMaxSub))
                        {
                            // create a new octree for it and add
                            if (children[i] == null) children[i] = new Octree(bMinSub, bMaxSub, iteration + 1);
                            children[i].AddEntity(entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Should only be called on the root of the Octree.
        /// </summary>
        public void DrawSolidMaterials()
        {
            drawCountStatistic = 0;
            int len = entities.Count;
            for (int i = 0; i < len; i++) entities[i].drawn = false;
            
            CheckForDraw();
            
            MaterialManager.DrawAllSolidMaterials();
        }

        /// <summary>
        /// Should only be called on the root of the Octree.
        /// </summary>
        public void DrawTransparentMaterials()
        {
            MaterialManager.DrawAllTransparentMaterials();
        }

        private void CheckForDraw()
        {
            if (iteration == MaxIterationDepth)
            {
                int len = entities.Count;
                for (int i = 0; i < len; i++)
                {
                    var entity = entities[i];
                    if (!entity.drawn)
                    {
                        entity.Material.RegisterForDraw(entity);
                        entity.drawn = true;
                        drawCountStatistic++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i] != null && Camera.SphereIsInFrustum(children[i].mid, children[i].midRadius))
                    {
                        children[i].CheckForDraw();
                    }
                }
            }
        }
    }
}
