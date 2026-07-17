using System.Collections;
using UnityEngine;

public class RuneRainAnimationEvent : MonoBehaviour
{[Header("Rune Rain FX")]
    [SerializeField] private GameObject runeRainFX;
    [SerializeField] Transform originalParent;

    ParticleSystem[] particles;

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
        particles = runeRainFX.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particles)
        {
            ps.Clear(true);
            ps.Play(true);
            ps.gameObject.transform.parent = null;
            ps.transform.localScale = new Vector3(Mathf.Abs(ps.transform.localScale.x), Mathf.Abs(ps.transform.localScale.y), Mathf.Abs(ps.transform.localScale.z));
        }

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(DisableFX());
    }

    private IEnumerator DisableFX()
    {
        yield return new WaitForSeconds(duration);

        runeRainFX.SetActive(false);
        foreach (ParticleSystem ps in particles)
        {
            ps.gameObject.transform.parent = originalParent;
            ps.transform.localScale = new Vector3(Mathf.Abs(ps.transform.localScale.x), Mathf.Abs(ps.transform.localScale.y), Mathf.Abs(ps.transform.localScale.z));
        }
    }
}
