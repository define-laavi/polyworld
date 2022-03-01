using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quixl {
    [CreateAssetMenu(fileName = "TribeData", menuName = "Custom/TribeData")]
    public class TribeData : ScriptableObject
    {
        public Color tribeGrassColor, tribeSandColor, tribeDirtColor;
        public FoliageData tribeFoliage;
        public int tribeFoliageAmountPerCell;
    }
}
