using SaintsField;
using UnityEngine;

namespace Tulip.Data
{
    [CreateAssetMenu(menuName = "World/Structure")]
    public class StructureData : ScriptableObject
    {
        [SaintsRow(inline: true)]
        [SerializeField] WorldData worldData;

        public WorldData WorldData => worldData;

#if UNITY_EDITOR
        /// Only call from the Editor
        public void SetWorldData(WorldData newWorldData) =>
            worldData = newWorldData;
#endif
    }
}
