using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tulip.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused
    }

    public class GameManager : MonoBehaviour
    {
        public delegate void GameStateChangeEvent(GameState oldState, GameState newState);

        public static event GameStateChangeEvent OnGameStateChange;

        public static GameState CurrentState { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() => CurrentState = GameState.MainMenu;

        private void OnEnable() => Application.wantsToQuit += IsSafeToQuit;
        private void OnDisable() => Application.wantsToQuit -= IsSafeToQuit;

        public static void SwitchTo(GameState newState)
        {
            if (newState == CurrentState)
                return;

            GameState oldState = CurrentState;
            CurrentState = newState;

            UpdateTimeScale();
            UpdateInputs();
            OnGameStateChange?.Invoke(oldState, newState);
        }

        public static void SetPaused(bool shouldPause) => SwitchTo(
            CurrentState switch
            {
                GameState.Playing when shouldPause => GameState.Paused,
                GameState.Paused when !shouldPause => GameState.Playing,
                _ => CurrentState
            }
        );

        public static void QuitGame()
        {
            if (!IsSafeToQuit())
                return;

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void UpdateTimeScale() => Time.timeScale = CurrentState switch
        {
            GameState.Paused when Settings.Gameplay.AllowPause => 0,
            _ => 1
        };

        private static void UpdateInputs()
        {
            InputActionMap playerControls = InputSystem.actions.actionMaps[0];
            InputActionMap uiControls = InputSystem.actions.actionMaps[1];

            if (CurrentState == GameState.Playing)
            {
                playerControls.Enable();
                uiControls.Disable();
            }
            else
            {
                playerControls.Disable();
                uiControls.Enable();
            }
        }

        private static bool IsSafeToQuit()
        {
            if (CurrentState is not (GameState.Playing or GameState.Paused))
                return true;

            // TODO: save game before quitting
            Debug.LogWarning("Quit requested. Should save game first.");

            return true;
        }
    }
}
