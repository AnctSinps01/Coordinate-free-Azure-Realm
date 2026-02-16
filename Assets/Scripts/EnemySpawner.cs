using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    
    [Header("地图范围设置")]
    public bool useMapGenerator = true;
    public MapGenerator mapGenerator;
    public float mapWidth = 20f;
    public float mapHeight = 20f;
    public float spawnOffset = 1f;
    
    private float timer;
    private float currentWidth;
    private float currentHeight;
    
    void Start()
    {
        UpdateMapSize();
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }
    
    void UpdateMapSize()
    {
        if (useMapGenerator && mapGenerator != null)
        {
            currentWidth = mapGenerator.width + 1f;
            currentHeight = mapGenerator.height + 1f;
        }
        else
        {
            currentWidth = mapWidth;
            currentHeight = mapHeight;
        }
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: enemyPrefab 未设置！");
            return;
        }
        
        UpdateMapSize();
        
        Vector3 spawnPosition = GetRandomEdgePosition();
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
    
    Vector3 GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);
        float halfWidth = currentWidth / 2f;
        float halfHeight = currentHeight / 2f;
        
        switch (edge)
        {
            case 0: // 上边
                return new Vector3(
                    Random.Range(-halfWidth, halfWidth),
                    halfHeight + spawnOffset,
                    0f
                );
            case 1: // 右边
                return new Vector3(
                    halfWidth + spawnOffset,
                    Random.Range(-halfHeight, halfHeight),
                    0f
                );
            case 2: // 下边
                return new Vector3(
                    Random.Range(-halfWidth, halfWidth),
                    -halfHeight - spawnOffset,
                    0f
                );
            case 3: // 左边
                return new Vector3(
                    -halfWidth - spawnOffset,
                    Random.Range(-halfHeight, halfHeight),
                    0f
                );
            default:
                return Vector3.zero;
        }
    }
    
    /// <summary>
    /// 清空所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }
    }
    
    /// <summary>
    /// 重置生成器（清空计时器）
    /// </summary>
    public void Reset()
    {
        timer = 0f;
        UpdateMapSize();
    }
    
    void OnDrawGizmos()
    {
        float gizmoWidth = currentWidth;
        float gizmoHeight = currentHeight;
        
        if (useMapGenerator && mapGenerator != null)
        {
            gizmoWidth = mapGenerator.width + 1f;
            gizmoHeight = mapGenerator.height + 1f;
        }
        else if (Application.isPlaying == false)
        {
            gizmoWidth = mapWidth;
            gizmoHeight = mapHeight;
        }
        
        float halfWidth = gizmoWidth / 2f + spawnOffset;
        float halfHeight = gizmoHeight / 2f + spawnOffset;
        
        Vector3 topLeft = new Vector3(-halfWidth, halfHeight, 0f);
        Vector3 topRight = new Vector3(halfWidth, halfHeight, 0f);
        Vector3 bottomRight = new Vector3(halfWidth, -halfHeight, 0f);
        Vector3 bottomLeft = new Vector3(-halfWidth, -halfHeight, 0f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
    
    void OnValidate()
    {
        if (spawnInterval < 0.1f)
            spawnInterval = 0.1f;
    }
}
