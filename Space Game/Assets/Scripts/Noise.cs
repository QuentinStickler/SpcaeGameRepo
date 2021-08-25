using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise{
    
    public enum NormalizeMode
    {
        Local,
        Global
    }

    public static float[,] generateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity,Vector2 offset, NormalizeMode normalizeMode)
    {
        System.Random random = new System.Random(seed);
        Vector2[] octaveOffset = new Vector2[octaves];
        float maxPossibleHeight = 0f;
        float amplitude = 1;
        float frequency = 1;
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetY = random.Next(-100000, 100000) - offset.y;
            octaveOffset[i] = new Vector2(offsetX, offsetY);
            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }
        if (scale <= 0) {scale = 0.0001f;}
        float[,] noiseMap = new float[width, height];

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        
        for(int y = 0; y < height; y ++)
        {
            for (int x = 0; x < width; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x-halfWidth +  + octaveOffset[i].x)/scale * frequency;
                    float sampleY = (y-halfHeight + octaveOffset[i].y)/scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity; 
                }

                if (noiseHeight > maxNoiseHeight) { maxNoiseHeight = noiseHeight;}
                else if (noiseHeight < minNoiseHeight) { minNoiseHeight = noiseHeight;}
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x,y] + 1) /  maxPossibleHeight;       //Wenn wir einen endless terrain machen, müssen wir erst die maximale Height calculaten, wodurch die Seams zwischen den Chunks weggehen
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight,0, int.MaxValue);
                }
            }
        }
        return noiseMap;
    }
}
 