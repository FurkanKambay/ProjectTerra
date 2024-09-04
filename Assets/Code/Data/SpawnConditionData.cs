using System.Linq;
using SaintsField;
using SaintsField.Playa;
using Tulip.Data.Items;
using UnityEngine;

namespace Tulip.Data
{
    [CreateAssetMenu(menuName = "Gameplay/Spawn Condition")]
    public class SpawnConditionData : ScriptableObject
    {
        [Header("Ground")]
        [SerializeField] bool needsGround;

        [EnableIf(nameof(needsGround))]
        [SerializeField] bool needsSafeGround;

        [EnableIf(nameof(needsGround))]
        [SerializeField] PlaceableData[] groundTiles;

        [Header("Clearance")]
        [SerializeField, Min(0)] int clearanceAbove;

        [DisableIf(nameof(needsGround))]
        [SerializeField, Min(0)] int clearanceBelow;

        // ReSharper disable NotAccessedField.Global
        [LayoutGroup("Referenced By", ELayout.Background | ELayout.TitleOut | ELayout.Foldout, marginTop: 16)]
        [SerializeField, ReadOnly] protected EntityData[] assignedEntities;
        // ReSharper restore NotAccessedField.Global

        /// <param name="entityData"></param>
        /// <param name="world"></param>
        /// <param name="baseCell">The bottom-left cell, NOT center or pivot</param>
        public bool CanSpawn(EntityData entityData, IWorld world, Vector2Int baseCell)
        {
            if (!world.CanAccommodate(baseCell, entityData.Size))
                return false;

            // Check tiles above
            for (int y = 0; y < clearanceAbove; y++)
            for (int x = 0; x < entityData.Size.x; x++)
            {
                if (world.HasBlock(baseCell + new Vector2Int(x, entityData.Size.y + y)))
                    return false;
            }

            // Check tiles below
            if (!needsGround)
            {
                for (int y = 1; y <= clearanceBelow; y++)
                for (int x = 0; x < entityData.Size.x; x++)
                {
                    if (world.HasBlock(baseCell + new Vector2Int(x, -y)))
                        return false;
                }
            }

            if (!needsGround)
                return true;

            // Check ground only
            for (int x = 0; x < entityData.Size.x; x++)
            {
                Vector2Int floorCell = baseCell + new Vector2Int(x, -1);
                PlaceableData floorTile = world.GetBlock(floorCell);

                if (!world.HasBlock(floorCell) || (needsSafeGround && floorTile.IsUnsafe))
                    return false;

                if (groundTiles.Length > 0 && !groundTiles.Contains(floorTile))
                    return false;
            }

            return true;
        }

        private void OnValidate() =>
            assignedEntities = Resources.FindObjectsOfTypeAll<EntityData>()
                .Where(entityData => entityData.SpawnConditionData == this)
                .ToArray();
    }
}
