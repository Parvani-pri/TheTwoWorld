using System.Collections;
using TwoWorlds.Combat;
using UnityEngine;

public class ProjectileAttack : MonoBehaviour
{
    [SerializeField] AttackData attackData;
    [SerializeField] float rotationSpeed = 20f;
    Transform playerTransform;
    float maxScaleUpMultiplier;
    float currentScale;
    Vector3 targetMaxScale;

    Vector3 targetDir;
    Vector3 finalizedPlayerPosition;
    Vector3 initialProjectilePosition;
    float targetSpeed = 3f;
    bool shouldScale = true;
    bool shouldMove = false;

    float finalDmg;

    void Awake()
    {
        finalDmg = attackData.Damage;
        maxScaleUpMultiplier = attackData.ChargeMultiplier;
        targetMaxScale = transform.localScale * maxScaleUpMultiplier;
    }


    void Start()
    {
        playerTransform = FindFirstObjectByType<PlayerCombatController>().transform;
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, rotationSpeed * currentScale) * Time.deltaTime);
        if (shouldMove)
        {
            transform.position += targetDir * targetSpeed * currentScale * Time.deltaTime;
        }
    }

    public void Charge(float duration)
    {
        StartCoroutine(ChargeCoroutine(duration));
    }

    IEnumerator ChargeCoroutine(float duration)
    {
        float timer = 0f;
        Vector3 initialScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        Vector3 targetScale = transform.localScale * maxScaleUpMultiplier;
        while (timer < duration && shouldScale)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetMaxScale, timer / duration);
            timer += Time.deltaTime;
            currentScale = transform.localScale.y / initialScale.y;
            yield return null;
        }
        if (timer >= duration)
        {
            transform.localScale = targetScale;
        }

    }

    private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.GetComponent<EnemyAttackAI>() != null)
        {
            if (other.GetComponent<CombatHealth>() != null)
            {
                other.GetComponent<CombatHealth>().TakeDamage((int)finalDmg);
                Destroy(gameObject);
            }
        }
    }

    public void Release(float finalDamage)
    {
        transform.parent = null;
        finalDmg = finalDamage;
        finalizedPlayerPosition = playerTransform.position;
        initialProjectilePosition = transform.position;
        targetDir = (initialProjectilePosition - finalizedPlayerPosition).normalized;
        shouldScale = false;
        shouldMove = true;
    }
}
