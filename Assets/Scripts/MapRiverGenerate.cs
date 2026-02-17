using UnityEngine;
using System;
using System.Collections.Generic;

public class MapRiverGenerate : MonoBehaviour
{
    public readonly int[,] MappingTable =
    {
        {  0,  0}, {  0,  0}, {  1,  0}, {  2,  0}, {  0,  0}, {  0,  0}, {  2,  4}, {  5,  0}, 
        {  1,  3}, {  2,  5}, {  4,  0}, {  6,  0}, {  1,  3}, {  2,  5}, {  7,  5}, { 10,  5}, 
        {  1,  1}, {  1,  1}, {  4,  1}, {  7,  1}, {  2,  1}, {  2,  1}, {  6,  1}, { 10,  1}, 
        {  3,  1}, {  8,  5}, {  9,  1}, { 15,  5}, {  8,  1}, { 11,  1}, { 15,  1}, { 22,  1}, 
        {  0,  0}, {  0,  0}, {  1,  0}, {  2,  0}, {  0,  0}, {  0,  0}, {  2,  4}, {  5,  0}, 
        {  2,  3}, {  5,  3}, {  7,  0}, { 10,  0}, {  2,  3}, {  5,  3}, { 13,  0}, { 26,  0}, 
        {  1,  1}, {  1,  1}, {  4,  1}, {  7,  1}, {  2,  1}, {  2,  1}, {  6,  1}, { 10,  1}, 
        {  8,  3}, { 12,  3}, { 16,  5}, { 23,  3}, { 14,  1}, { 27,  1}, { 25,  3}, { 28,  3}, 
        {  1,  2}, {  1,  2}, {  3,  0}, {  8,  0}, {  1,  2}, {  1,  2}, {  8,  4}, { 12,  0}, 
        {  4,  3}, {  7,  6}, {  9,  0}, { 15,  0}, {  4,  3}, {  7,  6}, { 16,  0}, { 23,  4}, 
        {  4,  2}, {  4,  2}, {  9,  2}, { 16,  4}, {  7,  2}, {  7,  2}, { 15,  4}, { 23,  0}, 
        {  9,  3}, { 16,  3}, { 17,  0}, { 18,  0}, { 16,  7}, { 24,  1}, { 18,  1}, { 19,  1}, 
        {  2,  6}, {  2,  6}, {  8,  6}, { 11,  0}, {  2,  6}, {  2,  6}, { 14,  4}, { 27,  6}, 
        {  6,  3}, { 10,  6}, { 15,  6}, { 22,  0}, {  6,  3}, { 10,  6}, { 25,  4}, { 28,  4}, 
        {  7,  7}, {  7,  7}, { 16,  2}, { 24,  0}, { 13,  2}, { 13,  2}, { 25,  6}, { 29,  0}, 
        { 15,  3}, { 23,  5}, { 18,  3}, { 19,  0}, { 25,  1}, { 29,  5}, { 21,  1}, { 20,  0}, 
        {  0,  0}, {  0,  0}, {  1,  0}, {  2,  0}, {  0,  0}, {  0,  0}, {  2,  4}, {  5,  0}, 
        {  1,  3}, {  2,  5}, {  4,  0}, {  6,  0}, {  1,  3}, {  2,  5}, {  7,  5}, { 10,  5}, 
        {  2,  7}, {  2,  7}, {  7,  4}, { 13,  1}, {  5,  1}, {  5,  1}, { 10,  4}, { 26,  1}, 
        {  8,  7}, { 14,  5}, { 16,  1}, { 25,  7}, { 12,  1}, { 27,  5}, { 23,  7}, { 28,  7}, 
        {  0,  0}, {  0,  0}, {  1,  0}, {  2,  0}, {  0,  0}, {  0,  0}, {  2,  4}, {  5,  0}, 
        {  2,  3}, {  5,  3}, {  7,  0}, { 10,  0}, {  2,  3}, {  5,  3}, { 13,  0}, { 26,  0}, 
        {  2,  7}, {  2,  7}, {  7,  4}, { 13,  1}, {  5,  1}, {  5,  1}, { 10,  4}, { 26,  1}, 
        { 11,  3}, { 27,  7}, { 24,  3}, { 29,  3}, { 27,  3}, { 30,  1}, { 29,  7}, { 31,  3}, 
        {  2,  2}, {  2,  2}, {  8,  2}, { 14,  0}, {  2,  2}, {  2,  2}, { 11,  2}, { 27,  2}, 
        {  7,  3}, { 13,  3}, { 16,  6}, { 25,  2}, {  7,  3}, { 13,  3}, { 24,  2}, { 29,  4}, 
        {  6,  2}, {  6,  2}, { 15,  2}, { 25,  0}, { 10,  2}, { 10,  2}, { 22,  2}, { 28,  0}, 
        { 15,  7}, { 25,  5}, { 18,  2}, { 21,  0}, { 23,  1}, { 29,  1}, { 19,  2}, { 20,  1}, 
        {  5,  2}, {  5,  2}, { 12,  2}, { 27,  0}, {  5,  2}, {  5,  2}, { 27,  4}, { 30,  0}, 
        { 10,  3}, { 26,  3}, { 23,  2}, { 28,  2}, { 10,  3}, { 26,  3}, { 29,  2}, { 31,  2}, 
        { 10,  7}, { 10,  7}, { 23,  6}, { 29,  6}, { 26,  2}, { 26,  2}, { 28,  6}, { 31,  0}, 
        { 22,  3}, { 28,  5}, { 19,  3}, { 20,  3}, { 28,  1}, { 31,  1}, { 20,  2}, { 32,  0}
    };

    public GameObject[] RiverTilePrefabs = new GameObject[33];


    [Header("跟随的摄像机")]
    public GameObject FollowCamera;

    // Tile字典，键为Tile坐标，值为Tile对象
    private Dictionary<Vector2Int, GameObject> CurrTiles = new Dictionary<Vector2Int, GameObject>();



    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        GenerateMap();
    }
    
    void GenerateMap()
    {
        float Scale = 0.3f;

        Vector3 Center = FollowCamera.transform.position / 8.0f;

        // 2x3 grid
        int minY = Mathf.FloorToInt(Center.y);
        int maxY = Mathf.CeilToInt(Center.y);
        int centerX = Center.x - Mathf.Floor(Center.x) < 0.5f ? Mathf.FloorToInt(Center.x) : Mathf.CeilToInt(Center.x);

        for (int x = centerX - 1; x <= centerX + 1; x++) {
            for (int y = minY; y <= maxY; y++) {

                if (CurrTiles.ContainsKey(new Vector2Int(x, y))) continue;

                PlaceRiverTile(x, y, Scale);
                CurrTiles[new Vector2Int(x, y)] = GameObject.Find($"TileRiver_{x}_{y}");
            }
        }

        foreach (var kvp in CurrTiles) {
            Vector2Int tilePos = kvp.Key;
            GameObject tileObj = kvp.Value;

            if (Math.Abs(tilePos.x - centerX) > 2 || tilePos.y - maxY > 1 || minY - tilePos.y > 1) {
                Destroy(tileObj);
                CurrTiles.Remove(tilePos);
            }
        }
    }

    public static float Hash21(Vector2 p)
    {
        Vector2 rotated = new Vector2(
            p.x * 0.87758256f - p.y * 0.47942554f,
            p.x * 0.47942554f + p.y * 0.87758256f
        );
        
        uint xb = BitConverter.ToUInt32(BitConverter.GetBytes(rotated.x), 0);
        uint yb = BitConverter.ToUInt32(BitConverter.GetBytes(rotated.y), 0);
        
        uint x = xb * 747796414u + 2891336453u;
        uint y = yb * 2654435769u + 3572223783u;
        
        uint s = x ^ y;
        s ^= s >> 16;
        s *= 0x7feb352du;
        s ^= s >> 15;
        s *= 0x846ca68bu;
        s ^= s >> 16;

        return s / 4294967295.0f; // 0xFFFFFFFFu
    }

    public static float HashContinuous21(Vector2 p)
    {
        Vector2 u = new Vector2(p.x - Mathf.Floor(p.x), p.y - Mathf.Floor(p.y));

        u = u * u * (new Vector2(3.0f, 3.0f) - 2.0f * u);

        Vector2 q = new Vector2(Mathf.Floor(p.x), Mathf.Floor(p.y));

        float p00 = Hash21(q);
        float p01 = Hash21(new Vector2(q.x, q.y + 1.0f));
        float p10 = Hash21(new Vector2(q.x + 1.0f, q.y));
        float p11 = Hash21(new Vector2(q.x + 1.0f, q.y + 1.0f));

        return Mathf.Lerp(Mathf.Lerp(p00, p01, u.y), Mathf.Lerp(p10, p11, u.y), u.x);
    }

    public static float FBM_Perlin(Vector2 p)
    {
        float rate = 2.0f;
        float cur = 1.0f;
        float res = 0.0f;
        float totalWeight = 0.0f;

        for (int i = 0; i < 5; i++)
        {
            res += cur * HashContinuous21(p);
            totalWeight += cur;
            cur /= rate;
            p *= rate;
        }

        return res / totalWeight;
    }

    public float GetRiver(Vector2 uv)
    {
        float riverStrength = 0.0f;
        float amp = 1.0f;
        float freq = 1.0f;
        float weightSum = 0.0f;

        Vector2 warp = Vector2.zero;

        for (int i = 0; i < 7; i++)
        {
            Vector2 coord = uv * freq + warp * 1.5f;

            float n = FBM_Perlin(coord) * 2.0f - 1.0f;

            // warp += new Vector2(n, -n) * 0.5f;

            float v = 1.0f - Mathf.Abs(n);
            v = Mathf.Pow(v, 6.0f);

            riverStrength += v * amp;

            weightSum += amp;
            amp *= 0.5f;
            freq *= 2.0f;

            // uv = new Vector2(uv.x * 0.8f - uv.y * 0.6f, uv.x * 0.6f + uv.y * 0.8f);
        }

        return Mathf.SmoothStep(0.3f, 0.7f, riverStrength / weightSum);
    }


    /// <summary>
    /// 根据位置和缩放放置河流瓦片
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="scale"></param>
    public void PlaceRiverTile(int x, int y, float scale)
    {
        Vector2 scaledPos = new Vector2(x, y) * scale;

        if (GetRiver(scaledPos) < 0.5f) {
            
            // GameObject tileBase = Instantiate(RiverTilePrefabs[32], new Vector3(x, y, 0) * 8.0f, Quaternion.identity);
            // tileBase.transform.SetParent(transform);
            // tileBase.name = $"TileRiver_{x}_{y}";

            return;
        }

        Vector3 pos = new Vector3(x, y, 0);

        int tileBit = 0;
        Vector2 Xx = new Vector2(scale, 0);
        Vector2 Yy = new Vector2(0, scale);
        if (GetRiver(scaledPos - Xx + Yy) > 0.5f) tileBit += 1;
        if (GetRiver(scaledPos      + Yy) > 0.5f) tileBit += 2;
        if (GetRiver(scaledPos + Xx + Yy) > 0.5f) tileBit += 4;
        if (GetRiver(scaledPos - Xx     ) > 0.5f) tileBit += 8;
        if (GetRiver(scaledPos + Xx     ) > 0.5f) tileBit += 16;
        if (GetRiver(scaledPos - Xx - Yy) > 0.5f) tileBit += 32;
        if (GetRiver(scaledPos      - Yy) > 0.5f) tileBit += 64;
        if (GetRiver(scaledPos + Xx - Yy) > 0.5f) tileBit += 128;

        int tileID = MappingTable[tileBit, 0];
        int transType = MappingTable[tileBit, 1];

        GameObject tile = Instantiate(RiverTilePrefabs[tileID], pos * 8.0f, Quaternion.identity);
        tile.transform.SetParent(transform);
        tile.name = $"TileRiver_{x}_{y}";

        // 应用变换
        bool isFlipped = transType >= 4;
        float rotationAngle = transType % 4 * 90f;

        // 设置缩放实现镜像 (注意：这会影响所有子物体)
        tile.transform.localScale = new Vector3(isFlipped ? -1f : 1f, 1f, 1f);

        // 设置旋转
        tile.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle * (isFlipped ? 1f : -1f));
        
        
        if (tileID != 32) {
            GameObject water = Instantiate(RiverTilePrefabs[32], new Vector3(x, y, 0) * 8.0f, Quaternion.identity);
            water.transform.SetParent(transform);
            tile.name = $"TileRiverChild_{x}_{y}";
            water.name = $"TileRiver_{x}_{y}";

            tile.transform.SetParent(water.transform);
            tile.transform.localPosition = Vector3.zero;

            tile.transform.GetComponent<SpriteRenderer>().sortingOrder = 1;
            water.transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }
    }

}

