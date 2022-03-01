using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quixl
{
    [CreateAssetMenu(fileName = "FoliageData", menuName = "Custom/FoliageData")]
    public class FoliageData : ScriptableObject
    {
        public float scale;
        public List<Transform> objects;

        public Transform GetRandom(System.Random r)
        {
            return objects[r.Next(objects.Count)];
        }
    }
}
