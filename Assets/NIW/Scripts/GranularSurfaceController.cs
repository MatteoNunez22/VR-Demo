using UnityEngine;
using System.Collections;

public class GranularSurfaceController : MonoBehaviour
{
    Material SnowMaterial;

    public Texture2D FootprintTexture;

    public int MapDimensions = 256;
    Texture2D HeightMap;

    void InitNoiseMap(Texture2D map)
    {
        for (int i = 0; i < map.height; i++)
        {
            for (int j = 0; j < map.width; j++)
            {
                float f = Mathf.PerlinNoise(i * 0.1f, j * 0.1f) + Mathf.PerlinNoise(i * 0.25f, j * 0.25f) * 0.25f + Mathf.PerlinNoise(i * 0.5f, j * 0.5f) * 0.5f;
                f = Map(f, 0, 1, 0.66f, 1.0f) * 0.25f;
                map.SetPixel(j, i, new Color(f, f, f));
            }

        }
        map.Apply();
    }

    public void Start()
    {
        HeightMap = new Texture2D(MapDimensions, MapDimensions);
        SnowMaterial = gameObject.GetComponent<Material>();
        InitNoiseMap(HeightMap);
        //gameObject.GetComponent<Renderer>().material.SetTexture("_HeightMap", HeightMap);
    }

    float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        float outVal = ((value - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin);
        return outVal;
    }

    public void ApplyStep(Vector3 position)
    {
        gameObject.GetComponent<Kvant.Stream>().footPosition = position;
    }
}
