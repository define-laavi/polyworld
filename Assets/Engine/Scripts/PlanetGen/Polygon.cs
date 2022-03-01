using System.Collections.Generic;
using UnityEngine;

namespace Quixl
{
    public class Polygon
    {       
        public Verticles verticles;
        public PolygonSet neighbours;

        public List<Transform> foliage;
        public Transform structure;

        public float height;
        int biome = 0;
        public TribeData tribe;
        public PathData pathData;

        public Polygon(Vector3 v1,Vector3 v2,Vector3 v3)
        {
            verticles = new Verticles() { v1, v2, v3 };
            neighbours = new PolygonSet();
            foliage = new List<Transform>();
            height = 1;
            pathData = new PathData(this);
        }

        #region Edition
        public void ChangeBiome(int _biome)
        {
            biome = _biome;
        }     
        public void ChangeHeight(float _height)
        {
            height += _height;
            verticles = new Verticles{ ((verticles & (0)).normalized * height), ((verticles & (1)).normalized * height), ((verticles & (2)).normalized * height)};
        }
        public void SetTribe(TribeData t)
        {
            tribe = t;
        }
        #endregion
        #region Getters
        public Color GetPolygonGroundColor()
        {
            if (tribe != null)
                return tribe.tribeGrassColor;
            else {
                if (biome == 0)
                    return Constants.COLOR_SAND;
                return Constants.COLOR_GRASS;
            }
        }
        public Color GetPolygonWallColor()
        {
            if (tribe != null)
                return tribe.tribeDirtColor;
            else
                return Constants.COLOR_DIRT;
        }
        public Color GetPolygonEdgeColor()
        {
            if (tribe != null)
                return tribe.tribeSandColor;
            else
                return Constants.COLOR_SAND;
        }
        public Vector3 GetPolygonCenter()
        {
            return ((verticles & 0) + (verticles & 1) + (verticles & 2))/ 3f;
        }
        public Vector3 GetRandomPositionInside(System.Random r)
        {
            int min = (int)(55 - 50 * Constants.TRIANGLE_SCALE), max = (int)(45 + 50 * Constants.TRIANGLE_SCALE);
            return Vector3.Lerp(Vector3.Lerp(verticles & (0), verticles & (1), r.Next(min, max) / 100f), verticles & (2), r.Next(min, max) / 100f);
        }
        #endregion

        public void TryAddNeighbour(Polygon other)
        {
            if (neighbours.Contains(other))
            {
                if (!other.neighbours.Contains(this))
                    other.neighbours.Add(this);
                else return;
            }
            else if (other.neighbours.Contains(this))
            {
                this.neighbours.Add(other);
            }
            else if(this.verticles.OverlappingElementsCount(other.verticles) == 2 )
            {
                this.neighbours.Add(other);
                other.neighbours.Add(this);
            }
        }
        public void SortNeighbours()
        {
            Polygon[] neighb = new Polygon[neighbours.Count];
            neighbours.CopyTo(neighb);

            neighbours.Clear();

            if (neighb[0].verticles.ContainsInDistance(verticles & (0)))
            { //we know it is 0 or 2 

                if (neighb[0].verticles.ContainsInDistance(verticles & (1)))
                {
                    neighbours.Add(neighb[0]);

                    if (neighb[1].verticles.ContainsInDistance(verticles & (1)))
                    {
                        neighbours.Add(neighb[1]);
                        neighbours.Add(neighb[2]);
                    }
                    else
                    {
                        neighbours.Add(neighb[2]);
                        neighbours.Add(neighb[1]);
                    }

                }
                else
                {
                    if (neighb[1].verticles.ContainsInDistance(verticles & (2)))
                    {
                        neighbours.Add(neighb[2]);
                        neighbours.Add(neighb[1]);
                    }
                    else
                    {
                        neighbours.Add(neighb[1]);
                        neighbours.Add(neighb[2]);
                    }
                    neighbours.Add(neighb[0]);
                }

            }
            else // we know it is neighbour 1
            {
                if (neighb[1].verticles.ContainsInDistance(verticles & (1)))
                {
                    neighbours.Add(neighb[1]);
                    neighbours.Add(neighb[0]);
                    neighbours.Add(neighb[2]);
                }
                else
                {
                    neighbours.Add(neighb[2]);
                    neighbours.Add(neighb[0]);
                    neighbours.Add(neighb[1]);
                }

            }
        }
        public void Normalize()
        {
            Verticles normalized = new Verticles();
            normalized += (verticles & 0).normalized;
            normalized += (verticles & 1).normalized;
            normalized += (verticles & 2).normalized;
            verticles = normalized;
        }
    }

    public class PathData : IHeapItem<PathData>
    {
        public float g_Cost, h_Cost;
        public float F_Cost { get { return g_Cost + h_Cost; } }
        int heapIndex;
        public Polygon parent;
        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }
            set
            {
                heapIndex = value;
            }
        }

        public PathData target;

        public int CompareTo(PathData pathObject)
        {
            int compare = F_Cost.CompareTo(pathObject.F_Cost);
            if (compare == 0)
                compare = h_Cost.CompareTo(pathObject.h_Cost);
            return -compare;
        }

        public PathData(Polygon p)
        {
            parent = p;
        }
    }
}
