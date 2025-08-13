using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOutlineCollider : MonoBehaviour
{
    [Header("주요 프로퍼티")]
    float alphaThreshold = 0.1f;
    public float simplifyTolerancePixels = 1f; //값이 클수록 콜라이더의 꼭짓점이 단순해집니다.
    [Range(0f, 0.05f)] public float shrinkAmount = 0.02f; // 값이 클수록 콜라이더가 작아집니다.
    public bool drawGizmos = true; // 콜라이더 기즈모를 그릴지 여부입니다.

    [Header("부가 프로퍼티")]
    List<List<Vector2>> contours = new List<List<Vector2>>();

    void Start()
    {
        BuildCollider();
    }

    public void BuildCollider()
    {
        contours.Clear();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;
        PolygonCollider2D poly = GetComponent<PolygonCollider2D>();

        if (sprite == null) { Debug.LogError("[PPRC] SpriteRenderer.sprite 가 없습니다."); return; }
        Texture2D tex = sprite.texture;
        if (!tex.isReadable) { Debug.LogError("[PPRC] 텍스처가 Read/Write Enabled 가 아닙니다."); return; }

        Rect texRect = sprite.textureRect;
        int w = (int)texRect.width;
        int h = (int)texRect.height;
        if (w == 0 || h == 0) { Debug.LogError("[PPRC] sprite.textureRect가 비어있음."); return; }

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

        bool[,] mask = new bool[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                mask[x, y] = (sprPixels[y * w + x].a / 255f) > alphaThreshold;

        var adjacency = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        void AddEdge(Vector2Int a, Vector2Int b)
        {
            if (!adjacency.TryGetValue(a, out var sa)) { sa = new HashSet<Vector2Int>(); adjacency[a] = sa; }
            if (!adjacency.TryGetValue(b, out var sb)) { sb = new HashSet<Vector2Int>(); adjacency[b] = sb; }
            sa.Add(b); sb.Add(a);
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!mask[x, y]) continue;

                if (x - 1 < 0 || !mask[x - 1, y])
                    AddEdge(new Vector2Int(x, y), new Vector2Int(x, y + 1));

                if (x + 1 >= w || !mask[x + 1, y])
                    AddEdge(new Vector2Int(x + 1, y + 1), new Vector2Int(x + 1, y));

                if (y - 1 < 0 || !mask[x, y - 1])
                    AddEdge(new Vector2Int(x + 1, y), new Vector2Int(x, y));

                if (y + 1 >= h || !mask[x, y + 1])
                    AddEdge(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
            }
        }

        if (adjacency.Count == 0)
        {
            poly.pathCount = 0;
            Debug.LogWarning("[PPRC] 경계 없음(완전 투명?).");
            return;
        }

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

                    if (loop.Count > (w + h) * 16)
                    {
                        Debug.LogWarning("[PPRC] 루프가 비정상적으로 큼, 중지.");
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

        Debug.Log($"[PPRC] 컨투어 개수: {contours.Count}");

        float ppu = sprite.pixelsPerUnit;
        Vector2 pivot = sprite.pivot;
        for (int i = 0; i < contours.Count; i++)
        {
            var c = contours[i];
            for (int j = 0; j < c.Count; j++)
            {
                Vector2 p = c[j];
                if (sr.flipX) p.x = w - p.x;
                if (sr.flipY) p.y = h - p.y;

                Vector2 local = (p - pivot) / ppu;
                c[j] = local;
            }

            // 여기서 축소 적용
            if (shrinkAmount > 0.001f)
                c = ShrinkPolygon(c, shrinkAmount);

            // 단순화는 축소 후가 더 자연스럽지만 상황에 따라 변경 가능
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

        Debug.Log($"[PPRC] 콜라이더 경로 적용 완료. 총 pathCount={poly.pathCount}");
    }

    static List<Vector2> RemoveColinear(List<Vector2> pts)
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

    static List<Vector2> RamerDouglasPeucker(List<Vector2> points, float epsilon)
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
    static float DistancePointLine(Vector2 p, Vector2 a, Vector2 b)
    {
        if (a == b) return Vector2.Distance(p, a);
        float t = Vector2.Dot(p - a, b - a) / Vector2.Dot(b - a, b - a);
        t = Mathf.Clamp01(t);
        Vector2 proj = a + t * (b - a);
        return Vector2.Distance(p, proj);
    }

    Vector2 GetPolygonCentroid(List<Vector2> polygon)
    {
        Vector2 sum = Vector2.zero;
        foreach (var p in polygon) sum += p;
        return sum / polygon.Count;
    }

    List<Vector2> ShrinkPolygon(List<Vector2> polygon, float amount)
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

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || contours == null) return;
        Gizmos.color = Color.red;
        foreach (var c in contours)
        {
            for (int i = 0; i < c.Count; i++)
            {
                Vector3 a = transform.TransformPoint(c[i]);
                Vector3 b = transform.TransformPoint(c[(i + 1) % c.Count]);
                Gizmos.DrawLine(a, b);
            }
        }
    }
}
