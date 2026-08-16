using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator (see plan for what was/wasn't ported).
    [CreateAssetMenu(fileName = "SimpleRandomWalkPrameters_", menuName = "PCG/SimpleRandomWalkData")]
    public class SimpleRandomWalkData : ScriptableObject
    {
        public int iterations = 10, walkLength = 10;
        public bool startRandomlyEachIteration = true;
    }
}
