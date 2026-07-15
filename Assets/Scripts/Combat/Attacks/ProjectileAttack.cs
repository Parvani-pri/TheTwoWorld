using System.Collections;
using TwoWorlds.Combat;
using UnityEngine;

public class ProjectileAttack : MonoBehaviour
{
    [SerializeField] AttackData attackData;
    Transform playerTransform;
    float maxScaleUpMultiplier;
    Vector3 targetMaxScale;

    Vector3 targetDir;
    float targetSpeed = 3f;
    bool shouldScale = true;
    bool shouldMove = false;

    void Awake()
    {
        maxScaleUpMultiplier = attackData.ChargeMultiplier;
        targetMaxScale = transform.localScale * maxScaleUpMultiplier;
    }


    void Start()
    {
        playerTransform = FindFirstObjectByType<PlayerCombatController>().transform;
    }

    void Update()
    {
        if (shouldMove)
        {
            transform.position += targetDir * targetSpeed * Time.deltaTime;
        }
    }

    public void Charge(float duration)
    {
        StartCoroutine(ChargeCoroutine(duration));
    }

    IEnumerator ChargeCoroutine(float duration)
    {
        print("start charging");
        print(targetMaxScale);
        print(transform.localScale);
        float timer = 0f;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = transform.localScale * maxScaleUpMultiplier;
        while (timer < duration && shouldScale)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetMaxScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        if (timer >= duration)
        {
            transform.localScale = targetScale;
        }

    }
    public void Release()
    {
        targetDir = (transform.position - playerTransform.position).normalized;
        shouldScale = false;
        shouldMove = true;
    }
}
