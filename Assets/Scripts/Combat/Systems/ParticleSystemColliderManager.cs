using TwoWorlds.Combat;
using UnityEngine;

public class ParticleSystemColliderManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    public static ParticleSystemColliderManager Instance;

    private void Awake()
    {
        // Check if an instance already exists in the scene
        if (Instance != null && Instance != this)
        {
            // Destroy the duplicate GameObject to enforce the singleton rule
            Destroy(gameObject);
            return;
        }

        // Assign the static reference to this instance
        Instance = this;
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
    }
}
