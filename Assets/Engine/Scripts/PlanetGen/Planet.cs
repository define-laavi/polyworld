using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quixl
{
    [SelectionBase]
    public class Planet : MonoBehaviour
    {
        #region Parameters
        PlanetSettings ps;
        PolygonSet allPolys,landPolys,waterPolys;
        PlanetMeshConstructor surfaceMesh, waterMesh;
        TribeData[] tribes;
        GameObject sObj, wObj, fObj;
        System.Random r;
        bool busy = false;
        #endregion
    
        public void RequestGeneration(PlanetSettings settings,TribeData[] t, int seed = -1)
        {
            if (!busy)
            {
                if (seed != -1) r = new System.Random(seed);
                else r = new System.Random();
                ps = settings;           
                tribes = t;
                busy = true;
                GameUIManager.StaticSetTrigger("Reset");
                ThreadManager.Add(GeneratePlanet, VisualizePlanet);
            }
        }
        public PolygonSet GetPolygonSet(PlanetPolygonset type)
        {
            switch (type)
            {
                case PlanetPolygonset.All:
                    return allPolys;
                case PlanetPolygonset.Continental:
                    return landPolys;
                case PlanetPolygonset.Water:
                    return waterPolys;
                default:
                    return new PolygonSet();
            }
        }
        public Polygon GetClosestPolygon(Vector3 mousePos,PlanetPolygonset type = PlanetPolygonset.All)
        {
            return GetClosestPolygon(mousePos,GetPolygonSet(type));
        }
        public PolygonSet GetPolygonsInRange(Vector3 position, float radius, PlanetPolygonset type = PlanetPolygonset.All)
        {
            return GetPolygonsInRange(position, radius, GetPolygonSet(type));
        }

        private static Polygon GetClosestPolygon(Vector3 pos, PolygonSet source)
        {
            Polygon p = null;
            float minDist = 99999;
            foreach (Polygon t in source)
            {
                float dist = (t.GetPolygonCenter() - pos).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    p = t;
                }
            }
            return p;
        }
        private static PolygonSet GetPolygonsInRange(Vector3 position, float radius, PolygonSet source)
        {
            PolygonSet ret = new PolygonSet();
            foreach (Polygon p in source)
            {
                foreach (Vector3 vertex in p.verticles)
                {
                    float distanceToSphere = (position - vertex).sqrMagnitude;

                    if (distanceToSphere <= radius * radius)
                    {
                        ret.Add(p);
                        break;
                    }
                }
            }
            return ret;
        }

        #region PlanetModification
        public void Edit_SetTribe(PolygonSet polygons, TribeData tribe)
        {
            foreach (Polygon p in polygons)
                Edit_SetTribe(p, tribe);
        }
        public void Edit_SetTribe(Polygon polygon, TribeData tribe)
        {
            polygon.SetTribe(tribe);
        }
        public void Edit_ChangeBiome(PolygonSet polygons, int biome)
        {
            foreach (Polygon p in polygons)
                Edit_ChangeBiome(p, biome);


        }
        public void Edit_ChangeBiome(Polygon polygon, int biome)
        {
            polygon.ChangeBiome(biome);
        }
        public void Edit_ChangeHeight(PolygonSet polygons, float height)
        {
            foreach (Polygon p in polygons)
                Edit_ChangeHeight(p, height);
        }
        public void Edit_ChangeHeight(Polygon polygon, float height)
        {
            polygon.ChangeHeight(height);
        }
        public void Edit_AddFoliage(PolygonSet polygons)
        {
            foreach (Polygon p in polygons)
                Edit_AddFoliage(p);
        }
        public void Edit_AddFoliage(Polygon p)
        {
            if (p.tribe == null)
            {
                int amount = r.Next(0, ps.foliageAmountPerCell+1);

                for (int i = 0; i < amount; i++)
                {
                    Vector3 pos = p.GetRandomPositionInside(r);
                    Transform b = Instantiate(ps.baseFoliage.GetRandom(r), pos, Quaternion.identity);
                    b.transform.LookAt(Vector3.zero);
                    b.transform.Rotate(-90, 0, 0);

                    b.localScale = new Vector3(ps.baseFoliage.scale, ps.baseFoliage.scale, ps.baseFoliage.scale);
                    b.parent = fObj.transform;
                    b.localRotation *= Quaternion.Euler(0, r.Next(0, 360), 0);

                    p.foliage.Add(b);
                }
            }
            else
            {
                int amount = r.Next(0, p.tribe.tribeFoliageAmountPerCell+1);

                for (int i = 0; i < amount; i++)
                {
                    Vector3 pos = p.GetRandomPositionInside(r);
                    Transform b = Instantiate(p.tribe.tribeFoliage.GetRandom(r), pos, Quaternion.identity);
                    b.transform.LookAt(Vector3.zero);
                    b.transform.Rotate(-90, 0, 0);

                    b.localScale = new Vector3(p.tribe.tribeFoliage.scale, p.tribe.tribeFoliage.scale, p.tribe.tribeFoliage.scale);
                    b.parent = fObj.transform;
                    b.localRotation *= Quaternion.Euler(0, r.Next(0, 360), 0);

                    p.foliage.Add(b);
                }
            }
        }
        #endregion

        #region PlanetGeneration
        private void GeneratePlanet()
        {
            CreateIcosahedron();
            Subdivide(Mathf.CeilToInt(Mathf.Log(ps.cellsCount / 20f, 4)));
            SetNeighbours();
            CreateTerrain();
            Filter();
            SetTribes();
            CalculatePlanetMesh();
        }

        private void CreateIcosahedron()
        {
            allPolys = new PolygonSet();

            float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

            Vector3 v0 = new Vector3(-1, t, 0).normalized;
            Vector3 v1 = new Vector3(1, t, 0).normalized;
            Vector3 v2 = new Vector3(-1, -t, 0).normalized;
            Vector3 v3 = new Vector3(1, -t, 0).normalized;
            Vector3 v4 = new Vector3(0, -1, t).normalized;
            Vector3 v5 = new Vector3(0, 1, t).normalized;
            Vector3 v6 = new Vector3(0, -1, -t).normalized;
            Vector3 v7 = new Vector3(0, 1, -t).normalized;
            Vector3 v8 = new Vector3(t, 0, -1).normalized;
            Vector3 v9 = new Vector3(t, 0, 1).normalized;
            Vector3 v10 = new Vector3(-t, 0, -1).normalized;
            Vector3 v11 = new Vector3(-t, 0, 1).normalized;

            allPolys.Add(new Polygon(v0, v11, v5));
            allPolys.Add(new Polygon(v0, v5, v1));
            allPolys.Add(new Polygon(v0, v1, v7));
            allPolys.Add(new Polygon(v0, v7, v10));
            allPolys.Add(new Polygon(v0, v10, v11));
            allPolys.Add(new Polygon(v1, v5, v9));
            allPolys.Add(new Polygon(v5, v11, v4));
            allPolys.Add(new Polygon(v11, v10, v2));
            allPolys.Add(new Polygon(v10, v7, v6));
            allPolys.Add(new Polygon(v7, v1, v8));
            allPolys.Add(new Polygon(v3, v9, v4));
            allPolys.Add(new Polygon(v3, v4, v2));
            allPolys.Add(new Polygon(v3, v2, v6));
            allPolys.Add(new Polygon(v3, v6, v8));
            allPolys.Add(new Polygon(v3, v8, v9));
            allPolys.Add(new Polygon(v4, v9, v5));
            allPolys.Add(new Polygon(v2, v4, v11));
            allPolys.Add(new Polygon(v6, v2, v10));
            allPolys.Add(new Polygon(v8, v6, v7));
            allPolys.Add(new Polygon(v9, v8, v1));
        }
        private void Subdivide(int level)
        {
            for (int i = 0; i < level; i++)
            {
                var newPolys = new PolygonSet();
                foreach (Polygon poly in allPolys)
                {
                    Vector3 a = poly.verticles & (0);
                    Vector3 b = poly.verticles & (1);
                    Vector3 c = poly.verticles & (2);

                    Vector3 ab = Vector3.Lerp(a, b, 0.5f);
                    Vector3 bc = Vector3.Lerp(b, c, 0.5f);
                    Vector3 ca = Vector3.Lerp(c, a, 0.5f);


                    newPolys += new Polygon(a, ab, ca);
                    newPolys += new Polygon(ab, bc, ca);
                    newPolys += new Polygon(c, ca, bc);
                    newPolys += new Polygon(b, bc, ab);
                }

                allPolys = newPolys;
            }

            foreach (Polygon p in allPolys) p.Normalize();
        }
        private void SetNeighbours()
        {
            foreach (Polygon p1 in allPolys)
            {
                foreach (Polygon p2 in allPolys)
                {
                    if (p1.neighbours.Count == 3)
                        break;

                    if (p2.neighbours.Count == 3)
                        continue;

                    if (p1 == p2)
                        continue;

                    p1.TryAddNeighbour(p2);
                }
                p1.SortNeighbours();
            }
        }
        private void CreateTerrain()
        {
            landPolys = new PolygonSet();
            while (landPolys.Count < ps.cellsCount)
                landPolys += GetPolygonsInRange(new Vector3(r.Next(-30, 30), r.Next(-30, 30), r.Next(-30, 30)).normalized, r.Next(15, 50) / 100f, allPolys);
            Edit_ChangeHeight(landPolys, Constants.POLYGON_HEIGHT_DIFF);
            Edit_ChangeBiome(landPolys, 1);

            waterPolys = allPolys - landPolys;
        }
        private void Filter()
        {
            for(int i = 0; i < 4; i++)
            {
                PolygonSet change = new PolygonSet();
                foreach(Polygon p in waterPolys)
                {
                    if (p.neighbours.Count((x) => x.height > p.height) >= 2) change += p;
                }
                Edit_ChangeBiome(change, 1);
                Edit_ChangeHeight(change, Constants.POLYGON_HEIGHT_DIFF);
                waterPolys -= change;
                landPolys += change;
            }
        }
        private void SetTribes()
        {
            PolygonSet openset = landPolys;
            foreach(TribeData t in tribes)
            {
                PolygonSet p;
                do
                {
                    p = GetPolygonsInRange(new Vector3(r.Next(-30, 30), r.Next(-30, 30), r.Next(-30, 30)).normalized, .30f, openset);
                }
                while (p.Count < 20);
                Edit_SetTribe(p, t);
                openset -= p;
            }
        }
        private void CalculatePlanetMesh()
        {
            surfaceMesh = new PlanetMeshConstructor(allPolys);
            Edit_ChangeHeight(waterPolys, Constants.POLYGON_HEIGHT_DIFF / 2f);
            waterMesh = new PlanetMeshConstructor(waterPolys, true);
            Edit_ChangeHeight(waterPolys, -Constants.POLYGON_HEIGHT_DIFF / 2f);
        }
        #endregion

        #region ObjectGeneration
        private void VisualizePlanet()
        {
            if (sObj != null) Destroy(sObj);
            if (wObj != null) Destroy(wObj);
            if (fObj != null) Destroy(fObj);

            GenerateObjectWithPMC("Surface", out sObj, surfaceMesh, ps.surfaceMaterial, true);
            GenerateObjectWithPMC("Water", out wObj, waterMesh, ps.waterMaterial, false);
            (fObj = new GameObject("Foliage")).transform.parent = this.transform;
            Edit_AddFoliage(landPolys);

            GameUIManager.StaticRemoveTrigger("Reset");
            GameUIManager.StaticSetTrigger("Generated");
            busy = false;
        }

        private void GenerateObjectWithPMC(string name, out GameObject obj, PlanetMeshConstructor planetMeshConstructor, Material material, bool needsPhysics)
        {
            obj = new GameObject(name);

            Mesh m = planetMeshConstructor.GetMesh();

            MeshRenderer surfaceRenderer = obj.AddComponent<MeshRenderer>();
            surfaceRenderer.material = material;
            obj.AddComponent<MeshFilter>().mesh = m;
            if (needsPhysics)
            {
                obj.AddComponent<MeshCollider>().sharedMesh = m;
                obj.AddComponent<Rigidbody>().isKinematic = true;
            }
            obj.transform.SetParent(transform);
        }
        #endregion

        public enum PlanetPolygonset
        {
            All,Continental,Water
        }
    }
}
