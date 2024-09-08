using Tulip.Data;
using UnityEngine;

namespace Tulip.GameWorld
{
    public enum EntityLocation
    {
        Outdoors,
        Indoors
    }

    public class EntityLocationDeterminer : MonoBehaviour
    {
        [SerializeField] HealthBase player;

        public Vector2 Position => entityTransform.position;
        public EntityLocation Location { get; private set; }

        private Transform entityTransform;

        private void Awake() => entityTransform = player.transform;

        private void Update()
        {
            IWorld world = player.Entity.World;
            Vector2Int playerCell = world.WorldToCell(Position);
            bool hasCurtain = world.HasTile(playerCell, TileType.Curtain);
            Location = hasCurtain ? EntityLocation.Indoors : EntityLocation.Outdoors;
        }
    }
}
