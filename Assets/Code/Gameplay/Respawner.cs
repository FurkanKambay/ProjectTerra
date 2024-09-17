using SaintsField;
using Tulip.Character;
using Tulip.Core;
using Tulip.Data;
using Tulip.Data.Gameplay;
using UnityEngine;

namespace Tulip.Gameplay
{
    public class Respawner : MonoBehaviour, IRespawner
    {
        [Header("References")]
        [SerializeField, Required] HealthBase health;

        [Header("Config")]
        [SerializeField] bool autoRespawn = true;
        [SerializeField] float respawnDelay;
        [SerializeField] Vector3 respawnPosition;

        public float SecondsUntilRespawn { get; private set; }
        public bool CanRespawn => SecondsUntilRespawn <= 0;

        private TangibleEntity entity;
        private Transform subject;
        private IWorld world;

        private void Awake()
        {
            entity = health.GetComponentInParent<TangibleEntity>();
            subject = entity.transform;
            world = entity.World;
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChange += GameState_Change;
            health.OnDie += Health_Die;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChange -= GameState_Change;
            health.OnDie -= Health_Die;
        }

        private void GameState_Change(GameState oldState, GameState newState)
        {
            bool startedPlaying = newState is GameState.Playing && oldState is not GameState.Paused;

            if (newState == GameState.MainMenu || startedPlaying)
                TryRespawn();
        }

        private void Update()
        {
            if (health.IsAlive)
                return;

            SecondsUntilRespawn -= Time.deltaTime;

            if (autoRespawn)
                TryRespawn();
        }

        [ContextMenu(nameof(TryRespawn))]
        public void TryRespawn()
        {
            if (!CanRespawn)
                return;

            SecondsUntilRespawn = 0;
            SetPosition();
            health.Revive();
        }

        private void SetPosition()
        {
            Vector2Int cell = world.WorldToCell(respawnPosition);

            while (!world.CanAccommodate(cell, entity.EntityData.Size))
                cell.y++;

            subject.position = world.CellCenter(cell);
        }

        private void Health_Die(HealthChangeEventArgs _) => SecondsUntilRespawn = respawnDelay;
    }
}
