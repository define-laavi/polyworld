using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quixl
{
    [CreateAssetMenu(fileName = "PlanetSettings", menuName = "Custom/PlanetSettings")]
    public class PlanetSettings : ScriptableObject
    {
        public GameObject planetPrefab;
        public Material surfaceMaterial, waterMaterial;
        [Range(.5f,1f)]
        public float edgeSandScale, edgeFoamScale;
        public int cellsCount;
        public Color baseGrassColor, baseSandColor, baseDirtColor;
        public FoliageData baseFoliage;
        public int foliageAmountPerCell;
        public float globalFoliageScalar;
    }
}
