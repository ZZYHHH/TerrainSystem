using UnityEngine;
using System.Collections;

public static class BaseNoise
{

    public enum NormalizeMode { Local, Global };

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre,int testMapwidth)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX;
            float offsetY;
            if (testMapwidth == 0)
            {
                offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
                offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            }
            else
            {
                offsetX= settings.offset.x + sampleCentre.x;
                offsetY = -settings.offset.y - sampleCentre.y;
            }    
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
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
                int testMapW = testMapwidth;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX, sampleY;
                    float perlinValue;
                    if (testMapW == 0)
                    {
                        sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                        sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;
                        perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    }
                    else
                    {
                        sampleX = (x + octaveOffsets[i].x);
                        sampleY = (y + octaveOffsets[i].y);
                        perlinValue = -1;
                        int size = testMapW * mapWidth / settings.scale;
                        if (sampleX >= 0 && sampleX < size && sampleY >= 0 && sampleY < size)
                            perlinValue = settings.testValue[(int)sampleX, (int)sampleY];

                    }

                    //sampleX -= (Mathf.FloorToInt( sampleX )/ testMapW - (sampleX > 0 ? 0 : 1)) * testMapW;
                    //sampleY -= (Mathf.FloorToInt(sampleY) / testMapH - (sampleY > 0 ? 0 : 1)) * testMapH;                

                    noiseHeight += perlinValue * amplitude;
                    //noiseHeight += Mathf.Abs(perlinValue * amplitude);
                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    //float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight );
                    noiseMap[x, y] = Mathf.Clamp(noiseMap[x, y], 0, int.MaxValue);
                }
               
            }
        }

        if (settings.normalizeMode == NormalizeMode.Local)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(maxLocalNoiseHeight, minLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }

        return noiseMap;
    }

}

[System.Serializable]
public class NoiseSettings
{
    public BaseNoise.NormalizeMode normalizeMode;

    public int scale = 50;

    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2;

    public int seed;
    public Vector2 offset;

    public Texture2D testMap;
    public float[,] testValue;
    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 1);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
    public NoiseSettings()
    {

    }
}