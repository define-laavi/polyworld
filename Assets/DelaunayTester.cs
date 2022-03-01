using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DelaunayTester : MonoBehaviour
{
    public bool generate = false;
    public bool displayDebug = false;
    public bool delaunay = false;
    public int pointsAmount;
    public float jitterAmount = 0.1f;
    Vector2[] points = new Vector2[0];

    List<Triangle> flatSurface = new List<Triangle>();
    List<NeighbourTriangle> spherical = new List<NeighbourTriangle>();
    public int length;

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

            upts.Add((new Vector3(x, y, z) + Random.onUnitSphere* jitterAmount).normalized);
        }
        Vector3[] pts = upts.ToArray();
        return pts;
    }

    Vector2[] Stereographic(Vector3[] input)
    {
        Vector2[] ret = new Vector2[input.Length];
        for(int i =0; i< input.Length; i++)
        {
            float x = input[i].x / (1f - input[i].z);
            float y = input[i].y / (1f - input[i].z);
            ret[i] = new Vector2(x*1000,y*1000);
        }
        return ret;
    }
    Vector3 Spherical(Vector2 input)
    {
        float same = (1 + input.x/1000f * input.x/1000f + input.y/1000f * input.y/1000f);
        float x = (2f * input.x/1000f) / same;
        float y = (2f * input.y/1000f) / same;
        float z = (same-2f) / (same);

        return new Vector3(x, y, z).normalized;
    }

    private void OnDrawGizmos()
    {
        if (generate)
        {
            generate = !generate;
            points = Stereographic(PointsOnSphere(pointsAmount));
            flatSurface = Triangulation(points);
            spherical = new List<NeighbourTriangle>();
            foreach(Triangle t in flatSurface)
                spherical.Add(new NeighbourTriangle(Spherical(t.a), Spherical(t.b), Spherical(t.c)));
            foreach (NeighbourTriangle t in spherical)
            {
                foreach (NeighbourTriangle other in spherical)
                {
                    if (t == other) continue;
                    if (t.neighbours.Contains(other)) continue;
                    // TODO: remake the overlappingelementsCount
                    if (t.GetVerts().Count((tVert) => { return other.GetVerts().Any((otherVert) => { return (otherVert - tVert).sqrMagnitude < 0.01f; }); }) == 2)
                    {
                        t.neighbours.Add(other);
                        other.neighbours.Add(t);
                    }
                }
            }


            length = flatSurface.Count;
        }
        if (points.Length >= 3)
        {
            if (displayDebug)
            {
                foreach (Triangle t in flatSurface)
                {
                    Gizmos.DrawLine(t.a / 1000f, t.b / 1000f);
                    Gizmos.DrawLine(t.b / 1000f, t.c / 1000f);
                    Gizmos.DrawLine(t.c / 1000f, t.a / 1000f);
                }
            }
            else
            {
                Gizmos.color = Color.red;
                foreach (NeighbourTriangle t in spherical)
                {
                    Gizmos.DrawLine(t.a, t.b);
                    Gizmos.DrawLine(t.b, t.c);
                    Gizmos.DrawLine(t.c, t.a);
                }
                Gizmos.color = Color.white;
                foreach (NeighbourTriangle t in spherical)
                {
                    foreach (NeighbourTriangle n in t.neighbours)
                    {
                        Gizmos.DrawLine(t.center, n.center);
                    }
                }
            }
        }
    }

    List<Triangle> Triangulation(Vector2[] points)
    {
        List<Triangle> triangulation = new List<Triangle>();
        triangulation.Add(Triangle.superTriangle);
        foreach (Vector2 p in points)
        {
            List<Triangle> badTriangles = new List<Triangle>();
            foreach (Triangle t in triangulation)
                if (new Circle(t.a, t.b, t.c).IsInside(p))
                    badTriangles.Add(t);
            List<Edge> polygon = new List<Edge>();
            foreach (Triangle t in badTriangles)
                foreach (Edge e in t.GetEdges())
                    if (!badTriangles.Any((x) => x != t && x.GetEdges().Any((y) => y == e)))
                        polygon.Add(e);
            foreach (Triangle t in badTriangles)
                triangulation.Remove(t);
            foreach(Edge e in polygon)
                triangulation.Add(new Triangle(e.start, e.end, p));
        }

        return triangulation.FindAll((x) => !x.GetVerts().Any((y) => Triangle.superTriangle.GetVerts().Contains(y)));
    }
}

public struct Triangle
{
    public Vector3 a, b, c;
    public Triangle(Vector3 p1,Vector3 p2,Vector3 p3)
    {
        a = p1;
        b = p2;
        c = p3;
    }

    public static bool operator ==(Triangle a, Triangle b)
    {
        return ((a.a + a.b + a.c) - (b.a + b.b + b.c)).sqrMagnitude < 0.1f;
    }

    public static bool operator !=(Triangle a, Triangle b)
    {
        return ((a.a + a.b + a.c) - (b.a + b.b + b.c)).sqrMagnitude >= 0.1f;
    }

    public static Triangle superTriangle
    {
        get { return new Triangle(new Vector2(-999999, -999999), new Vector2(0, 999999), new Vector2(999999, -999999)); }
    }

    public Edge[] GetEdges()
    {
        return new Edge[3] { new Edge(a, b), new Edge(b, c), new Edge(c, a)};
    }

    public Vector2[] GetVerts()
    {
        return new Vector2[3] { a, b, c };
    }
}

public class NeighbourTriangle
{
    public Vector3 a, b, c;

    public Vector3 center
    {
        get { return (a + b + c) / 3f; }
    }

    public List<NeighbourTriangle> neighbours;

    public NeighbourTriangle(Triangle t)
    {
        a = t.a;
        b = t.b;
        c = t.c;
        neighbours = new List<NeighbourTriangle>();
    }

    public NeighbourTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        a = p1;
        b = p2;
        c = p3;

        neighbours = new List<NeighbourTriangle>();
    }

    public Edge[] GetEdges()
    {
        return new Edge[3] { new Edge(a, b), new Edge(b, c), new Edge(c, a) };
    }

    public Vector3[] GetVerts()
    {
        return new Vector3[3] { a, b, c };
    }
}

public struct Edge
{
    public Vector2 start, end;

    public Edge(Vector2 _a,Vector2 _b)
    {
        start = _a;
        end = _b;
    }

    public static bool operator ==(Edge a, Edge b)
    {
        return ((a.start + a.end) - (b.start + b.end)).sqrMagnitude < 0.01f;
    }

    public static bool operator !=(Edge a, Edge b)
    {
        return ((a.start + a.end) - (b.start + b.end)).sqrMagnitude >= 0.01f;
    }
}

public struct Circle
{
    public Vector2 center;
    public float radius;

    public bool Equals(Circle other)
    {
        return this.center == other.center && this.radius == other.radius;
    }
    
    public Circle(Vector2 c, float r)
    {
        center = c;
        radius = r;
    }

   
    public Circle(Vector2 a, Vector2 b, Vector2 c)
    {
        // Get the perpendicular bisector of (x1, y1) and (x2, y2).
        float x1 = (b.x + a.x) / 2;
        float y1 = (b.y + a.y) / 2;
        float dy1 = b.x - a.x;
        float dx1 = -(b.y - a.y);

        // Get the perpendicular bisector of (x2, y2) and (x3, y3).
        float x2 = (c.x + b.x) / 2;
        float y2 = (c.y + b.y) / 2;
        float dy2 = c.x - b.x;
        float dx2 = -(c.y - b.y);

        // See where the lines intersect.
        bool lines_intersect, segments_intersect;
        Vector2 intersection, close1, close2;
        FindIntersection(
            new Vector2(x1, y1), new Vector2(x1 + dx1, y1 + dy1),
            new Vector2(x2, y2), new Vector2(x2 + dx2, y2 + dy2),
            out lines_intersect, out segments_intersect,
            out intersection, out close1, out close2);
        if (!lines_intersect)
        {
            center = new Vector2(0, 0);
            radius = 0;
        }
        else
        {
            center = intersection;
            float dx = center.x - a.x;
            float dy = center.y - a.y;
            radius = (float)Mathf.Sqrt(dx * dx + dy * dy);
        }
    }

    private static void FindIntersection(
    Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
    out bool lines_intersect, out bool segments_intersect,
    out Vector2 intersection,
    out Vector2 close_p1, out Vector2 close_p2)
    {
        // Get the segments' parameters.
        float dx12 = p2.x - p1.x;
        float dy12 = p2.y - p1.y;
        float dx34 = p4.x - p3.x;
        float dy34 = p4.y - p3.y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1 =
            ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                / denominator;
        if (float.IsInfinity(t1))
        {
            // The lines are parallel (or close enough to it).
            lines_intersect = false;
            segments_intersect = false;
            intersection = new Vector2(float.NaN, float.NaN);
            close_p1 = new Vector2(float.NaN, float.NaN);
            close_p2 = new Vector2(float.NaN, float.NaN);
            return;
        }
        lines_intersect = true;

        float t2 =
            ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12)
                / -denominator;

        // Find the point of intersection.
        intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        segments_intersect =
            ((t1 >= 0) && (t1 <= 1) &&
             (t2 >= 0) && (t2 <= 1));

        // Find the closest points on the segments.
        if (t1 < 0)
        {
            t1 = 0;
        }
        else if (t1 > 1)
        {
            t1 = 1;
        }

        if (t2 < 0)
        {
            t2 = 0;
        }
        else if (t2 > 1)
        {
            t2 = 1;
        }

        close_p1 = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);
        close_p2 = new Vector2(p3.x + dx34 * t2, p3.y + dy34 * t2);
    }

    public bool IsInside(Vector2 point)
    {
        return Vector2.Distance(point, center) <= radius;
    }
}
