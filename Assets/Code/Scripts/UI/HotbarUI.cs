using Game.Data;
using Game.Data.Interfaces;
using Game.Data.Tiles;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [SerializeField] InventoryBase inventory;

        private AudioSource audioSource;
        private UIDocument document;
        private VisualElement root;

        private void OnHotbarModified()
        {
            ItemStack[] items = inventory.Items;
            for (int i = 0; i < items.Length; i++)
            {
                VisualElement button = root[i];
                Label label = button.Q<Label>();
                Image image = button.Q<Image>();

                ItemStack slot = items[i];
                IItem item = slot?.Item;

                label.visible = item is { MaxAmount: > 1 };
                label.text = slot?.Amount.ToString();
                image.sprite = item?.Icon;

                if (item == null) continue;
                image.transform.scale = Vector3.one * item.IconScale;
                image.tintColor = item is BlockTile block ? block.color : Color.white;
            }
        }

        private void OnHotbarSelectionChanged(int index)
        {
            audioSource.Play();
            for (int i = 0; i < root.childCount; i++)
            {
                if (i == index)
                    root[i].AddToClassList("selected");
                else
                    root[i].RemoveFromClassList("selected");
            }
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement[0];
        }

        private void OnEnable()
        {
            inventory.HotbarModified += OnHotbarModified;
            inventory.HotbarSelectionChanged += OnHotbarSelectionChanged;
        }

        private void OnDisable()
        {
            inventory.HotbarModified -= OnHotbarModified;
            inventory.HotbarSelectionChanged -= OnHotbarSelectionChanged;
        }
    }
}