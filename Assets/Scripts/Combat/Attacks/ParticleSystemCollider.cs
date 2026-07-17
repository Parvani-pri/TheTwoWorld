using NUnit.Framework;
using System.Collections.Generic;
using TwoWorlds.Combat;
using UnityEngine;

public class ParticleSystemCollider : MonoBehaviour
{

    [SerializeField] AttackData attackData;
    ParticleSystem ps;
    List<Collider2D> colliders;
    private List<ParticleSystem.Particle> enterParticles = new List<ParticleSystem.Particle>();
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        transform.parent = null;
    }

    public void AddCollider2D(Collider2D collider2D)
    {
        var ps_temp = ps;
        var triggerModule = ps_temp.trigger;

        // 2. Enable the module
        triggerModule.enabled = true;

        // 3. Set how the particles behave on interaction (e.g., Kill, Callback, or Ignore)
        triggerModule.enter = ParticleSystemOverlapAction.Callback;

        // 4. Assign the collider to a specific index slot (e.g., index 0)
        triggerModule.SetCollider(0, collider2D);
        ps = ps_temp;
        colliders.Add(collider2D);
    }

    void OnParticleTrigger()
    {
        // 1. Collect all particles that entered the trigger zone this frame
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterParticles, out var colliderData);

        if (numEnter > 0)
        {
            Component other = colliderData.GetCollider(0, 0);
            if (other != null)
            {
                if (other.GetComponentInParent<EnemyAttackAI>() != null)
                {

                    if (other.GetComponentInParent<CombatHealth>() != null)
                    {

                        other.GetComponentInParent<CombatHealth>().TakeDamage(attackData.Damage);
                    }
                }
            }
        }
    }
}
