using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    private const float scale = 5f;
    const float playerMoveThresholdForChunkUpdate = 25f;
    private const float sqrplayerMoveThresholdForChunkUpdate =
        playerMoveThresholdForChunkUpdate * playerMoveThresholdForChunkUpdate;
    public LODInfo[] detailLevels;
    public static float maxDistance;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    private Vector2 viewerPosOld;
    private static MapGenerator mapGenerator;
    private int chunkSize;
    private int chunksVivisible;
    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        maxDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVivisible = Mathf.RoundToInt(maxDistance / chunkSize);
        mapGenerator = FindObjectOfType<MapGenerator>();
        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
        if ((viewerPosOld - viewerPosition).sqrMagnitude > sqrplayerMoveThresholdForChunkUpdate)
        {
            viewerPosOld = viewerPosition;
            UpdateVisibleChunks();
        }
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
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize,detailLevels, transform, mapMaterial));
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
        private MeshCollider meshCollider;
        
        private LODInfo[] detailLevels;
        private LoDMesh[] lodMeshes;
        private LoDMesh collisionLoDMesh;
        
        private MapData mapData;
        private bool mapDataReceived;
        private int prevLodIndex = 1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            pos = coord * size;
            bounds = new Bounds(pos, Vector3.one * size);
            Vector3 positionV3 = new Vector3(pos.x, 0, pos.y);
            meshObject = new GameObject("Terrain Chunk");
            
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;
            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            
            SetVisible(false);
            lodMeshes = new LoDMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LoDMesh(detailLevels[i].lod,UpdateChunkVisibility);
                if (detailLevels[i].useForCollider)
                {
                    collisionLoDMesh = lodMeshes[i];
                }
            }
            
            mapGenerator.RequestMapData(pos,OnMapDataReceived);
        }
        public void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.textureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize,  
                MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            UpdateChunkVisibility();
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.createMesh();
        }

        public void UpdateChunkVisibility()
        {
            if (mapDataReceived)
            {
                float playerPosFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool isChunkVisible = playerPosFromNearestEdge <= maxDistance;
                if (isChunkVisible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (playerPosFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != prevLodIndex)
                    {
                        LoDMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            prevLodIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    if (lodIndex == 0)
                    {
                        if (collisionLoDMesh.hasMesh)
                        {
                            meshCollider.sharedMesh = collisionLoDMesh.mesh;
                        }
                        else if (!collisionLoDMesh.hasRequestedMesh)
                        {
                            collisionLoDMesh.RequestMesh(mapData);
                        }
                    }
                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(isChunkVisible);
            }
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
        private System.Action updateCallback;

        public LoDMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.createMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData,lod,OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceThreshold;
        public bool useForCollider;
    }
}
