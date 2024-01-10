using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using static GenerateWorld.Noise;

public class GenerateWorld : MonoBehaviour
{
    [Header("Noise Settings")]
    [SerializeField] public float scale = 10f;
    [SerializeField] public int seed = 1234567, chunkSize = 20, octaves = 4;
    [Range(-1f, 1f)]
    [SerializeField] public float population = 0.5f, persistance = 0.5f;
    [SerializeField] public float lacunarity = 1.8f;
    [Header("Tilemap References")]
    [SerializeField] public Tilemap map;
    [SerializeField] public TileBase landTile, waterTile = null;
    [SerializeField] public Grid mainGrid;
    [Header("View Settings")]
    [SerializeField] public Transform player;
    public Vector2Int latestChunkPosition;
    [Range(1, 25)]
    [SerializeField] public int maxChunkViewDistance;

    
    public Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>()
        , unloadedChunks = new Dictionary<Vector2Int, Chunk>();
    public Queue<Chunk> chunksToBeUnloaded = new Queue<Chunk>();
    public Stack<Chunk> chunksToBeLoaded = new Stack<Chunk>();
    void Start()
    {
        UnityEngine.Random.InitState(seed);
    }

    private void GenerateChunk(Vector2Int chunkPosition)
    {
        Chunk chunk = new Chunk(chunkPosition, chunkSize, seed, scale, octaves, population, lacunarity, landTile, waterTile, map, mainGrid);
        this.loadedChunks[chunkPosition] = chunk;
    }

    public void UpdateTile(Vector3Int cellPosition)
    {
        if (map.GetTile(cellPosition) == null) return;
        Chunk.UpdateTile((Vector2Int) cellPosition, landTile);
        map.SetTile(cellPosition, landTile);
    }

    public Chunk getChunkByCellPosition(Vector2Int cellPosition)
    {
        Vector2Int chunkPosition = cellPosition / chunkSize;
        if (!loadedChunks.ContainsKey(chunkPosition))
        {
            return null;
        }
        return loadedChunks[chunkPosition];
    }

    public Vector2Int worldPositionToChunkPosition(Vector2 position)
    {
        Vector2Int chunkPosition = new Vector2Int((int)position.x, (int)position.y);
        chunkPosition.x = (int)Mathf.Ceil(chunkPosition.x / this.chunkSize);
        chunkPosition.y = -(int)Mathf.Ceil(chunkPosition.y / this.chunkSize) + 1;

        return chunkPosition;
    }

    void Update()
    {
        if (chunksToBeUnloaded.Count > 0)
        {
            Chunk c = chunksToBeUnloaded.Dequeue();
            c.UnloadChunk();
        }

        if (chunksToBeLoaded.Count > 0)
        {
            Chunk c = chunksToBeLoaded.Pop();
            c.LoadChunk();
            if (!this.loadedChunks.ContainsKey(c.chunkPosition))
            {
                this.loadedChunks[c.chunkPosition] = c;
            }
        }
        Vector2Int playerChunkPosition = this.worldPositionToChunkPosition(this.player.position);

        //Vector2Int playerChunkPosition = (Vector2Int)(world.mainGrid.WorldToCell(world.player.position) / world.chunkSize) * new Vector2Int(1, -1);
        if (this.latestChunkPosition != null && this.latestChunkPosition == playerChunkPosition) return;
        new UpdateChunkThread(this, playerChunkPosition);
    }

    public class UpdateChunkThread
    {
        public UpdateChunkThread(GenerateWorld world, Vector2Int playerChunkPosition) {
            Vector2Int unloadPositions = new Vector2Int(world.latestChunkPosition.x - playerChunkPosition.x, world.latestChunkPosition.y - playerChunkPosition.y);
            world.latestChunkPosition = playerChunkPosition;

            ThreadStart checkUnload = delegate {
                foreach (KeyValuePair<Vector2Int, Chunk> key in world.loadedChunks)
                {
                    Chunk c = key.Value;
                    if (c.isLoaded)
                    {
                        if (Mathf.Abs(c.chunkPosition.x  - playerChunkPosition.x) > world.maxChunkViewDistance || Mathf.Abs(c.chunkPosition.y - playerChunkPosition.y) > world.maxChunkViewDistance)
                        {
                            if (!world.chunksToBeUnloaded.Contains(world.loadedChunks[c.chunkPosition]))
                            {
                                foreach (Vector3Int pos in c.positions)
                                {
                                    if (Chunk.modifiedTiles.ContainsKey((Vector2Int)pos))
                                    {
                                        c.localModifiedTiles[pos] = Chunk.modifiedTiles[(Vector2Int)pos];

                                    }
                                }
                                c.UpdateChunkTiles();
                                world.chunksToBeUnloaded.Enqueue(world.loadedChunks[c.chunkPosition]);
                            }
                        }
                    }
                }
            };

            ThreadStart checkLoad = delegate
            {
                for (int i = playerChunkPosition.x - world.maxChunkViewDistance; i < playerChunkPosition.x + world.maxChunkViewDistance; i++)
                {
                    for (int j = playerChunkPosition.y - world.maxChunkViewDistance; j < playerChunkPosition.y + world.maxChunkViewDistance; j++)
                    {
                        Vector2Int position = new Vector2Int(i, j);
                        if (!world.loadedChunks.ContainsKey(position))
                        {
                            world.GenerateChunk(position);
                        }
                        if (!world.loadedChunks[position].isLoaded)
                        {
                            world.chunksToBeLoaded.Push(world.loadedChunks[position]);
                        }
                    }
                }

            };

            new Thread(checkUnload).Start();
            new Thread(checkLoad).Start();
        }

    }

    public static class Noise
    {
        // Source code from @SebLague :: https://github.com/SebLague/Procedural-Landmass-Generation/tree/master/Proc%20Gen%20E03
        public enum NormalizeMode { Local, Global };
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) - offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }

            if (scale <= 0)
            {
                scale = 0.0001f;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;


            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {

                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                }
            }


            return noiseMap;
        }
    }

    public class Chunk
    {
        public Vector2Int chunkPosition { get; private set; }
        private Vector2Int chunkOffset { get; }
        private float[,] noiseMap;
        public static int chunkSize { get; private set; }
        private Tilemap map;
        public static Dictionary<Vector2Int, TileBase> modifiedTiles = new Dictionary<Vector2Int, TileBase>();
        public Dictionary<Vector3Int, TileBase> localModifiedTiles = new Dictionary<Vector3Int, TileBase>();
        public bool isLoaded { get; private set; } = false;
        public TileBase[] tiles, emptyTiles;
        public Vector3Int[] positions { get; private set; }
        public Chunk(Vector2Int chunkPosition, int chunkSize, int seed, float scale, int octaves, float population, float lacunarity, TileBase land, TileBase water, Tilemap map, Grid mainGrid)
        {
            this.chunkPosition = chunkPosition;
            this.chunkOffset = chunkPosition * chunkSize;
            Chunk.chunkSize = chunkSize;
            this.map = map;
            this.noiseMap = GenerateWorld.Noise.GenerateNoiseMap(chunkSize, chunkSize, seed, scale, octaves, population, lacunarity, chunkOffset, NormalizeMode.Global);

            this.positions = new Vector3Int[chunkSize * chunkSize];
            this.tiles = new TileBase[chunkSize * chunkSize];
            this.emptyTiles = new TileBase[chunkSize * chunkSize];
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    Vector2Int absoluteTilemapPosition = new Vector2Int(x + chunkOffset.x, y - chunkOffset.y);
                    positions[x * chunkSize + y] = (Vector3Int)absoluteTilemapPosition;
                    if (noiseMap[x, y] > population)
                    {
                        tiles[x * chunkSize + y] = land;
                    }
                    else
                    {
                        tiles[x * chunkSize + y] = water;
                    }
                    emptyTiles[x * chunkSize + y] = null;
                }
            }
            isLoaded = false;
        }

        public static void UpdateTile(Vector2Int position, TileBase tile)
        {
            modifiedTiles[position] = tile;
        }

        public void LoadChunk()
        {
            this.isLoaded = true;

            map.SetTiles(this.positions, this.tiles);

        }

        public void UpdateChunkTiles()
        {
            if (this.localModifiedTiles.Count > 0)
            {
                int index = 0;
                foreach (Vector3Int position in this.positions)
                {
                    if (this.localModifiedTiles.ContainsKey(position))
                    {
                        this.tiles[index] = this.localModifiedTiles[position];
                    }
                    ++index;
                }
                this.localModifiedTiles.Clear();
            }
        }


        public void SetChunkTiles(TileBase[] tiles)
        {
            this.tiles = tiles;
        }
        public void UnloadChunk()
        {
            map.SetTiles(positions, emptyTiles);
            this.isLoaded = false;
        }
    }

}
