using System.Collections.Generic;
using System.Linq;
using SaintsField;
using SaintsField.Playa;
using Tulip.Character;
using Tulip.Core;
using Tulip.Data;
using Tulip.GameWorld;
using UnityEditor;
using UnityEngine;

namespace Tulip.Gameplay
{
    public class EnemySpawner : MonoBehaviour
    {
        [LayoutGroup("References", ELayout.Background | ELayout.TitleOut)]
        [SerializeField] World world;
        [SerializeField] new Camera camera;
        [SerializeField] Transform spawnParent;

        [LayoutGroup("Config", ELayout.Background | ELayout.TitleOut)]
        [LayoutGroup("Config/Spawning", ELayout.TitleOut)]
        [SerializeField, Min(0)] int maxSpawns = 100;

        [OverlayRichLabel("<color=gray>tiles")]
        [SerializeField, Min(0)] int radius = 5;

        [OverlayRichLabel("<color=gray>sec")]
        [SerializeField, Min(0)] float interval = 10f;

        [OverlayRichLabel("<color=gray>sec")]
        [SerializeField, Min(0)] float gracePeriod;

        [SerializeField] EntitySpawnPoolData entitySpawnPoolData;

        private IEnumerable<Vector2Int> suitableCells;

        private bool isActive;
        private float timeSinceLastSpawn;

        private void OnEnable() => GameManager.OnGameStateChange += Game_StateChanged;
        private void OnDisable() => GameManager.OnGameStateChange -= Game_StateChanged;

        private void Update()
        {
            if (!isActive)
            {
                timeSinceLastSpawn = -gracePeriod;
                return;
            }

            timeSinceLastSpawn += Time.deltaTime;

            if (timeSinceLastSpawn < interval)
                return;

            if (TrySpawnEnemy())
                timeSinceLastSpawn = 0;
        }

        private void Game_StateChanged(GameState oldState, GameState newState)
        {
            isActive = newState != GameState.MainMenu;

            if (!isActive)
                DestroyAllSpawns();
        }

        private bool TrySpawnEnemy()
        {
            if (spawnParent.childCount >= maxSpawns)
                return false;

            if (entitySpawnPoolData.Amount == 0)
                return false;

            EntityData entityData = GetRandomEnemy();
            suitableCells = GetSuitableCells(entityData);

            if (!suitableCells.Any())
                return false;

            Vector2Int baseCell = GetRandomSpawnCell();
            Vector2Int centerCell = new(baseCell.x + (entityData.Size.x / 2), baseCell.y);

            TangibleEntity spawnedEnemy = Spawn(entityData.Prefab, world.CellCenter(centerCell));
            spawnedEnemy.SetResidence(world, baseCell);

            if (entityData.IsStatic)
                world.TryAddStaticEntity(baseCell, spawnedEnemy);

            return true;
        }

        private TangibleEntity Spawn(GameObject prefab, Vector3 position) =>
            Instantiate(prefab, position, Quaternion.identity, spawnParent).GetComponent<TangibleEntity>();

        private EntityData GetRandomEnemy() =>
            entitySpawnPoolData[Random.Range(0, entitySpawnPoolData.Amount)];

        private Vector2Int GetRandomSpawnCell() =>
            suitableCells.ElementAt(Random.Range(0, suitableCells.Count()));

        private IEnumerable<Vector2Int> GetSuitableCells(EntityData entityData)
        {
            if (!isActive)
                yield break;

            Vector3 cameraExtents = new(camera.orthographicSize * camera.aspect, camera.orthographicSize);
            Vector3 spawnExtents = new(cameraExtents.x + radius, cameraExtents.y + radius);
            Vector3 cameraCenter = camera.transform.position;

            Vector2Int camTopRight = world.WorldToCell(cameraCenter + cameraExtents);
            Vector2Int camBottomLeft = world.WorldToCell(cameraCenter - cameraExtents);
            Vector2Int spawnTopRight = world.WorldToCell(cameraCenter + spawnExtents);
            Vector2Int spawnBottomLeft = world.WorldToCell(cameraCenter - spawnExtents);

            for (int y = spawnBottomLeft.y; y <= spawnTopRight.y; y++)
            {
                for (int x = spawnBottomLeft.x; x <= spawnTopRight.x; x++)
                {
                    if (y <= camTopRight.y && y >= camBottomLeft.y && x <= camTopRight.x && x >= camBottomLeft.x)
                        continue;

                    var cell = new Vector2Int(x, y);

                    if (entityData.CanSpawnAt(world, cell))
                        yield return cell;
                }
            }
        }

#if UNITY_EDITOR

        [Button, LayoutEnd]
        private void DestroyAllSpawns()
        {
            for (int i = 0; i < spawnParent.childCount; i++)
                Destroy(spawnParent.GetChild(i).gameObject);

            world.ClearEntities();
        }

        private void OnDrawGizmosSelected()
        {
            if (entitySpawnPoolData.Amount == 0)
                return;

            Handles.color = Color.yellow;

            foreach (Vector2Int cell in GetSuitableCells(entitySpawnPoolData[0]))
                Handles.DrawSolidDisc(world.CellCenter(cell), Vector3.forward, 0.2f);
        }
#endif
    }
}
