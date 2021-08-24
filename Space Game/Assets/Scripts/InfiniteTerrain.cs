using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public const int maxDistance = 450;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    private static MapGenerator mapGenerator;
    private int chunkSize;
    private int chunksVivisible;
    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVivisible = Mathf.RoundToInt(maxDistance / chunkSize);
        mapGenerator = FindObjectOfType<MapGenerator>();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }
    
    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVivisible; yOffset <= chunksVivisible; yOffset++)
        {
            for (int xOffset = -chunksVivisible; xOffset <= chunksVivisible; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateChunkVisibility();
                    if (terrainChunkDictionary[viewedChunkCoord].isVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize,transform, mapMaterial));
                }
            }
        }
    }


    public class TerrainChunk
    {
        private Vector2 pos;
        private GameObject meshObject;
        private Bounds bounds;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            pos = coord * size;
            bounds = new Bounds(pos, Vector3.one * size);
            Vector3 positionV3 = new Vector3(pos.x, 0, pos.y);
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);
            
            mapGenerator.RequestMapData(OnMapDataReceived);
        }
        public void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData,OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.createMesh();
        }

        public void UpdateChunkVisibility()
        {
            float playerPosFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool isChunkVisible = playerPosFromNearestEdge <= maxDistance;
            SetVisible(isChunkVisible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LoDMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;

        public LoDMesh(int lod)
        {
            this.lod = lod;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.createMesh();
            hasMesh = true;
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData,OnMeshDataReceived);
        }
    }
}
