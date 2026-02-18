using UnityEngine;

public class MapGenerator_old : MonoBehaviour
{
    [Header("地图设置")]
    public int width = 20;
    public int height = 20;
    
    [Header("地块预制体")]
    public GameObject tilePrefab;

    
    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        //GenerateMap();
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
    }
    
    /// <summary>
    /// 清空所有地图瓦片
    /// </summary>
    public void ClearMap()
    {
        // 销毁所有子对象（地图瓦片）
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 重新生成地图
    /// </summary>
    public void RegenerateMap()
    {
        ClearMap();
        GenerateMap();
    }
}
