using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("地图设置")]
    public int width = 20;
    public int height = 20;
    
    [Header("地块预制体")]
    public GameObject tilePrefab;
    
    [Header("墙壁设置")]
    public float wallThickness = 0.5f;
    
    void Start()
    {
        GenerateMap();
    }
    
    void GenerateMap()
    {
        // 计算起始位置
        int startX = -width / 2;
        int endX = width / 2;
        int startY = -height / 2;
        int endY = height / 2;
        
        // 生成地面
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.SetParent(transform);
                tile.name = $"Tile_{x}_{y}";
            }
        }
        
        // 生成边界空气墙
        GenerateWalls(startX, endX, startY, endY);
    }
    
    void GenerateWalls(int startX, int endX, int startY, int endY)
    {
        // 计算地图边界中心点和大小
        float centerX = 0;
        float centerY = 0;
        float mapWidth = width + 1;  // 包含边界
        float mapHeight = height + 1;
        
        // 上墙
        CreateWall("Wall_Top", 
            new Vector3(centerX, endY + 0.5f + wallThickness / 2, 0),
            new Vector2(mapWidth, wallThickness));
        
        // 下墙
        CreateWall("Wall_Bottom", 
            new Vector3(centerX, startY - 0.5f - wallThickness / 2, 0),
            new Vector2(mapWidth, wallThickness));
        
        // 左墙
        CreateWall("Wall_Left", 
            new Vector3(startX - 0.5f - wallThickness / 2, centerY, 0),
            new Vector2(wallThickness, mapHeight));
        
        // 右墙
        CreateWall("Wall_Right", 
            new Vector3(endX + 0.5f + wallThickness / 2, centerY, 0),
            new Vector2(wallThickness, mapHeight));
    }
    
    void CreateWall(string name, Vector3 position, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.position = position;
        wall.transform.SetParent(transform);
        
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }
}
