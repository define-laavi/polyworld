using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quixl
{
    public class PolygonSet : HashSet<Polygon>
    {
        #region Operators
        public static PolygonSet operator +(PolygonSet a, PolygonSet b)
        {
            PolygonSet ret = new PolygonSet();
            ret.UnionWith(a);
            ret.UnionWith(b);
            return ret;
        }

        public static Polygon operator &(PolygonSet a, int b)
        {
            Polygon[] array = new Polygon[a.Count];
            a.CopyTo(array);
            return array[b % array.Length];
        }

        public static PolygonSet operator *(PolygonSet a, PolygonSet b)
        {
            PolygonSet ret = new PolygonSet();
            ret.UnionWith(a);
            ret.IntersectWith(b);
            return ret;
        }

        public static PolygonSet operator -(PolygonSet a, PolygonSet b)
        {
            PolygonSet ret = new PolygonSet();
            ret.UnionWith(a);
            ret.ExceptWith(b);
            return ret;
        }

        public static PolygonSet operator +(PolygonSet a, Polygon b)
        {
            PolygonSet ret = new PolygonSet();
            ret.UnionWith(a);
            ret.Add(b);
            return ret;
        }

        public static PolygonSet operator *(PolygonSet a, Polygon b)
        {
            PolygonSet ret = new PolygonSet();
            ret.UnionWith(a);
            if (!ret.Contains(b))
                ret.Add(b);
            return ret;
        }

        public static PolygonSet operator -(PolygonSet a, Polygon b)
        {
            PolygonSet ret = new PolygonSet();
            ret.UnionWith(a);
            ret.Remove(b);
            return ret;
        }
        #endregion

        public void RemoveAt(int index)
        {
            Remove(this & index);
        }

        public PolygonSet Reverse()
        {
            Polygon[] arr = new Polygon[Count];
            PolygonSet ret = new PolygonSet();
            CopyTo(arr);

            for (int i = arr.Length - 1; i >= 0; i--)
            {
                ret.Add(arr[i]);
            }
            return ret;
        }
    }
}