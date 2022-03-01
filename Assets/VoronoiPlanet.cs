using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoronoiPlanet : MonoBehaviour
{
    [Range(0,1000)]
    public int pointsnum;
    public int hoffset, highlight;
    public float radius;
    public int[] highlists;

    Vector3[] PointsOnSphere(int n)
    {
        List<Vector3> upts = new List<Vector3>();
        float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        float off = 2.0f / n;
        float x = 0;
        float y = 0;
        float z = 0;
        float r = 0;
        float phi = 0;

        for (var k = 0; k < n; k++)
        {
            y = k * off - 1 + (off / 2);
            r = Mathf.Sqrt(1 - y * y);
            phi = k * inc;
            x = Mathf.Cos(phi) * r;
            z = Mathf.Sin(phi) * r;

            upts.Add(new Vector3(x, y, z).normalized);
        }
        Vector3[] pts = upts.ToArray();
        return pts;
    }

    private void OnDrawGizmos()
    {
        Vector3[] points = PointsOnSphere(pointsnum);
            
        foreach(Vector3 p in points)
        {
            Gizmos.DrawSphere(p, radius);
        }

    }
}
