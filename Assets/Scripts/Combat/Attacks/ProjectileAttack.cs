using TwoWorlds.Combat;
using UnityEngine;

public class ProjectileAttack : MonoBehaviour
{
    [SerializeField] AttackData attackData;
    float maxScaleUpMultiplier;
    Vector3 targetMaxScale;

    void Awake()
    {
        maxScaleUpMultiplier = attackData.ChargeMultiplier;
        targetMaxScale = transform.localScale * maxScaleUpMultiplier;
    }

    private void Update()
    {
        if (transform.localScale.x <= targetMaxScale.x)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetMaxScale, Time.deltaTime);
        }

    }
}
