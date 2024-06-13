using System;
using Tulip.Core;
using Tulip.Data;
using Tulip.Data.Items;
using Tulip.GameWorld;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tulip.Player
{
    public class WorldModifier : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] InputActionReference smartCursor;

        [Header("Config")]
        [SerializeField] Vector2 hotspotOffset;
        public float range = 5f;

        public Vector3Int MouseCell { get; private set; }

        public Vector3Int? FocusedCell
        {
            get => focusedCell;
            private set
            {
                if (focusedCell == value) return;
                focusedCell = value;
                OnChangeCellFocus?.Invoke(focusedCell);
            }
        }

        public event Action<Vector3Int?> OnChangeCellFocus;

        private World world;
        private IWielderBrain brain;
        private Inventory inventory;
        private IItemWielder itemWielder;

        private Vector3Int? focusedCell;
        private Vector2 rangePath;
        private Vector3 hitPoint;

        private void Awake()
        {
            world = FindAnyObjectByType<World>();
            brain = GetComponent<IWielderBrain>();
            inventory = GetComponent<Inventory>();
            itemWielder = GetComponent<IItemWielder>();
        }

        private void Update()
        {
            if (smartCursor.action.triggered)
                Options.Instance.Gameplay.UseSmartCursor = !Options.Instance.Gameplay.UseSmartCursor;

            if (itemWielder.CurrentItem is not Tool) return;
            AssignCells();
        }

        private void HandleItemSwing(Usable item, Vector3 _)
        {
            if (item is not Tool tool) return;

            if (!FocusedCell.HasValue) return;

            Bounds bounds = world.CellBoundsWorld(FocusedCell.Value);
            Vector2 topLeft = bounds.center - bounds.extents + (Vector3.one * 0.02f);
            Vector2 bottomRight = bounds.center + bounds.extents - (Vector3.one * 0.02f);

            int layerMask = LayerMask.GetMask("Enemy", "Player", "NPC");
            if (Physics2D.OverlapArea(topLeft, bottomRight, layerMask))
                return;

            if (!tool.IsUsableOn(world, FocusedCell.Value)) return;

            InventoryModification modification = tool.UseOn(world, FocusedCell.Value);
            inventory.ApplyModification(modification);
        }

        private void AssignCells()
        {
            MouseCell = world.WorldToCell(brain.AimPosition);

            Vector2 hotspot = (Vector2)transform.position + hotspotOffset;
            rangePath = Vector2.ClampMagnitude((Vector2)brain.AimPosition - hotspot, range);

            if (!Options.Instance.Gameplay.UseSmartCursor || inventory.HotbarSelected?.Item is not Pickaxe)
            {
                float distance = Vector3.Distance(hotspot, brain.AimPosition);
                FocusedCell = distance <= range ? MouseCell : null;
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(
                hotspot, rangePath, range,
                LayerMask.GetMask("World"));

            hitPoint = hit.point - (hit.normal * 0.1f);
            FocusedCell = hit.collider ? world.WorldToCell(hitPoint) : null;
        }

        private void OnDrawGizmosSelected()
        {
            if (!Options.Instance.Gameplay.UseSmartCursor) return;

            Vector2 hotspot = (Vector2)transform.position + hotspotOffset;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(hotspot, hotspot + rangePath);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hitPoint, .1f);
        }

        private void OnEnable() => itemWielder.OnSwing += HandleItemSwing;
        private void OnDisable() => itemWielder.OnSwing -= HandleItemSwing;
    }
}
