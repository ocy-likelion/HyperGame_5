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

    // 메인
    /// <summary>
    /// 스프라이트를 기반으로 PolygonCollider2D 생성
    /// - Downscale, 투명도 기준, Colinear 제거, RDP 단순화 적용
    /// </summary>
    public void BuildCollider()
    {
        contours.Clear();

        // 필요 변수 가져오기
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;
        Texture2D tex = sprite.texture;
        PolygonCollider2D poly = GetComponent<PolygonCollider2D>();

        // 텍스쳐 체크
        if (!tex.isReadable) 
        {
            Debug.LogError("텍스처의 Read/Write를 Enabled로 변경해야합니다.");
            return;
        }

        // 스프라이트의 영역 체크
        Rect texRect = sprite.textureRect;
        int w = (int)texRect.width;
        int h = (int)texRect.height;
        if (w == 0 || h == 0)
        {
            Debug.LogError("sprite.textureRect가 비어있습니다.");
            return;
        }

        // 스프라이트 영역의 픽셀만 추출
        Color32[] allPixels = tex.GetPixels32(); // 텍스처의 모든 픽셀 읽기
        Color32[] sprPixels = new Color32[w * h]; // 스프라이트가 있는 영역의 픽셀 배열 생성
        int texW = tex.width;
        int startX = (int)texRect.x;
        int startY = (int)texRect.y;
        for (int y = 0; y < h; y++)
        {
            int src = (startY + y) * texW + startX;
            Array.Copy(allPixels, src, sprPixels, y * w, w);
        }

        // Downscale(최적화)
        int w_downScale = Mathf.Max(1, w / DOWNSCALE_AMOUNT);
        int h_downScale = Mathf.Max(1, h / DOWNSCALE_AMOUNT);

        // 알파값을 기준으로 스프라이트를 마스킹
        bool[,] mask = new bool[w_downScale, h_downScale];
        for (int y = 0; y < h_downScale; y++)
        {
            for (int x = 0; x < w_downScale; x++)
            {
                int srcX = x * DOWNSCALE_AMOUNT;
                int srcY = y * DOWNSCALE_AMOUNT;
                mask[x, y] = (sprPixels[srcY * w + srcX].a / 255f) > alphaThreshold;
            }
        }

        // 그래프 구조
        Dictionary<Vector2Int, HashSet<Vector2Int>> adjacency = new();
        void AddEdge(Vector2Int a, Vector2Int b)
        {
            if (!adjacency.TryGetValue(a, out HashSet<Vector2Int> sa))
            { 
                sa = new HashSet<Vector2Int>(); adjacency[a] = sa;
            }
            sa.Add(b);

            if (!adjacency.TryGetValue(b, out HashSet<Vector2Int> sb))
            { 
                sb = new HashSet<Vector2Int>(); adjacency[b] = sb;
            }
            sb.Add(a);
        }

        // Mask 기준으로 Edge 생성
        for (int y = 0; y < h_downScale; y++)
        {
            for (int x = 0; x < w_downScale; x++)
            {
                if (!mask[x, y]) continue;

                if (x - 1 < 0 || !mask[x - 1, y])
                {
                    AddEdge(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }
                if (x + 1 >= w_downScale || !mask[x + 1, y])
                {
                    AddEdge(new Vector2Int(x + 1, y + 1), new Vector2Int(x + 1, y));
                }
                if (y - 1 < 0 || !mask[x, y - 1])
                {
                    AddEdge(new Vector2Int(x + 1, y), new Vector2Int(x, y));
                }
                if (y + 1 >= h_downScale || !mask[x, y + 1])
                {
                    AddEdge(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
                }
            }
        }
        if (adjacency.Count == 0) // 예외처리
        {
            poly.pathCount = 0;
            Debug.LogWarning("경계가 없는 완전 투명한 스프라이트입니다.");
            return;
        }

        // 스프라이트의 모든 픽셀을 돌아볼 루프 생성(Polygon Collider에 적용하기 위한)
        HashSet<(Vector2Int, Vector2Int)> used = new();

        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> kv in adjacency) // Edge 순회
        {
            Vector2Int start = kv.Key;
            foreach (Vector2Int neighbor in kv.Value)
            {
                (Vector2Int start, Vector2Int neighbor) edgeKey = (start, neighbor);
                (Vector2Int neighbor, Vector2Int start) edgeKeyRev = (neighbor, start);

                if (used.Contains(edgeKey) || used.Contains(edgeKeyRev)) continue;

                List<Vector2Int> loop = new List<Vector2Int>();
                Vector2Int cur = start;
                Vector2Int next = neighbor;

                used.Add((cur, next));
                used.Add((next, cur));
                loop.Add(cur);
                loop.Add(next);

                while (true) // 다음 점 찾기
                {
                    if (!adjacency.TryGetValue(next, out HashSet<Vector2Int> neighs)) break;

                    Vector2Int candidate = new Vector2Int(int.MinValue, int.MinValue);
                    bool found = false;

                    foreach (Vector2Int n2 in neighs)
                    {
                        if (n2 == cur) continue;
                        if (used.Contains((next, n2)) || used.Contains((n2, next))) continue;

                        candidate = n2;
                        found = true;

                        break;
                    }

                    if (!found) // 루프 닫기 체크
                    {
                        if (next != loop[0] && adjacency[next].Contains(loop[0]) && !used.Contains((next, loop[0])))
                        {
                            candidate = loop[0];
                            found = true;
                        }
                    }

                    // 루프 종료
                    if (!found) break; 
                    cur = next;
                    next = candidate;
                    used.Add((cur, next));
                    used.Add((next, cur));
                    if (next == loop[0]) break;
                    loop.Add(next);

                    // 안전장치
                    if (loop.Count > (w_downScale + h_downScale) * 16) 
                    {
                        Debug.LogWarning("비정상적으로 큰 루프가 감지되었습니다.");
                        break;
                    }
                }

                if (loop.Count >= 3) // 점이 3개는 넘어가야 콜라이더를 만들 수 있다.
                {
                    if (loop[loop.Count - 1] == loop[0]) // 중복 점 제거(시작점과 끝점)
                    {
                        loop.RemoveAt(loop.Count - 1);
                    }

                    List<Vector2> floatLoop = new List<Vector2>();
                    foreach (Vector2Int v in loop) // 실수 좌표료 변환(Polygon Collider에 등록하기 위함)
                    {
                        floatLoop.Add(new Vector2(v.x, v.y));
                    }

                    floatLoop = RemoveColinear(floatLoop); // 직선 상에 있는 불필요한 점 제거

                    if (floatLoop.Count >= 3) // 최종 점 개수 확인
                    {
                        contours.Add(floatLoop);
                    }
                }
            }
        }

        // Downscale 보정 및 PolygonCollider2D 설정
        Vector2 pivot = sprite.pivot; // collider 맞출 때 중심 기준 좌표

        for (int i = 0; i < contours.Count; i++)
        {
            List<Vector2> c = contours[i];

            for (int j = 0; j < c.Count; j++) 
            {
                Vector2 p = c[j] * DOWNSCALE_AMOUNT; // Downscale된 것을 원래 크기로 다시 늘리기

                // flip 체크
                if (sr.flipX)
                {
                    p.x = w - p.x;
                }
                if (sr.flipY)
                {
                    p.y = h - p.y;
                }

                Vector2 local = (p - pivot) / sprite.pixelsPerUnit; // 중심점을 스프라이트의 중심점으로 옮기기(원래는 0, 0으로 되어있음)
                c[j] = local;
            }

            if (shrinkAmount > 0.001f) // 축소값이 있다면 축소 적용
            {
                c = ShrinkPolygon(c, shrinkAmount);
            }

            if (simplifyTolerancePixels > 0.001f) // RDP 알고리즘(다각형 근사화)
            {
                float tolUnits = simplifyTolerancePixels / sprite.pixelsPerUnit;
                c = RamerDouglasPeucker(c, tolUnits);
            }

            contours[i] = c;
        }

        // Polygon Collider 2D에 경로 적용
        poly.pathCount = contours.Count;
        for (int i = 0; i < contours.Count; i++)
        {
            poly.SetPath(i, contours[i].ToArray());
        }
    }
    /// <summary> 
    /// 직선상에 있는 점 제거
    /// - 처리 속도 향상, 최적화(점이 적을수록 물리 연산이 쉬워진다)
    /// </summary>
    private static List<Vector2> RemoveColinear(List<Vector2> pts)
    {
        if (pts.Count < 3) // 점 3개 미만이면 그냥 반환
        {
            return new List<Vector2>(pts);
        }

        List<Vector2> outp = new();
        for (int i = 0; i < pts.Count; i++)
        {
            // 연속된 세 점 찾기
            Vector2 a = pts[(i - 1 + pts.Count) % pts.Count];
            Vector2 b = pts[i];
            Vector2 c = pts[(i + 1) % pts.Count];

            // 방향을 비교
            Vector2 ab = (b - a).normalized;
            Vector2 bc = (c - b).normalized;

            // 방향이 거의 같으면 점 b를 제거
            if (Vector2.Distance(ab, bc) < 0.001f) continue;

            outp.Add(b);
        }

        return outp;
    }
    /// <summary>
    /// Ramer-Douglas-Peucker 알고리즘 적용
    /// - 폴리곤 점들을 단순화하여 불필요한 점 제거
    /// - Collider 계산량 감소
    /// - epsilon : 허용 오차
    /// </summary>
    private static List<Vector2> RamerDouglasPeucker(List<Vector2> points, float epsilon)
    {
        if (points == null || points.Count < 3) // 3개 미만이면 리턴
        {
            return new List<Vector2>(points);
        }

        bool[] keep = new bool[points.Count];
        keep[0] = keep[points.Count - 1] = true;

        void RDP(int a, int b) // 재귀용
        {
            float maxDist = 0f;
            int index = -1;
            Vector2 A = points[a];
            Vector2 B = points[b];
            for (int i = a + 1; i < b; i++)
            {
                float dist = DistancePointLine(points[i], A, B);

                if (dist > maxDist)
                {
                    maxDist = dist; index = i;
                }
            }

            if (maxDist > epsilon)
            {
                keep[index] = true;
                RDP(a, index);
                RDP(index, b);
            }
        }

        RDP(0, points.Count - 1);

        List<Vector2> res = new();
        for (int i = 0; i < points.Count; i++)
        {
            if (keep[i])
            {
                res.Add(points[i]);
            }
        }

        return res;
    }
    /// <summary>
    /// 점 p와 선 AB 사이의 거리 계산
    /// </summary>
    private static float DistancePointLine(Vector2 p, Vector2 a, Vector2 b)
    {
        if (a == b)
        {
            return Vector2.Distance(p, a);
        }

        float t = Vector2.Dot(p - a, b - a) / Vector2.Dot(b - a, b - a);
        t = Mathf.Clamp01(t);
        Vector2 proj = a + t * (b - a);

        return Vector2.Distance(p, proj);
    }
    /// <summary>
    /// 폴리곤을 중심점을 기준으로 안쪽으로 이동시켜 크기 축소
    /// - amount : 축소할 양
    /// </summary>
    private List<Vector2> ShrinkPolygon(List<Vector2> polygon, float amount)
    {
        Vector2 centroid = GetPolygonCentroid(polygon);
        List<Vector2> shrunk = new();

        foreach (Vector2 p in polygon)
        {
            Vector2 dir = (p - centroid).normalized;
            shrunk.Add(p - dir * amount);
        }

        return shrunk;
    }
    /// <summary>
    /// 폴리곤의 중심점 계산
    /// - ShrinkPolygon에서 기준점으로 사용
    /// - 콜라이더 크기를 줄일 때 사용
    /// </summary>
    private Vector2 GetPolygonCentroid(List<Vector2> polygon)
    {
        Vector2 sum = Vector2.zero;

        foreach (Vector2 p in polygon)
        {
            sum += p;
        }

        return sum / polygon.Count;
    }
}