using UnityEngine;

namespace Tulip.Data
{
    [CreateAssetMenu(menuName = "Gameplay/Entity Spawn Pool")]
    public class EntitySpawnPoolData : ScriptableObject
    {
        [SerializeField] EntityData[] entities;

        public EntityData[] Entities => entities;
        public int Amount => entities.Length;

        public EntityData this[int index] => entities[index];
    }
}
