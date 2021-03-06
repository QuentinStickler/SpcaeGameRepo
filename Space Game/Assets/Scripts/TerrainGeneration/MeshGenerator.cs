using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData generateMesh(float[,] heightMap, float heightMultiplier,AnimationCurve _heightCurve, int levelOfDetail)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        int meshSize = width - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = width - 2;
        
        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;
        
        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int[,] vertexIndicesMap = new int[height, width];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;
        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == width - 1 || x == 0 || x == height - 1;
                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }
        

        for (int y = 0; y < height; y+= meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x+= meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 percent = new Vector2((x-meshSimplificationIncrement) / (float) meshSize, (y-meshSimplificationIncrement) / (float) meshSize);
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - percent.y * meshSizeUnsimplified);
                
                meshData.AddVertex(vertexPosition,percent,vertexIndex);

                if (x < width - 1 && y < height - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x+meshSimplificationIncrement, y];
                    int c = vertexIndicesMap[x, y+meshSimplificationIncrement];
                    int d = vertexIndicesMap[x+meshSimplificationIncrement, y+meshSimplificationIncrement];
                    meshData.addTriangle(a,d,c);
                    meshData.addTriangle(d,a,b);
                }
                vertexIndex++;
            }
        }
        meshData.BakeNormals();

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    private Vector3[] borderVertices;
    private int[] borderTriangles;
    private int triangleIndex;
    private int borderTriangleIndex;
    private Vector3[] bakedNormals;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshHeight * meshWidth];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        borderVertices = new Vector3[meshWidth * 4 + 4];
        borderTriangles = new int[meshWidth * 24];
    }

    public void AddVertex(Vector3 vertexPos, Vector2 verticesUV, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPos;
        }
        else
        {
            vertices[vertexIndex] = vertexPos;
            uvs[vertexIndex] = verticesUV;
        }
    }

    public void addTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex+1] = b;
            borderTriangles[borderTriangleIndex+2] = c;
            borderTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex+1] = b;
            triangles[triangleIndex+2] = c;
            triangleIndex += 3;
        }

    }
    public Vector3[] CalculateNormals() //Selber die Normals von den R??ndern der Chunks berechnen, damit die einen smoothen ??bergang zu anderen Chunks hat
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex+1];
            int vertexIndexC = triangles[normalTriangleIndex+2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }
        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex+1];
            int vertexIndexC = borderTriangles[normalTriangleIndex+2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }
        

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }
    public Mesh createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = bakedNormals;
        return mesh;
    }
    
}