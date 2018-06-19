using UnityEngine;

public class ProceduralTerrainGenerator : MonoBehaviour {

    [Header ("Terrain Paramters;")]
    public int width  = 256;
    public int height = 256;
    public int depth = 20;
    public float scale = 20.0f;

    private void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;

        terrainData.size = new Vector3(width, depth, height);

        terrainData.SetHeights(0, 0, GenerateHeightMap());

        return terrainData;
    }

    float[,] GenerateHeightMap()
    {
        float[,] heightMap = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heightMap[x, y] = RMNoise(x, y);
            }
        }
        return heightMap;
    }

    float RMNoise(int x, int y)
    {
        //We are going to create Ridged Multifractal Noise.
        float xCoordinate = (float)x / width * scale;
        float yCoordinate = (float)y / height * scale;

        //First get a sample of perlin noise between -1 and 1
        float p = (Mathf.PerlinNoise(xCoordinate, yCoordinate) * 2f) - 1f;

        //Get billow noise by getting the abs of this
        p = Mathf.Abs(p);

        //Finally invert the noise.
        p = Mathf.Abs(p - 1f);
        return p;
    }
}
