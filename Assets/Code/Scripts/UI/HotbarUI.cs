using System;
using System.Collections;
using Game.Data;
using Game.Data.Interfaces;
using Game.Data.Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [SerializeField] InventoryBase inventory;
        [SerializeField] float tooltipShowDuration;

        private AudioSource audioSource;
        private UIDocument document;
        private VisualElement hotbarRoot;
        private VisualElement tooltipRoot;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            document = GetComponent<UIDocument>();
            hotbarRoot = document.rootVisualElement[0];
            tooltipRoot = document.rootVisualElement[1];
        }

        private void UpdateHotbar()
        {
            ItemStack[] items = inventory.Items;
            for (int i = 0; i < items.Length; i++)
            {
                ItemStack slot = items[i];
                IItem item = slot?.Item;

                VisualElement button = hotbarRoot[i];
                Label slotLabel = button.Q<Label>();
                Image slotImage = button.Q<Image>();

                slotLabel.visible = item is { MaxAmount: > 1 };
                slotLabel.text = slot?.Amount.ToString();
                slotImage.sprite = item?.Icon;

                if (item == null) continue;
                slotImage.transform.scale = Vector3.one * item.IconScale;
                slotImage.tintColor = item is WorldTile tile ? tile.color : Color.white;
            }
        }

        private void UpdateHotbarSelection(int index)
        {
            audioSource.Play();

            for (int i = 0; i < hotbarRoot.childCount; i++)
                hotbarRoot[i].RemoveFromClassList("selected");

            hotbarRoot[index].AddToClassList("selected");
            UpdateTooltip();
        }

        private void UpdateTooltip()
        {
            IItem selectedItem = inventory.HotbarSelected?.Item;
            tooltipRoot.visible = selectedItem != null;

            Label nameLabel = tooltipRoot.Q<Label>("tooltip-name");
            Label descriptionLabel = tooltipRoot.Q<Label>("tooltip-description");
            nameLabel.text = selectedItem?.Name;
            descriptionLabel.text = selectedItem?.Description;

            int slotIndex = inventory.HotbarSelectedIndex;
            Vector3 slotPosition = hotbarRoot[slotIndex].layout.position;
            float offset = (tooltipRoot.layout.size.x / 2f) - (hotbarRoot[slotIndex].layout.size.x / 2f);

            float newPositionX = slotPosition.x - tooltipRoot.layout.position.x - offset;
            tooltipRoot.transform.position = Vector3.right * newPositionX;

            if (!tooltipRoot.visible) return;
            StopAllCoroutines();
            StartCoroutine(showTooltip());
            return;

            IEnumerator showTooltip()
            {
                tooltipRoot.visible = true;
                yield return new WaitForSeconds(tooltipShowDuration);
                tooltipRoot.visible = false;
            }
        }

        private void OnEnable()
        {
            inventory.OnModifyHotbar += UpdateHotbar;
            inventory.OnChangeHotbarSelection += UpdateHotbarSelection;
        }

        private void OnDisable()
        {
            inventory.OnModifyHotbar -= UpdateHotbar;
            inventory.OnChangeHotbarSelection -= UpdateHotbarSelection;
        }
    }
}
