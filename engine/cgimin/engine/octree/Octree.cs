using cgimin.engine.camera;
using cgimin.engine.helpers;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Engine.cgimin.engine.octree
{
    public class Octree
    {
        private static int drawCountStatistic;

        private static int maxIterationDepth = 5;

        internal List<Octree> children;

        internal Vector3 bMin;
        internal Vector3 bMax;

        internal Vector3 mid;
        internal float midRadius;

        internal List<OctreeEntity> enteties;

        private int iteration;

        private static List<OctreeEntity> transparentList = new List<OctreeEntity>();

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
                if (enteties == null) enteties = new List<OctreeEntity>();
                enteties.Add(entity);
            }

            // extracting position from object transform matrix
            Vector3 pos = new Vector3(entity.Transform.M41, entity.Transform.M42, entity.Transform.M43);
            float radius = entity.Object3d.radius;

            if (Helpers.SphereAARectangleIntersect(pos, radius, bMin, bMax))
            {

                if (iteration == maxIterationDepth)
                {
                    // only add entity when at max iteration depth
                    if (enteties == null) enteties = new List<OctreeEntity>();
                    enteties.Add(entity);
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

 

        public void Draw()
        {

            if (iteration == 1)
            {
                transparentList.Clear();

                drawCountStatistic = 0;
                int len = enteties.Count;
                for (int i = 0; i < len; i++) enteties[i].drawn = false;
            }

            if (iteration == maxIterationDepth)
            {
                int len = enteties.Count;
                for (int i = 0; i < len; i++)
                {
                    if (enteties[i].drawn == false)
                    {
                        enteties[i].drawn = true;

                        if (!enteties[i].Material.isTransparent)
                        {
                            enteties[i].Object3d.Transformation = enteties[i].Transform;
                            enteties[i].Material.DrawWithSettings(enteties[i].Object3d, enteties[i].MaterialSetting);
                        }
                        else
                        {
                            transparentList.Add(enteties[i]);
                        }

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
                        children[i].Draw();
                    }
                }
            }

            if (iteration == 1)
            {
                Console.WriteLine(transparentList.Count);

                // Alle transparentren Objekte zeichnen
                foreach (OctreeEntity transEntity in transparentList)
                {
                    transEntity.distToCam = (new Vector3(transEntity.Transform.M41, transEntity.Transform.M42, transEntity.Transform.M43) - Camera.Position).Length;
                }

                // Sortiert die Liste nach 'distToCam'
                transparentList.Sort((x, y) => y.distToCam.CompareTo(x.distToCam));

                foreach (OctreeEntity transEntity in transparentList)
                {
                    transEntity.Object3d.Transformation = transEntity.Transform;
                    transEntity.Material.DrawWithSettings(transEntity.Object3d, transEntity.MaterialSetting);
                }

            }
            // Transformation.M41, Transformation.M42, Transformation.M43

        }


    }
}
