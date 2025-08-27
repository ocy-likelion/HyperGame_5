using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObject : MonoBehaviour
{
    // 프리팹
    [Header("프리팹")]
    [SerializeField] private ParticleSystem particle;

    // private 필드
    private EffectObjectPool effectObjectPool;

    public void InitEffectObject(EffectObjectPool effectObjectPool) // 오브젝트 초기화
    {
        this.effectObjectPool = effectObjectPool;
        transform.SetParent(effectObjectPool.transform);
    }
    public void PlayEffect() // 이펙트 코루틴 재생
    {
        StartCoroutine(ParticleCoroutine());
    }
    private IEnumerator ParticleCoroutine() // 파티클 코루틴
    {
        particle.Play();
        yield return new WaitForSeconds(1.5f);
        effectObjectPool.Return(gameObject);
    }
}
