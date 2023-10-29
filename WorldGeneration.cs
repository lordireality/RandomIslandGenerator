using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    public int width = 1000;
    public int height = 1000;
    public float scale = 5;
    public float offsetX = 100f;
    public float offsetY = 100f;
    public Texture2D[] terrainTextures;
    public Texture2D sandTexture;
    public GameObject treePrefab;
    public GameObject palmPrefab;
    public GameObject rockPrefab;
    public int maxRocks = 1000;

    public int maxTrees = 100;
    public int islandRadius = 500;
    int seed = 0;

    void Start()
    {
        if(seed == 0)
        {
            seed = Random.Range(0, 10000);
        }
        GenerateWorld();
    }

    void GenerateWorld()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = GenerateTerrainData();
        terrain.terrainData = terrainData;
        this.GetComponent<TerrainCollider>().terrainData = terrainData;
        PaintTextures(terrainData);
        GenerateTrees(terrainData, terrain.transform.position);
        
    }

    TerrainData GenerateTerrainData()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, 100, height);

        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float sampleX = (float)x / width * scale + offsetX;
                float sampleY = (float)y / height * scale + offsetY;
                float distanceToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(width / 2, height / 2));
                
                float heightValue = Mathf.Clamp01((islandRadius - distanceToCenter) / islandRadius) * Mathf.PerlinNoise(sampleX+ seed, sampleY+ seed);
                heights[x, y] = heightValue;
            }
        }
        
        terrainData.SetHeights(0, 0, heights);

        return terrainData;
    }

    void PaintTextures(TerrainData terrainData)
    {
        int textureCount = terrainTextures.Length;
        SplatPrototype[] splatPrototypes = new SplatPrototype[textureCount+1];
        Debug.Log(terrainTextures.Length);
        for (int i = 0; i < textureCount; i++)
        {
            splatPrototypes[i] = new SplatPrototype();
            splatPrototypes[i].texture = terrainTextures[i];
            splatPrototypes[i].tileSize = new Vector2(1, 1);
        }
        splatPrototypes[textureCount] = new SplatPrototype();
        splatPrototypes[textureCount].texture = sandTexture;
        splatPrototypes[textureCount].tileSize = new Vector2(1, 1);

        terrainData.splatPrototypes = splatPrototypes;

        int splatMapWidth = terrainData.alphamapWidth;
        int splatMapHeight = terrainData.alphamapHeight;
        float[,,] splatMap = new float[splatMapWidth, splatMapHeight, textureCount+1];

        for (int x = 0; x < splatMapWidth; x++)
        {
            for (int y = 0; y < splatMapHeight; y++)
            {
                float[] splatWeights = new float[textureCount+1];
                float terrainHeight = terrainData.GetHeight(Mathf.FloorToInt((float)y / splatMapHeight * height),Mathf.FloorToInt((float)x / splatMapWidth * width));
                
                if (terrainHeight < 13f)
                {
                    
                    splatWeights[textureCount] = 1f;
                } else
                {
                    for (int i = 0; i < textureCount; i++)
                    {
                        
                        splatWeights[i] = Random.value;
                        
                    }
                    NormalizeWeights(splatWeights);

                }

                

                for (int i = 0; i <= textureCount; i++)
                {
                    splatMap[x, y, i] = splatWeights[i];
                }
            }
        }
        
        terrainData.SetAlphamaps(0, 0, splatMap);

    }
        void GenerateTrees(TerrainData terrainData, Vector3 terrainPosition)
    {
        TreePrototype[] treePrototypes = new TreePrototype[3];
        treePrototypes[0] = new TreePrototype();
        treePrototypes[0].prefab = palmPrefab;
        treePrototypes[1] = new TreePrototype();
        treePrototypes[1].prefab = treePrefab;
        treePrototypes[2] = new TreePrototype();
        treePrototypes[2].prefab = rockPrefab;

        terrainData.treePrototypes = treePrototypes;

        TreeInstance[] treeInstances = new TreeInstance[maxTrees+maxRocks];

        for (int i = 0; i < maxTrees; i++)
        {
            
            
            var xpos = Random.Range(0f, 1f);
            //var xpos = Mathf.RoundToInt(xposRaw);
            var xposTerrain = Mathf.RoundToInt(xpos * width);
            var ypos = Random.Range(0f, 1f);
            var yposTerrain = Mathf.RoundToInt(ypos * height);
            //var ypos = Mathf.RoundToInt(yposRaw);
            var zpos = terrainData.GetInterpolatedHeight(xpos, ypos)/100;
            
            if(zpos < 0.9){
                i--;
            } else if (zpos > 0.11 && zpos < 0.23)
            {
                TreeInstance treeInstance = new TreeInstance();
                treeInstance.prototypeIndex = 0;
                treeInstance.position = new Vector3(xpos, zpos, ypos);
                treeInstance.widthScale = 1f;
                treeInstance.heightScale = 1f;
                treeInstance.color = Color.white;
                treeInstance.lightmapColor = Color.white;

                treeInstances[i] = treeInstance;
            } else
            {
                TreeInstance treeInstance = new TreeInstance();
                treeInstance.prototypeIndex = 1;
                treeInstance.position = new Vector3(xpos, zpos, ypos);
                treeInstance.widthScale = 1f;
                treeInstance.heightScale = 1f;
                treeInstance.color = Color.white;
                treeInstance.lightmapColor = Color.white;
                treeInstances[i] = treeInstance;
            }
            
        }
        for (int i = maxTrees; i < maxTrees+maxRocks; i++)
        {
            Debug.Log(i);
            var xpos = Random.Range(0f, 1f);
            //var xpos = Mathf.RoundToInt(xposRaw);
            var xposTerrain = Mathf.RoundToInt(xpos * width);
            var ypos = Random.Range(0f, 1f);
            var yposTerrain = Mathf.RoundToInt(ypos * height);
            //var ypos = Mathf.RoundToInt(yposRaw);
            var zpos = terrainData.GetInterpolatedHeight(xpos, ypos) / 100;
            if (zpos < 0.11)
            {
                i--;
            } else
            {
                TreeInstance treeInstance = new TreeInstance();
                treeInstance.prototypeIndex = 2;
                treeInstance.position = new Vector3(xpos, zpos, ypos);
                treeInstance.widthScale = 1f;
                treeInstance.heightScale = 1f;
                treeInstance.color = Color.white;
                treeInstance.lightmapColor = Color.white;

                treeInstances[i] = treeInstance;
            }

        }

        terrainData.treeInstances = treeInstances;
        terrainData.RefreshPrototypes();
    }
    void NormalizeWeights(float[] weights)
    {
        float sum = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
        }

        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] /= sum;
        }
    }
}