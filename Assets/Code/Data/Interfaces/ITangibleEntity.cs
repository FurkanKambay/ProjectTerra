using UnityEngine;

namespace Tulip.Data
{
    public interface ITangibleEntity
    {
        public string Name { get; }

        public EntityData EntityData { get; }
        public HealthBase Health { get; }

        public IWorld World { get; }
        public Vector2Int Cell { get; }
        public RectInt Rect { get; }
    }
}
