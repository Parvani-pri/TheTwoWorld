using TwoWorlds.Combat;
using UnityEngine;

public class ParticleSystemCollider : MonoBehaviour
{
    private void OnEnable()
    {
        transform.parent = null;
    }

    [SerializeField] AttackData attackData;
    private void OnParticleCollision(GameObject other)
    {
        print("enemy hit");
        if (other.GetComponentInParent<EnemyAttackAI>() != null)
        {

            if (other.GetComponentInParent<CombatHealth>() != null)
            {

                other.GetComponentInParent<CombatHealth>().TakeDamage(attackData.Damage);
            }
        }
    }
}
