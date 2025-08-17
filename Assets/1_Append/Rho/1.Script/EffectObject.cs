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
        if (particle.isPlaying)
        {
            particle.Stop();
        }

        particle.Play();
        StartCoroutine(ParticleCoroutine());
    }

    IEnumerator ParticleCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        effectPool.Return(this.gameObject);
    }
}
