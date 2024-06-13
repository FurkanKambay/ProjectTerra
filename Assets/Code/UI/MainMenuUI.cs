using SaintsField;
using Tulip.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Tulip.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Required] UIDocument document;

        [Header("Config")]
        [SerializeField] UnityEvent onClickPlay;

        private VisualElement root;
        private Button playButton;

        public void SetRootUIVisibility(bool visible) => root.visible = visible;

        private void OnEnable()
        {
            Time.timeScale = 1;

            root = document.rootVisualElement.ElementAt(0);
            playButton = root.Q<Button>("PlayButton");
            playButton.RegisterCallback<ClickEvent>(HandlePlayClicked);
        }

        private void OnDisable() => playButton.UnregisterCallback<ClickEvent>(HandlePlayClicked);

        private async void HandlePlayClicked(ClickEvent _)
        {
            playButton.SetEnabled(false);
            playButton.text = "Loading...";
            await Awaitable.NextFrameAsync();

            onClickPlay?.Invoke();
            await GameState.SwitchTo(GameState.Playing);
        }
    }
}
