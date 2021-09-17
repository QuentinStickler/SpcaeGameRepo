using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour     //macht die Noisemap zu ner texture und applied die auf n Plane
{
    public Renderer textureRenderer;
    public MeshFilter filter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture; //Applied die neue Texture im Editor direkt und nicht bei Runtime
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void drawMesh(MeshData data, Texture2D texture)
    {
        filter.sharedMesh = data.createMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
