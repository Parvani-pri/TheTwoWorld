using System.Collections;
using UnityEngine;

public class RuneRainAnimationEvent : MonoBehaviour
{[Header("Rune Rain FX")]
    [SerializeField] private GameObject runeRainFX;

    [Header("Play Duration")]
    [SerializeField] private float duration = 4f;

    private Coroutine currentCoroutine;

    public void AE_PlayRuneRain()
    {
        if (runeRainFX == null)
        {
            Debug.LogWarning($"{name}: RuneRainFX is not assigned.");
            return;
        }

        runeRainFX.SetActive(true);

        // 重新播放所有 Particle
        ParticleSystem[] particles = runeRainFX.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particles)
        {
            ps.Clear(true);
            ps.Play(true);
        }

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(DisableFX());
    }

    private IEnumerator DisableFX()
    {
        yield return new WaitForSeconds(duration);

        runeRainFX.SetActive(false);
    }
}
