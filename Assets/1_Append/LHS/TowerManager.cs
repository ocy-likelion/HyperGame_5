using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    // 현재 씬에 존재하는 블록들을 등록/관리
    private readonly List<GameObject> blocks = new List<GameObject>();

    public void RegisterBlock(GameObject block)
    {
        if (block != null && !blocks.Contains(block))
            blocks.Add(block);
    }

    public void UnregisterBlock(GameObject block)
    {
        if (block != null) blocks.Remove(block);
    }

    public List<GameObject> GetAllBlocks()
    {
        // 외부에서 리스트 수정 못 하도록 복사본을 반환
        return new List<GameObject>(blocks);
    }

    public List<GameObject> GetTopBlocks(int count)
    {
        return blocks
            .Where(b => b != null)
            .OrderByDescending(b => b.transform.position.y)
            .Take(count)
            .ToList();
    }
}