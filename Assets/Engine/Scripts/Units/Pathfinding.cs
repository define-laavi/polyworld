using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quixl
{
    public class Pathfinding
    {
        public static PolygonSet FindPath(Polygon start, Polygon dest, PolygonSet source)
        {
            Heap<PathData> openSet = new Heap<PathData>(source.Count);
            HashSet<PathData> closedSet = new HashSet<PathData>(); 

            openSet.Add(start.pathData);

            while (openSet.Count > 0)
            {
                PathData pathData = openSet.RemoveFirst();
                closedSet.Add(pathData);

                if (pathData == null)
                {
                    Debug.Log("NoPoly");
                    return new PolygonSet();
                }
                
                
                if (pathData == dest.pathData)
                    return RetracePath(start, dest);

                foreach (Polygon neighbour in pathData.parent.neighbours)
                {
                    if (closedSet.Contains(neighbour.pathData) || !source.Contains(neighbour))
                        continue;
                    else
                    {
                        float newMovementCostToNeighbour = pathData.g_Cost + GetSphericalDistance(pathData.parent.GetPolygonCenter().normalized, neighbour.GetPolygonCenter().normalized);
                        if (newMovementCostToNeighbour < neighbour.pathData.g_Cost || !openSet.Contains(neighbour.pathData))
                        {
                            neighbour.pathData.g_Cost = newMovementCostToNeighbour;
                            neighbour.pathData.h_Cost = GetSphericalDistance(neighbour.GetPolygonCenter().normalized, dest.GetPolygonCenter().normalized);
                            neighbour.pathData.target = pathData;

                            if (!openSet.Contains(neighbour.pathData))
                                openSet.Add(neighbour.pathData);
                        }
                    }
                }
            }
            return null;
        }

        private static PolygonSet RetracePath(Polygon startNode, Polygon endNode)
        {
            PolygonSet  path = new PolygonSet();
            Polygon currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.pathData.target.parent;
            }

            return path.Reverse();
        }

        private static float GetSphericalDistance(Vector3 a, Vector3 b)
        {
            return Mathf.Acos(Vector3.Dot(a.normalized, b.normalized));
        }
    }
}