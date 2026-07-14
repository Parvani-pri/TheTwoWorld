using System.Collections;
using UnityEngine;

public class WaterAttackFX : MonoBehaviour
{
    [Header("Wave")]
    [SerializeField] private Transform waveSlashA;
    [SerializeField] private Transform waveSlashB;

    [Header("Fish")]
    [SerializeField] private Transform fish01;
    [SerializeField] private Transform fish02;
    [SerializeField] private Transform fish03;

    [Header("Splash")]
    [SerializeField] private GameObject splashBurst;

    [Header("Particle")]
    [SerializeField] private ParticleSystem foamTrail;

    [Header("Movement")]
    [SerializeField] private bool moveLeft = true;
    [SerializeField] private float moveDistance = 2.8f;
    [SerializeField] private float totalDuration = 1.15f;

    [Tooltip("海浪先原地生成幾耐，之後先開始向前衝。")]
    [SerializeField] private float chargeDuration = 0.2f;

    [Tooltip("海浪向前移動幾耐。")]
    [SerializeField] private float moveDuration = 0.7f;

    [Header("Wave Scale")]
    [SerializeField] private float startScale = 0.15f;
    [SerializeField] private float chargeScale = 0.45f;
    [SerializeField] private float maxScale = 1f;

    [Header("Wave Motion")]
    [SerializeField] private float waveVerticalAmount = 0.08f;
    [SerializeField] private float waveRotationAmount = 4f;

    [Header("Fish Jump")]
    [SerializeField] private float fishJumpHeight = 0.65f;
    [SerializeField] private float fishJumpDuration = 0.45f;

    [SerializeField] private float fish01Delay = 0.25f;
    [SerializeField] private float fish02Delay = 0.4f;
    [SerializeField] private float fish03Delay = 0.55f;

    [SerializeField] private float fish01HorizontalOffset = -0.15f;
    [SerializeField] private float fish02HorizontalOffset = 0.1f;
    [SerializeField] private float fish03HorizontalOffset = 0.3f;

    [SerializeField] private float fishSpinAmount = 35f;

    [Header("Fade")]
    [Tooltip("由總時間邊個比例開始淡出。0.7 代表最後 30% 淡出。")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeStartNormalizedTime = 0.72f;

    [Header("Splash Timing")]
    [SerializeField] private float splashTime = 0.78f;
    [SerializeField] private float splashVisibleDuration = 0.3f;

    [Header("Destroy")]
    [SerializeField] private float extraDestroyDelay = 0.1f;

    private Vector3 rootStartPosition;

    private Vector3 waveAStartLocalPosition;
    private Vector3 waveBStartLocalPosition;

    private Vector3 waveAStartLocalScale;
    private Vector3 waveBStartLocalScale;

    private Quaternion waveAStartLocalRotation;
    private Quaternion waveBStartLocalRotation;

    private Vector3 fish01StartLocalPosition;
    private Vector3 fish02StartLocalPosition;
    private Vector3 fish03StartLocalPosition;

    private Quaternion fish01StartLocalRotation;
    private Quaternion fish02StartLocalRotation;
    private Quaternion fish03StartLocalRotation;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    private Coroutine playCoroutine;

    private void Awake()
    {
        CacheInitialValues();
        CacheSpriteRenderers();
    }

    private void OnEnable()
    {
        ResetEffect();

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        playCoroutine = StartCoroutine(PlayEffect());
    }

    private void OnDisable()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
    }

    private void CacheInitialValues()
    {
        rootStartPosition = transform.position;

        if (waveSlashA != null)
        {
            waveAStartLocalPosition = waveSlashA.localPosition;
            waveAStartLocalScale = waveSlashA.localScale;
            waveAStartLocalRotation = waveSlashA.localRotation;
        }

        if (waveSlashB != null)
        {
            waveBStartLocalPosition = waveSlashB.localPosition;
            waveBStartLocalScale = waveSlashB.localScale;
            waveBStartLocalRotation = waveSlashB.localRotation;
        }

        CacheFishInitialValue(
            fish01,
            out fish01StartLocalPosition,
            out fish01StartLocalRotation
        );

        CacheFishInitialValue(
            fish02,
            out fish02StartLocalPosition,
            out fish02StartLocalRotation
        );

        CacheFishInitialValue(
            fish03,
            out fish03StartLocalPosition,
            out fish03StartLocalRotation
        );
    }

    private void CacheSpriteRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    private void ResetEffect()
    {
        rootStartPosition = transform.position;

        ResetWave(
            waveSlashA,
            waveAStartLocalPosition,
            waveAStartLocalRotation
        );

        ResetWave(
            waveSlashB,
            waveBStartLocalPosition,
            waveBStartLocalRotation
        );

        ResetFish(
            fish01,
            fish01StartLocalPosition,
            fish01StartLocalRotation
        );

        ResetFish(
            fish02,
            fish02StartLocalPosition,
            fish02StartLocalRotation
        );

        ResetFish(
            fish03,
            fish03StartLocalPosition,
            fish03StartLocalRotation
        );

        if (splashBurst != null)
        {
            splashBurst.SetActive(false);
        }

        if (foamTrail != null)
        {
            foamTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            foamTrail.Play();
        }

        SetAllSpriteAlpha(1f);
    }

    private IEnumerator PlayEffect()
    {
        StartCoroutine(
            PlayFishJump(
                fish01,
                fish01StartLocalPosition,
                fish01StartLocalRotation,
                fish01Delay,
                fish01HorizontalOffset
            )
        );

        StartCoroutine(
            PlayFishJump(
                fish02,
                fish02StartLocalPosition,
                fish02StartLocalRotation,
                fish02Delay,
                fish02HorizontalOffset
            )
        );

        StartCoroutine(
            PlayFishJump(
                fish03,
                fish03StartLocalPosition,
                fish03StartLocalRotation,
                fish03Delay,
                fish03HorizontalOffset
            )
        );

        StartCoroutine(PlaySplash());

        float elapsed = 0f;
        float direction = moveLeft ? -1f : 1f;

        Vector3 endPosition =
            rootStartPosition +
            Vector3.right * moveDistance * direction;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            float totalT = Mathf.Clamp01(elapsed / totalDuration);

            UpdateRootMovement(elapsed, endPosition);
            UpdateWaveScale(elapsed);
            UpdateWaveMotion(totalT);
            UpdateFade(totalT);

            yield return null;
        }

        SetAllSpriteAlpha(0f);

        if (foamTrail != null)
        {
            foamTrail.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        yield return new WaitForSeconds(extraDestroyDelay);

        Destroy(gameObject);
    }

    private void UpdateRootMovement(
        float elapsed,
        Vector3 endPosition
    )
    {
        if (elapsed <= chargeDuration)
        {
            transform.position = rootStartPosition;
            return;
        }

        float movementElapsed = elapsed - chargeDuration;

        float movementT = Mathf.Clamp01(
            movementElapsed / Mathf.Max(0.01f, moveDuration)
        );

        float easedT = EaseOutCubic(movementT);

        transform.position = Vector3.Lerp(
            rootStartPosition,
            endPosition,
            easedT
        );
    }

    private void UpdateWaveScale(float elapsed)
    {
        float scale;

        if (elapsed <= chargeDuration)
        {
            float chargeT = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, chargeDuration)
            );

            scale = Mathf.Lerp(
                startScale,
                chargeScale,
                EaseOutBack(chargeT)
            );
        }
        else
        {
            float movementElapsed = elapsed - chargeDuration;

            float movementT = Mathf.Clamp01(
                movementElapsed / Mathf.Max(0.01f, moveDuration)
            );

            scale = Mathf.Lerp(
                chargeScale,
                maxScale,
                EaseOutCubic(movementT)
            );
        }

        ApplyUniformScale(
            waveSlashA,
            waveAStartLocalScale,
            scale
        );

        ApplyUniformScale(
            waveSlashB,
            waveBStartLocalScale,
            scale
        );
    }

    private void UpdateWaveMotion(float totalT)
    {
        float waveSin = Mathf.Sin(totalT * Mathf.PI * 4f);

        if (waveSlashA != null)
        {
            Vector3 localPosition = waveAStartLocalPosition;
            localPosition.y += waveSin * waveVerticalAmount;

            waveSlashA.localPosition = localPosition;

            waveSlashA.localRotation =
                waveAStartLocalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    waveSin * waveRotationAmount
                );
        }

        if (waveSlashB != null)
        {
            Vector3 localPosition = waveBStartLocalPosition;
            localPosition.y -= waveSin * waveVerticalAmount;

            waveSlashB.localPosition = localPosition;

            waveSlashB.localRotation =
                waveBStartLocalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    -waveSin * waveRotationAmount
                );
        }
    }

    private void UpdateFade(float totalT)
    {
        if (totalT < fadeStartNormalizedTime)
        {
            SetAllSpriteAlpha(1f);
            return;
        }

        float fadeT = Mathf.InverseLerp(
            fadeStartNormalizedTime,
            1f,
            totalT
        );

        float alpha = Mathf.Lerp(1f, 0f, fadeT);
        SetAllSpriteAlpha(alpha);
    }

    private IEnumerator PlayFishJump(
        Transform fish,
        Vector3 startLocalPosition,
        Quaternion startLocalRotation,
        float delay,
        float horizontalOffset
    )
    {
        if (fish == null)
        {
            yield break;
        }

        fish.gameObject.SetActive(false);

        yield return new WaitForSeconds(delay);

        fish.gameObject.SetActive(true);

        float elapsed = 0f;
        float direction = moveLeft ? -1f : 1f;

        while (elapsed < fishJumpDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, fishJumpDuration)
            );

            float yOffset =
                4f *
                fishJumpHeight *
                t *
                (1f - t);

            float xOffset =
                horizontalOffset *
                direction *
                t;

            fish.localPosition =
                startLocalPosition +
                new Vector3(xOffset, yOffset, 0f);

            float spin =
                Mathf.Lerp(
                    -fishSpinAmount,
                    fishSpinAmount,
                    t
                ) * direction;

            fish.localRotation =
                startLocalRotation *
                Quaternion.Euler(0f, 0f, spin);

            yield return null;
        }

        fish.gameObject.SetActive(false);
    }

    private IEnumerator PlaySplash()
    {
        if (splashBurst == null)
        {
            yield break;
        }

        splashBurst.SetActive(false);

        yield return new WaitForSeconds(splashTime);

        splashBurst.SetActive(true);

        ParticleSystem splashParticle =
            splashBurst.GetComponent<ParticleSystem>();

        if (splashParticle != null)
        {
            splashParticle.Clear();
            splashParticle.Play();
        }

        yield return new WaitForSeconds(splashVisibleDuration);

        splashBurst.SetActive(false);
    }

    private void ResetWave(
        Transform wave,
        Vector3 startLocalPosition,
        Quaternion startLocalRotation
    )
    {
        if (wave == null)
        {
            return;
        }

        wave.gameObject.SetActive(true);
        wave.localPosition = startLocalPosition;
        wave.localRotation = startLocalRotation;
    }

    private static void ResetFish(
        Transform fish,
        Vector3 startLocalPosition,
        Quaternion startLocalRotation
    )
    {
        if (fish == null)
        {
            return;
        }

        fish.localPosition = startLocalPosition;
        fish.localRotation = startLocalRotation;
        fish.gameObject.SetActive(false);
    }

    private static void CacheFishInitialValue(
        Transform fish,
        out Vector3 localPosition,
        out Quaternion localRotation
    )
    {
        if (fish != null)
        {
            localPosition = fish.localPosition;
            localRotation = fish.localRotation;
        }
        else
        {
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
        }
    }

    private static void ApplyUniformScale(
        Transform target,
        Vector3 originalScale,
        float multiplier
    )
    {
        if (target == null)
        {
            return;
        }

        target.localScale = new Vector3(
            originalScale.x * multiplier,
            originalScale.y * multiplier,
            originalScale.z
        );
    }

    private void SetAllSpriteAlpha(float alpha)
    {
        if (
            spriteRenderers == null ||
            originalColors == null
        )
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }

            Color color = originalColors[i];
            color.a *= alpha;
            spriteRenderers[i].color = color;
        }
    }

    private static float EaseOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return 1f - Mathf.Pow(1f - value, 3f);
    }

    private static float EaseOutBack(float value)
    {
        value = Mathf.Clamp01(value);

        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f +
               c3 * Mathf.Pow(value - 1f, 3f) +
               c1 * Mathf.Pow(value - 1f, 2f);
    }
}