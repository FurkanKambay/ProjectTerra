using System.Collections.Generic;
using SaintsField;
using SaintsField.Playa;
using Tulip.Core;
using Tulip.Data;
using Tulip.Data.Items;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tulip.GameWorld
{
    public class WorldGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] World world;

        [Header("Config")]
        [SerializeField, Expandable] WorldGenConfig config;

        private float[,] perlinNoise;
        private List<TileChangeData> walls;
        private List<TileChangeData> blocks;
        private List<TileChangeData> curtains;

        // ReSharper disable once UnusedMember.Local
        [Button] async void GenerateWorld()
        {
            await GenerateData();
            await SetupWorld();
        }

        private void OnEnable() => GameState.OnGameStateChange += HandleGameStateChange;
        private void OnDisable() => GameState.OnGameStateChange -= HandleGameStateChange;

        private async void Start() => await GenerateData();

        private async Awaitable SetupWorld()
        {
            world.Size = new Vector2Int(config.Width, config.Height);

            world.ResetTilemaps();
            world.SetTiles(TileType.Wall, walls.ToArray());
            world.SetTiles(TileType.Block, blocks.ToArray());
            world.SetTiles(TileType.Curtain, curtains.ToArray());
            world.isReadonly = false;

            await Awaitable.NextFrameAsync();
        }

        private async Awaitable GenerateData()
        {
            CalculateNoise();

#if !UNITY_WEBGL
            await Awaitable.BackgroundThreadAsync();
#endif

            int tileCount = config.Width * config.Height;
            walls = new List<TileChangeData>(tileCount);
            blocks = new List<TileChangeData>(tileCount);
            curtains = new List<TileChangeData>(tileCount);

            int center = config.Width / 2;
            int snowDistance = center - config.SnowDistance;
            int sandDistance = center + config.SandDistance;

            for (int y = 0; y < config.Height; y++)
            {
                float densityCutoff = config.HeightDensityCurve.Evaluate(y / (float)config.Height);

                for (int x = 0; x < config.Width; x++)
                {
                    Placeable wall = config.StoneWall;
                    float noise = perlinNoise[x, y];

                    bool isGrassHeight = config.Height - y <= config.GrassLayerHeight;

                    Placeable block =
                        isGrassHeight && x < snowDistance ? config.Snow :
                        isGrassHeight && x >= sandDistance ? config.Sand :
                        noise > densityCutoff ? null :
                        noise < config.OreCutoff ? config.CopperVein :
                        isGrassHeight ? config.Grass :
                        config.Stone;

                    // int index = (config.width * y) + x;
                    var cell = new Vector3Int(x, y, 0);

                    walls.Add(new TileChangeData(cell, wall.RuleTile, wall.Color, Matrix4x4.identity));

                    if (block)
                        blocks.Add(new TileChangeData(cell, block.RuleTile, block.Color, Matrix4x4.identity));

                    curtains.Add(new TileChangeData(cell, null, Color.white, Matrix4x4.identity));
                }
            }

            walls.TrimExcess();
            blocks.TrimExcess();
            curtains.TrimExcess();

            await Awaitable.MainThreadAsync();
        }

        private void CalculateNoise()
        {
            perlinNoise = new float[config.Width, config.Height];

            for (float y = 0; y < config.Height; y++)
            {
                for (float x = 0; x < config.Width; x++)
                {
                    float sample = Mathf.PerlinNoise(
                        config.PerlinOffset.x + (x / config.Width * (config.Width * config.densityFactor)),
                        config.PerlinOffset.y + (y / config.Height * (config.Height * config.densityFactor))
                    );

                    perlinNoise[(int)x, (int)y] = Mathf.Clamp01(sample);
                }
            }
        }

        private async void HandleGameStateChange(GameState oldState, GameState newState)
        {
            if (oldState == GameState.MainMenu && newState == GameState.Playing)
                await SetupWorld();
        }
    }
}