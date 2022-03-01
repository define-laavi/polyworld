using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Quixl {
    public class PlanetMeshConstructor
    {
        List<Vector3> vertices;
        List<int> triangles;
        List<Color> colors;
        float heightFactor;
        bool waterMesh;
        static Dictionary<string, float> EvenHeightCorner;

        public PlanetMeshConstructor(PolygonSet allPolys, bool isWater = false)
        {
            waterMesh = isWater;
            if(!waterMesh) EvenHeightCorner = new Dictionary<string, float>();
            vertices = new List<Vector3>();
            triangles = new List<int>();
            colors = new List<Color>();

            Constants.TRIANGLE_SCALE = waterMesh ? Constants.GLOBAL_FOAM_EDGE_SCALE : Constants.GLOBAL_SAND_EDGE_SCALE;

            if(!waterMesh)foreach (Polygon p in allPolys) FeedPolygonVerticesToDictionary(p);
            foreach (Polygon p in allPolys) TriangulatePolygon(p);

        }

        #region BasicMeshFunctions
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color co)
        {
            int vertexIndex = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);

            colors.Add(co);
            colors.Add(co);
            colors.Add(co);

        }
        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color co)
        {
            AddTriangle(a, b, d, co);
            AddTriangle(b, c, d, co);
        }

        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color ca, Color cb,Color cc)
        {
            int vertexIndex = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);

            colors.Add(ca);
            colors.Add(cb);
            colors.Add(cc);

        }
        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color ca,Color cb,Color cc,Color cd)
        {
            AddTriangle(a, b, d, ca,cb,cd);
            AddTriangle(b, c, d, cb,cc,cd);
        }
        #endregion

        public Mesh GetMesh()
        {
            Mesh mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
                colors = colors.ToArray(),
            };

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            mesh.name = "Planet Mesh";
            return mesh;
        }

        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            colors.Clear();
            EvenHeightCorner.Clear();
        }

        public void FeedPolygonVerticesToDictionary(Polygon p)
        {
            foreach (Vector3 v in p.verticles)
            {
                if (EvenHeightCorner.ContainsKey(v.normalized.ToString()))
                {
                    EvenHeightCorner[v.normalized.ToString()] = EvenHeightCorner[v.normalized.ToString()] == p.height ? p.height : 0;
                }
                else
                {
                    EvenHeightCorner.Add(v.normalized.ToString(), p.height);
                }
            }
        }

        public void TriangulatePolygon(Polygon p)
        {
            p.ChangeHeight(0);

            heightFactor = waterMesh ? p.height : (p.height * 3 / 4f + 1 / 4f);

            string pcase = GetPolygonCase(p);
            if (p.height == 1) PolygonCase0(p);
            else if (CompareCases("000000", pcase)) PolygonCase0(p);
            else if (CompareCases("100000", pcase)) PolygonCase1(p, 0);
            else if (CompareCases("010000", pcase)) PolygonCase1(p, 1);
            else if (CompareCases("001000", pcase)) PolygonCase1(p, 2);
            else if (CompareCases("--0100", pcase)) PolygonCase2(p, 0);
            else if (CompareCases("0--010", pcase)) PolygonCase2(p, 1);
            else if (CompareCases("-0-001", pcase)) PolygonCase2(p, 2);
            else if (CompareCases("---101", pcase)) PolygonCase3(p, 0);
            else if (CompareCases("---110", pcase)) PolygonCase3(p, 1);
            else if (CompareCases("---011", pcase)) PolygonCase3(p, 2);
            else if (CompareCases("1--010", pcase)) PolygonCase4(p, 0);
            else if (CompareCases("-1-001", pcase)) PolygonCase4(p, 1);
            else if (CompareCases("--1100", pcase)) PolygonCase4(p, 2);
            else if (CompareCases("110000", pcase)) PolygonCase5(p, 0);
            else if (CompareCases("011000", pcase)) PolygonCase5(p, 1);
            else if (CompareCases("101000", pcase)) PolygonCase5(p, 2);
            else if (CompareCases("111000", pcase)) PolygonCase6(p);
            else if (CompareCases("---111", pcase)) PolygonCase7(p);
        }

        private bool CompareCases(string input, string toCompare)
        {
            
            for(int i = 0; i < input.Length; i++)
            {
                if(input[i] != '-' && input[i] != toCompare[i])
                {
                    return false;
                }
            }
            return true;
        }
        private string GetPolygonCase(Polygon p)
        {
            if(!waterMesh)return $"{(EvenHeightCorner[(p.verticles & 0).normalized.ToString()] == 0 ? 1 : 0)}{(EvenHeightCorner[(p.verticles & 1).normalized.ToString()] == 0 ? 1 : 0)}{(EvenHeightCorner[(p.verticles & 2).normalized.ToString()] == 0 ? 1 : 0)}{((p.neighbours & 0).height < p.height ? 1 : 0)}{((p.neighbours & 1).height < p.height ? 1 : 0 )}{((p.neighbours & 2).height < p.height ? 1: 0 )}";
            return $"{(EvenHeightCorner[(p.verticles & 0).normalized.ToString()] == 0 ? 1 : 0)}{(EvenHeightCorner[(p.verticles & 1).normalized.ToString()] == 0 ? 1 : 0)}{(EvenHeightCorner[(p.verticles & 2).normalized.ToString()] == 0 ? 1 : 0)}{((p.neighbours & 0).height > p.height ? 1 : 0)}{((p.neighbours & 1).height > p.height ? 1 : 0 )}{((p.neighbours & 2).height > p.height ? 1: 0 )}";
        }
        
        private void PolygonCase0(Polygon p)
        {
            //Debug.Log("0");
            //return;
            Vector3 a = p.verticles & 0;
            Vector3 b = p.verticles & 1;
            Vector3 c = p.verticles & 2;

            //MainLand
            AddTriangle(a, b, c, waterMesh ? new Color(0, 0, 1): p.GetPolygonGroundColor());
        }
        private void PolygonCase1(Polygon p, int index)
        {
            //Debug.Log("1");
            //return;
            Vector3 a = p.verticles & index + 0;
            Vector3 b = p.verticles & index + 1;
            Vector3 c = p.verticles & index + 2;
            Vector3 d = Vector3.Lerp(b, a, Constants.TRIANGLE_SCALE);
            Vector3 e = Vector3.Lerp(c, a, Constants.TRIANGLE_SCALE);

            //MainLand
            AddQuad(d, b, c, e, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());

            if (!waterMesh)
            {
                //EdgeStrip
                AddTriangle(a.normalized * heightFactor, d.normalized * heightFactor, e.normalized * heightFactor, p.GetPolygonEdgeColor());
                //Wall
                AddQuad(e, e.normalized * heightFactor, d.normalized * heightFactor, d, p.GetPolygonWallColor());
            }
            else
            {
                //EdgeStrip
                AddTriangle(a, d, e, new Color(1, 0, 0),new Color(0,0,0),new Color(0,0,0));
         
            }

        }
        private void PolygonCase2(Polygon p, int index)
        {
            //Debug.Log("2");
            //return;
            Vector3 a = p.verticles & index + 0;
            Vector3 b = p.verticles & index + 1;
            Vector3 c = p.verticles & index + 2;
            Vector3 d = Vector3.Lerp(c, a, Constants.TRIANGLE_SCALE);
            Vector3 e = Vector3.Lerp(c, b, Constants.TRIANGLE_SCALE);

            //MainLand
            AddTriangle(c, d, e, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());
            
            if (!waterMesh)
            {
                //EdgeStrip
                AddQuad(d.normalized * heightFactor, a.normalized * heightFactor, b.normalized * heightFactor, e.normalized * heightFactor,p.GetPolygonEdgeColor());
                //Wall
                AddQuad(d, d.normalized * heightFactor, e.normalized * heightFactor, e, p.GetPolygonWallColor());
                AddQuad(a.normalized * heightFactor, a.normalized, b.normalized, b.normalized * heightFactor, p.GetPolygonEdgeColor());
            }
            else
            {
                //EdgeStrip
                AddQuad(d, a, b, e, new Color(0, 0, 0),new Color(1,0,0),new Color(1,0,0),new Color(0,0,0));
                
            }


        }
        private void PolygonCase3(Polygon p, int index)
        {
            //Debug.Log("3");
            //return;
            Vector3 a = p.verticles & index + 0;
            Vector3 b = p.verticles & index + 1;
            Vector3 c = p.verticles & index + 2;
            Vector3 d = Vector3.Lerp(Vector3.Lerp(b,c,.5f), a, Constants.TRIANGLE_SCALE*11/12f);
            Vector3 e = Vector3.Lerp(c, b,Constants.TRIANGLE_SCALE);
            Vector3 f = Vector3.Lerp(b, c,Constants.TRIANGLE_SCALE);

            //MainLand
            AddTriangle(d, e, f, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());
            
            if (!waterMesh)
            {
                //EdgeStrip
                AddQuad(a.normalized * heightFactor, b.normalized * heightFactor, e.normalized * heightFactor, d.normalized * heightFactor, p.GetPolygonEdgeColor());
                AddQuad(a.normalized * heightFactor, d.normalized * heightFactor, f.normalized * heightFactor, c.normalized * heightFactor, p.GetPolygonEdgeColor());
                //Wall
                AddQuad(d, d.normalized * heightFactor, e.normalized * heightFactor, e, p.GetPolygonWallColor());
                AddQuad(a.normalized * heightFactor, a.normalized, b.normalized, b.normalized * heightFactor, p.GetPolygonEdgeColor());

                AddQuad(f, f.normalized * heightFactor, d.normalized * heightFactor, d, p.GetPolygonWallColor());
                AddQuad(c.normalized * heightFactor, c.normalized, a.normalized, a.normalized * heightFactor, p.GetPolygonEdgeColor());
            }
            else
            {
                //EdgeStrip
                AddQuad(a, b, e, d, new Color(1, 0, 0),new Color(1,0,0),new Color(0,0,0),new Color(0,0,0));
                AddQuad(a, d, f, c, new Color(1, 0, 0),new Color(0,0,0),new Color(0,0,0),new Color(1,0,0));               
            }

        }
        private void PolygonCase4(Polygon p, int index)
        {
            //Debug.Log("4");
            //return;
            Vector3 a = p.verticles & index + 0;
            Vector3 b = p.verticles & index + 1;
            Vector3 c = p.verticles & index + 2;
            Vector3 d = Vector3.Lerp(b, a, Constants.TRIANGLE_SCALE);
            Vector3 e = Vector3.Lerp(c, a, Constants.TRIANGLE_SCALE);
            Vector3 f = Vector3.Lerp(a, b, Constants.TRIANGLE_SCALE);
            Vector3 g = Vector3.Lerp(a, c, Constants.TRIANGLE_SCALE);

            //MainLand
            AddQuad(e, d, f, g, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());
            
            if (!waterMesh)
            {
                //EdgeStrip
                AddTriangle(a.normalized * heightFactor, d.normalized * heightFactor, e.normalized * heightFactor, p.GetPolygonEdgeColor());
                AddQuad(b.normalized * heightFactor, c.normalized * heightFactor, g.normalized * heightFactor, f.normalized * heightFactor, p.GetPolygonEdgeColor());

                //Wall
                AddQuad(d, d.normalized * heightFactor, e.normalized * heightFactor, e, p.GetPolygonWallColor());

                AddQuad(f, f.normalized * heightFactor, g.normalized * heightFactor, g, p.GetPolygonWallColor());
                AddQuad(b.normalized * heightFactor, b.normalized, c.normalized, c.normalized * heightFactor, p.GetPolygonEdgeColor());
            }
            else
            {
                //EdgeStrip
                AddTriangle(a, d, e, new Color(1, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                AddQuad(b, c, g, f, new Color(1, 0, 0), new Color(1, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
            }
        }
        private void PolygonCase5(Polygon p, int index)
        {
            //Debug.Log("5");
            //return;
            Vector3 a = p.verticles & index + 0;
            Vector3 b = p.verticles & index + 1;
            Vector3 c = p.verticles & index + 2;
            Vector3 d = Vector3.Lerp(b, a, Constants.TRIANGLE_SCALE);
            Vector3 e = Vector3.Lerp(c, a, Constants.TRIANGLE_SCALE);
            Vector3 f = Vector3.Lerp(a, b, Constants.TRIANGLE_SCALE);
            Vector3 g = Vector3.Lerp(c, b, Constants.TRIANGLE_SCALE);
            //MainLand
            AddQuad(d, f, g, e, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());
            AddTriangle(c,e, g, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());
            

            //Wall
            if (!waterMesh)
            {
                //EdgeStrip
                AddTriangle(a.normalized * heightFactor, d.normalized * heightFactor, e.normalized * heightFactor, p.GetPolygonEdgeColor());
                AddTriangle(b.normalized * heightFactor, g.normalized * heightFactor, f.normalized * heightFactor, p.GetPolygonEdgeColor());

                AddQuad(e, e.normalized * heightFactor, d.normalized * heightFactor, d, p.GetPolygonWallColor());
                AddQuad(f, f.normalized * heightFactor, g.normalized * heightFactor, g, p.GetPolygonWallColor());
            }
            else
            {
                //EdgeStrip
                AddTriangle(a, d, e, new Color(1, 0, 0),new Color(0,0,0),new Color(0,0,0));
                AddTriangle(b, g, f, new Color(1, 0, 0),new Color(0,0,0),new Color(0,0,0));
            }
        }
        private void PolygonCase6(Polygon p)
        {
            //Debug.Log("6");
            //return;
            Vector3 a = p.verticles & 0;
            Vector3 b = p.verticles & 1;
            Vector3 c = p.verticles & 2;
            Vector3 d = Vector3.Lerp(b, a, Constants.TRIANGLE_SCALE);
            Vector3 e = Vector3.Lerp(c, a, Constants.TRIANGLE_SCALE);
            Vector3 f = Vector3.Lerp(a, b, Constants.TRIANGLE_SCALE);
            Vector3 g = Vector3.Lerp(c, b, Constants.TRIANGLE_SCALE);
            Vector3 h = Vector3.Lerp(a, c, Constants.TRIANGLE_SCALE);
            Vector3 i = Vector3.Lerp(b, c, Constants.TRIANGLE_SCALE);
            //MainLand
            AddQuad(d, f, h, e, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());
            AddQuad(h, f, g, i, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());
            
            if (!waterMesh)
            {
                //EdgeStrip
                AddTriangle(a.normalized * heightFactor, d.normalized * heightFactor, e.normalized * heightFactor, p.GetPolygonEdgeColor());
                AddTriangle(b.normalized * heightFactor, g.normalized * heightFactor, f.normalized * heightFactor, p.GetPolygonEdgeColor());
                AddTriangle(c.normalized * heightFactor, h.normalized * heightFactor, i.normalized * heightFactor, p.GetPolygonEdgeColor());
          
                //Wall
                AddQuad(d, d.normalized * heightFactor, e.normalized * heightFactor, e, p.GetPolygonWallColor());
                AddQuad(f, f.normalized * heightFactor, g.normalized * heightFactor, g, p.GetPolygonWallColor());
                AddQuad(i, i.normalized * heightFactor, h.normalized * heightFactor, h, p.GetPolygonWallColor());
            }
            else{
                //EdgeStrip
                AddTriangle(a, d, e, new Color(1, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                AddTriangle(b, g, f, new Color(1, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                AddTriangle(c, h, i, new Color(1, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
            }
        }
        private void PolygonCase7(Polygon p)
        {
            //Debug.Log("3");
            //return;
            Vector3 a = p.verticles & 0;
            Vector3 b = p.verticles & 1;
            Vector3 c = p.verticles & 2;
            Vector3 d = Vector3.Lerp(Vector3.Lerp(b, c, .5f), a, Constants.TRIANGLE_SCALE);
            Vector3 e = Vector3.Lerp(Vector3.Lerp(a, c, .5f), b, Constants.TRIANGLE_SCALE);
            Vector3 f = Vector3.Lerp(Vector3.Lerp(a, b, .5f), c, Constants.TRIANGLE_SCALE);

            //MainLand
            AddTriangle(d, e, f, waterMesh ? new Color(0, 0, 1) : p.GetPolygonGroundColor());

            if (!waterMesh)
            {
                //EdgeStrip
                AddQuad(a.normalized * heightFactor, b.normalized * heightFactor, e.normalized * heightFactor, d.normalized * heightFactor, p.GetPolygonEdgeColor());
                AddQuad(b.normalized * heightFactor, c.normalized * heightFactor, f.normalized * heightFactor, e.normalized * heightFactor, p.GetPolygonEdgeColor());
                AddQuad(c.normalized * heightFactor, a.normalized * heightFactor, d.normalized * heightFactor, f.normalized * heightFactor, p.GetPolygonEdgeColor());
                //Wall
                AddQuad(d, d.normalized * heightFactor, e.normalized * heightFactor, e, p.GetPolygonWallColor());
                AddQuad(a.normalized * heightFactor, a.normalized, b.normalized, b.normalized * heightFactor, p.GetPolygonEdgeColor());

                AddQuad(e, e.normalized * heightFactor, f.normalized * heightFactor, f, p.GetPolygonWallColor());
                AddQuad(b.normalized * heightFactor, b.normalized, c.normalized, c.normalized * heightFactor, p.GetPolygonEdgeColor());

                AddQuad(f, f.normalized * heightFactor, d.normalized * heightFactor, d, p.GetPolygonWallColor());
                AddQuad(c.normalized * heightFactor, c.normalized, a.normalized, a.normalized * heightFactor, p.GetPolygonEdgeColor());
            }
            else
            {
                //EdgeStrip
                AddQuad(a, b, e, d, new Color(1, 0, 0), new Color(1, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                AddQuad(b, c, f, e, new Color(1, 0, 0), new Color(1, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                AddQuad(a, d, f, c, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0), new Color(1, 0, 0));

            }
        }
    }
}
