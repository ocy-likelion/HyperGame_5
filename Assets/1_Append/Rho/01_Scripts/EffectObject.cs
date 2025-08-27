using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObject : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;
    private EffectObjectPool effectPool;
    
    public void InitEffectObject(EffectObjectPool _effectObjectPool)
    {
        effectPool = _effectObjectPool;
        this.transform.SetParent(_effectObjectPool.transform);
    }

    public void PlayEffect()
    {
        StartCoroutine(ParticleCoroutine());
    }

    IEnumerator ParticleCoroutine()
    {
        particle.Play();
        yield return new WaitForSeconds(1.5f);
        effectPool.Return(this.gameObject);
    }
}
