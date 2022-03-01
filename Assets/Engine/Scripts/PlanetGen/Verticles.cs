using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quixl
{
    public class Verticles : HashSet<Vector3>
    {
        #region Operators
        public static Verticles operator +(Verticles a, Verticles b)
        {
            Verticles ret = new Verticles();
            ret.UnionWith(a);
            ret.UnionWith(b);
            return ret;
        }

        public static Vector3 operator &(Verticles a, int b)
        {
            Vector3[] array = new Vector3[a.Count];
            a.CopyTo(array);
            return array[b % array.Length];
        }

        public static Verticles operator *(Verticles a, Verticles b)
        {
            Verticles ret = new Verticles();
            ret.UnionWith(a);
            ret.IntersectWith(b);
            return ret;
        }

        public static Verticles operator -(Verticles a, Verticles b)
        {
            Verticles ret = new Verticles();
            ret.UnionWith(a);
            ret.ExceptWith(b);
            return ret;
        }

        public static Verticles operator +(Verticles a, Vector3 b)
        {
            Verticles ret = new Verticles();
            ret.UnionWith(a);
            ret.Add(b);
            return ret;
        }

        public static Verticles operator *(Verticles a, Vector3 b)
        {
            Verticles ret = new Verticles();
            ret.UnionWith(a);
            if (!ret.Contains(b))
                ret.Add(b);
            return ret;
        }

        public static Verticles operator -(Verticles a, Vector3 b)
        {
            Verticles ret = new Verticles();
            ret.UnionWith(a);
            ret.Remove(b);
            return ret;
        }

        public static implicit operator Vector3[] (Verticles a)
        {
            Vector3[] v = new Vector3[a.Count];
            a.CopyTo(v);
            return v;
        }

        #endregion
        public int IndexOf(Vector3 vs)
        {
            int i = 0;
            foreach(Vector3 v in this)
            {
                if (v == vs)
                    return i;
                i++;
            }
            return -1;
        }
        public void AddRange(params Vector3[] vs)
        {
            foreach (Vector3 v in vs)
                this.Add(v);
        }
        public int OverlappingElementsCount(Verticles other)
        {
            int ret = 0;
            for (int i = 0; i < this.Count; i++)
            {
                for (int j = 0; j < other.Count; j++)
                {
                    if (Vector3.Distance(other & (j), (this & (i))) < 0.01f)
                        ret++;
                }
            }

            return ret;
        }
        public bool ContainsInDistance(Vector3 v)
        {
            foreach (Vector3 vc in this)
                if (Vector3.Distance(vc, v) < 0.01f)
                    return true;
            return false;
        }
    }
}