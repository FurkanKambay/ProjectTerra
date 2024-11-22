using System.Collections;
using FMODUnity;
using SaintsField;
using Tulip.Core;
using Tulip.Data;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tulip.UI
{
    public class HotbarPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Required] UIDocument document;
        [SerializeField, Required] SaintsInterface<Component, IHotbar> hotbar;

        [Header("FMOD Events")]
        [SerializeField, Required] StudioEventEmitter hotbarSfx;

        [Header("Config")]
        [OverlayRichLabel("<color=grey>ms")]
        [SerializeField] int tooltipShowDuration = 1000;

        [OverlayRichLabel("<color=grey>ms")]
        [SerializeField] int tooltipSlideDuration = 100;

        // ReSharper disable NotAccessedField.Local
        [CreateProperty] ItemStack heldItem;
        [CreateProperty] ItemStack[] items;
        // ReSharper restore NotAccessedField.Local

        private VisualElement hotbarRoot;
        private VisualElement tooltipRoot;
        private int selectedIndex;

        private void OnEnable() => GameManager.OnGameStateChange += HandleGameStateChange;
        private void OnDisable() => GameManager.OnGameStateChange -= HandleGameStateChange;

        private void Start() => RefreshDocument();

        private void RefreshDocument()
        {
            if (document.rootVisualElement == null)
                return;

            hotbarRoot = document.rootVisualElement[0];
            tooltipRoot = document.rootVisualElement[1];
            document.rootVisualElement.dataSource = this;

            UpdateItems();
            SelectSlot(hotbar.I.SelectedIndex);
            UpdateTooltip();
        }

        private void SelectSlot(int index)
        {
            selectedIndex = index;

            for (int i = 0; i < hotbarRoot.childCount; i++)
                hotbarRoot[i].RemoveFromClassList("selected");

            hotbarRoot[index].AddToClassList("selected");

            UpdateTooltip();
        }

        private void UpdateTooltip()
        {
            if (tooltipRoot == null)
                return;

            ItemStack selectedStack = hotbar.I.SelectedStack;
            heldItem = selectedStack;

            // BUG: tooltip doesn't hide when amount becomes 0
            tooltipRoot.visible = selectedStack.IsValid;

            int slotIndex = hotbar.I.SelectedIndex;
            Vector3 slotPosition = hotbarRoot[slotIndex].layout.position;
            float offset = (tooltipRoot.layout.size.x / 2f) - (hotbarRoot[slotIndex].layout.size.x / 2f);

            float newPositionX = slotPosition.x - tooltipRoot.layout.position.x - offset;
            tooltipRoot.experimental.animation.Position(Vector3.right * newPositionX, tooltipSlideDuration);

            if (!tooltipRoot.visible)
                return;

            StopAllCoroutines();
            StartCoroutine(showTooltip());
            return;

            IEnumerator showTooltip()
            {
                tooltipRoot.visible = true;
                yield return new WaitForSeconds(tooltipShowDuration / 1000f);
                tooltipRoot.visible = false;
            }
        }

        private void UpdateItems() => items = hotbar.I.Items;

        private void HandleHotbarChangedSelection(int index)
        {
            if (selectedIndex == index || !document.enabled)
                return;

            hotbarSfx.Play();
            SelectSlot(index);
        }

        private void HandleGameStateChange(GameState oldState, GameState newState)
        {
            document.enabled = newState is GameState.Playing;

            if (document.enabled)
            {
                RefreshDocument();

                hotbar.I.OnModify += UpdateItems;
                hotbar.I.OnChangeSelection += HandleHotbarChangedSelection;
            }
            else
            {
                hotbar.I.OnModify -= UpdateItems;
                hotbar.I.OnChangeSelection -= HandleHotbarChangedSelection;
            }
        }
    }
}
