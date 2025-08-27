using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOutlineCollider : MonoBehaviour
{
    // 상수
    private const int DOWNSCALE_AMOUNT = 4; // 스프라이트를 다운스케일링 하는 정도(2~4 정도가 적당)(처리 속도 개선)
    private const float alphaThreshold = 0.1f; // 투명한지 아닌지 구분할 아트 임계값

    // private 필드(인스펙터 노출)
    [SerializeField] private float simplifyTolerancePixels = 1.2f; // 값이 0에 가까울수록 콜라이더가 정교해지는 값(즉 0에 가까울수록 메모리 사용량이 늘어남)(0.8 ~ 1.2가 적정값)
    [Range(0f, 0.1f)] [SerializeField] private float shrinkAmount = 0.02f; // 콜라이더 크기를 줄일 양

    // private 필드
    private List<List<Vector2>> contours = new List<List<Vector2>>();

    // 핵심 메서드
    /// <summary>
    /// 스프라이트를 기반으로 PolygonCollider2D 생성
    /// - Downscale, 투명도 기준, Colinear 제거, RDP 단순화 적용
    /// </summary>
    public void BuildCollider()
    {
        contours.Clear();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;
        PolygonCollider2D poly = GetComponent<PolygonCollider2D>();

        Texture2D tex = sprite.texture;
        if (!tex.isReadable) { Debug.LogError("텍스처의 Read/Write를 Enabled로 변경해야합니다."); return; }

        Rect texRect = sprite.textureRect;
        int w = (int)texRect.width;
        int h = (int)texRect.height;
        if (w == 0 || h == 0) { Debug.LogError("sprite.textureRect가 비어있습니다."); return; }

        Color32[] allPixels = tex.GetPixels32();

        Color32[] sprPixels = new Color32[w * h];
        int texW = tex.width;
        int startX = (int)texRect.x;
        int startY = (int)texRect.y;
        for (int y = 0; y < h; y++)
        {
            int src = (startY + y) * texW + startX;
            Array.Copy(allPixels, src, sprPixels, y * w, w);
        }

        // 다운스케일링
        int w2 = Mathf.Max(1, w / DOWNSCALE_AMOUNT);
        int h2 = Mathf.Max(1, h / DOWNSCALE_AMOUNT);
        bool[,] mask = new bool[w2, h2];

        for (int y = 0; y < h2; y++)
            for (int x = 0; x < w2; x++)
            {
                int srcX = x * DOWNSCALE_AMOUNT;
                int srcY = y * DOWNSCALE_AMOUNT;
                mask[x, y] = (sprPixels[srcY * w + srcX].a / 255f) > alphaThreshold;
            }

        var adjacency = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        void AddEdge(Vector2Int a, Vector2Int b)
        {
            if (!adjacency.TryGetValue(a, out var sa)) { sa = new HashSet<Vector2Int>(); adjacency[a] = sa; }
            if (!adjacency.TryGetValue(b, out var sb)) { sb = new HashSet<Vector2Int>(); adjacency[b] = sb; }
            sa.Add(b); sb.Add(a);
        }

        // edge 생성도 downscale mask 기준
        for (int y = 0; y < h2; y++)
        {
            for (int x = 0; x < w2; x++)
            {
                if (!mask[x, y]) continue;

                if (x - 1 < 0 || !mask[x - 1, y])
                    AddEdge(new Vector2Int(x, y), new Vector2Int(x, y + 1));

                if (x + 1 >= w2 || !mask[x + 1, y])
                    AddEdge(new Vector2Int(x + 1, y + 1), new Vector2Int(x + 1, y));

                if (y - 1 < 0 || !mask[x, y - 1])
                    AddEdge(new Vector2Int(x + 1, y), new Vector2Int(x, y));

                if (y + 1 >= h2 || !mask[x, y + 1])
                    AddEdge(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
            }
        }

        if (adjacency.Count == 0)
        {
            poly.pathCount = 0;
            Debug.LogWarning("경계가 없는 완전 투명한 스프라이트입니다.");
            return;
        }

        // 스프라이트의 모든 픽셀을 돌아볼 루프 생성
        var used = new HashSet<(Vector2Int, Vector2Int)>();
        foreach (var kv in adjacency)
        {
            Vector2Int start = kv.Key;
            foreach (var neighbor in kv.Value)
            {
                var edgeKey = (start, neighbor);
                var edgeKeyRev = (neighbor, start);
                if (used.Contains(edgeKey) || used.Contains(edgeKeyRev)) continue;

                var loop = new List<Vector2Int>();
                Vector2Int cur = start;
                Vector2Int next = neighbor;

                used.Add((cur, next));
                used.Add((next, cur));
                loop.Add(cur);
                loop.Add(next);

                while (true)
                {
                    if (!adjacency.TryGetValue(next, out var neighs)) break;
                    Vector2Int candidate = new Vector2Int(int.MinValue, int.MinValue);
                    bool found = false;
                    foreach (var n2 in neighs)
                    {
                        if (n2 == cur) continue;
                        if (used.Contains((next, n2)) || used.Contains((n2, next))) continue;
                        candidate = n2;
                        found = true;
                        break;
                    }
                    if (!found)
                    {
                        if (next != loop[0] && adjacency[next].Contains(loop[0]) && !used.Contains((next, loop[0])))
                        {
                            candidate = loop[0];
                            found = true;
                        }
                    }
                    if (!found) break;

                    cur = next;
                    next = candidate;
                    used.Add((cur, next));
                    used.Add((next, cur));
                    if (next == loop[0]) break;
                    loop.Add(next);

                    if (loop.Count > (w2 + h2) * 16)
                    {
                        Debug.LogWarning("비정상적으로 큰 루프가 감지되었습니다.");
                        break;
                    }
                }

                if (loop.Count >= 3)
                {
                    if (loop[loop.Count - 1] == loop[0])
                        loop.RemoveAt(loop.Count - 1);

                    var floatLoop = new List<Vector2>();
                    foreach (var v in loop) floatLoop.Add(new Vector2(v.x, v.y));

                    floatLoop = RemoveColinear(floatLoop);

                    if (floatLoop.Count >= 3)
                        contours.Add(floatLoop);
                }
            }
        }

        // Downscale 보정 및 PolygonCollider2D 설정
        float ppu = sprite.pixelsPerUnit;
        Vector2 pivot = sprite.pivot;
        for (int i = 0; i < contours.Count; i++)
        {
            var c = contours[i];
            for (int j = 0; j < c.Count; j++)
            {
                // downscale 보정
                Vector2 p = c[j] * DOWNSCALE_AMOUNT;

                if (sr.flipX) p.x = w - p.x;
                if (sr.flipY) p.y = h - p.y;

                Vector2 local = (p - pivot) / ppu;
                c[j] = local;
            }

            if (shrinkAmount > 0.001f)
                c = ShrinkPolygon(c, shrinkAmount);

            if (simplifyTolerancePixels > 0.001f)
            {
                float tolUnits = simplifyTolerancePixels / ppu;
                c = RamerDouglasPeucker(c, tolUnits);
            }

            contours[i] = c;
        }

        poly.pathCount = contours.Count;
        for (int i = 0; i < contours.Count; i++)
        {
            poly.SetPath(i, contours[i].ToArray());
        }
    }
    /// <summary> 
    /// 직선상에 있는 점 제거
    /// - 처리 속도 향상을 위함
    /// </summary>
    private static List<Vector2> RemoveColinear(List<Vector2> pts)
    {
        if (pts.Count < 3) return new List<Vector2>(pts);
        var outp = new List<Vector2>();
        for (int i = 0; i < pts.Count; i++)
        {
            Vector2 a = pts[(i - 1 + pts.Count) % pts.Count];
            Vector2 b = pts[i];
            Vector2 c = pts[(i + 1) % pts.Count];
            Vector2 ab = (b - a).normalized;
            Vector2 bc = (c - b).normalized;
            if (Vector2.Distance(ab, bc) < 1e-4f) continue;
            outp.Add(b);
        }
        return outp;
    }
    /// <summary>
    /// Ramer-Douglas-Peucker 알고리즘 적용
    /// - 폴리곤 점들을 단순화하여 불필요한 점 제거
    /// - epsilon : 허용 오차
    /// </summary>
    private static List<Vector2> RamerDouglasPeucker(List<Vector2> points, float epsilon)
    {
        if (points == null || points.Count < 3) return new List<Vector2>(points);
        bool[] keep = new bool[points.Count];
        keep[0] = keep[points.Count - 1] = true;

        void RDP(int a, int b)
        {
            float maxDist = 0f;
            int index = -1;
            Vector2 A = points[a];
            Vector2 B = points[b];
            for (int i = a + 1; i < b; i++)
            {
                float dist = DistancePointLine(points[i], A, B);
                if (dist > maxDist) { maxDist = dist; index = i; }
            }
            if (maxDist > epsilon)
            {
                keep[index] = true;
                RDP(a, index);
                RDP(index, b);
            }
        }

        RDP(0, points.Count - 1);
        var res = new List<Vector2>();
        for (int i = 0; i < points.Count; i++) if (keep[i]) res.Add(points[i]);
        return res;
    }
    /// <summary>
    /// 점 p와 선 AB 사이의 거리 계산
    /// </summary>
    private static float DistancePointLine(Vector2 p, Vector2 a, Vector2 b)
    {
        if (a == b) return Vector2.Distance(p, a);
        float t = Vector2.Dot(p - a, b - a) / Vector2.Dot(b - a, b - a);
        t = Mathf.Clamp01(t);
        Vector2 proj = a + t * (b - a);
        return Vector2.Distance(p, proj);
    }
    /// <summary>
    /// 폴리곤의 중심점 계산
    /// - ShrinkPolygon에서 기준점으로 사용
    /// - 콜라이더 크기를 줄일 때 사용
    /// </summary>
    private Vector2 GetPolygonCentroid(List<Vector2> polygon)
    {
        Vector2 sum = Vector2.zero;
        foreach (var p in polygon) sum += p;
        return sum / polygon.Count;
    }
    /// <summary>
    /// 폴리곤을 중심점을 기준으로 안쪽으로 이동시켜 크기 축소
    /// - amount : 축소 거리
    /// </summary>
    private List<Vector2> ShrinkPolygon(List<Vector2> polygon, float amount)
    {
        Vector2 centroid = GetPolygonCentroid(polygon);
        List<Vector2> shrunk = new List<Vector2>();
        foreach (var p in polygon)
        {
            Vector2 dir = (p - centroid).normalized;
            shrunk.Add(p - dir * amount);
        }
        return shrunk;
    }
}