using SaintsField.Playa;
using Tulip.Data.Tiles;
using UnityEngine;

namespace Tulip.Data.Items
{
    [CreateAssetMenu(menuName = "Items/Placeable")]
    public class Placeable : WorldToolBase
    {
        public override Sprite Icon => ruleTile.m_DefaultSprite;

        public Color Color => color;
        public CustomRuleTile RuleTile => ruleTile;
        public TileType TileType => tileType;
        public PlaceableMaterial Material => material;

        public bool IsUnsafe => isUnsafe;
        public bool IsUnbreakable => isUnbreakable;
        public int Hardness => hardness;
        public Ore Ore => ore;

        [Header("World Tile Data")]
        [SerializeField] protected Color color;
        [SerializeField] protected CustomRuleTile ruleTile;
        [SerializeField] protected TileType tileType;
        [SerializeField] protected PlaceableMaterial material;

        [SerializeField] protected bool isUnsafe;
        [SerializeField] protected bool isUnbreakable;

        [Min(1), PlayaDisableIf(nameof(isUnbreakable))]
        [SerializeField] protected int hardness = 50;

        [SerializeField] protected Ore ore;

        public override InventoryModification UseOn(IWorld world, Vector2Int cell) => tileType switch
        {
            TileType.Block when IsUsableOn(world, cell) => world.PlaceTile(cell, this),
            _ => default
        };

        public override bool IsUsableOn(IWorld world, Vector2Int cell) => tileType switch
        {
            // TODO: maybe bring back this constraint (originally for cell highlighting)
            // bool notOccupiedByPlayer = !world.CellIntersects(cell, playerCollider.bounds);
            TileType.Block => !world.HasBlock(cell),
            _ => false
        };

        private void OnEnable()
        {
            if (ruleTile)
                ruleTile.Placeable = this;
        }

        private void OnValidate()
        {
            if (ruleTile)
                ruleTile.Placeable = this;
        }

        private void Reset()
        {
            maxAmount = 999;
            cooldown = 0.25f;
        }
    }
}
