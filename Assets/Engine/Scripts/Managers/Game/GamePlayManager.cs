using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Quixl
{
    public class GamePlayManager : MonoBehaviour
    {
        private static GamePlayManager singleton;

        public PlanetSettings planetSettings;
        public TribeData[] tribes;
        [Range(-1,50)]
        public int seed;

        public LineRenderer lr;

        Planet p;
        GameObject pObjReference;
        Camera cam;
        Polygon poly;


        private void Start()
        {
            singleton = this;
            pObjReference = Instantiate(planetSettings.planetPrefab, Vector3.zero, Quaternion.identity);
            p = pObjReference.GetComponent<Planet>();
            cam = FindObjectOfType<Camera>();

            Constants.COLOR_GRASS = planetSettings.baseGrassColor;
            Constants.COLOR_SAND = planetSettings.baseSandColor;
            Constants.COLOR_DIRT = planetSettings.baseDirtColor;
            Constants.GLOBAL_FOLIAGE_SCALAR = planetSettings.globalFoliageScalar;
            Constants.GLOBAL_FOAM_EDGE_SCALE = planetSettings.edgeFoamScale;
            Constants.GLOBAL_SAND_EDGE_SCALE = planetSettings.edgeSandScale;
            Generate();
        }      

        public void Generate()
        {
            p.RequestGeneration(planetSettings, tribes, seed);
        }

        public static void SelectPolygonAt(Vector3 pos)
        {
            singleton.SelectPolyAt(pos);
        }
        private void SelectPolyAt(Vector3 pos)
        {
            poly = p.GetClosestPolygon(pos, Planet.PlanetPolygonset.Continental);

            Verticles verts = new Verticles();
            foreach (Vector3 vert in poly.verticles)
                verts += vert * 1.01f;
            lr.transform.LookAt(poly.GetPolygonCenter());
            lr.SetPositions(verts);
        }
    }
}
