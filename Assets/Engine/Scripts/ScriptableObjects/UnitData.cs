using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quixl
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "Custom/UnitData")]
    public class UnitData : ScriptableObject
    {
        public GameObject model;
        public int attack, defence, movement, maxHealth, range;
    }
}
