using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoWorlds.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] float interactRadius = 1.5f;
        [SerializeField] LayerMask interactableLayers = ~0;
        [SerializeField] bool showDebugGizmos = true;

        IInteractable currentTarget;
        bool gameplayBlocked;
        bool inventoryOpen;

        void Start()
        {
            if (inputReader == null)
                inputReader = FindFirstObjectByType<InputReader>();

            if (inputReader?.InteractAction != null)
                inputReader.InteractAction.performed += OnInteractPerformed;
            else
                Debug.LogError("[PlayerInteractor] InputReader or Interact action is missing.");
        }

        void OnDestroy()
        {
            if (inputReader?.InteractAction != null)
                inputReader.InteractAction.performed -= OnInteractPerformed;
        }

        void OnEnable()
        {
            GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged += OnInventoryOpenChanged;
        }

        void OnDisable()
        {
            GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged -= OnInventoryOpenChanged;
        }

        void Update()
        {
            if (IsInputBlocked())
            {
                currentTarget = null;
                return;
            }

            currentTarget = FindClosestInteractable();
        }

        void OnInteractPerformed(InputAction.CallbackContext _)
        {
            if (IsInputBlocked())
                return;

            if (currentTarget == null)
                return;

            if (currentTarget.CanInteract(gameObject))
                currentTarget.Interact(gameObject);
        }

        IInteractable FindClosestInteractable()
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = interactableLayers,
                useTriggers = true
            };

            var hits = new Collider2D[16];
            var hitCount = Physics2D.OverlapCircle(transform.position, interactRadius, filter, hits);

            IInteractable closest = null;
            var closestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                if (hit.transform == transform)
                    continue;

                var interactable = GetInteractable(hit);
                if (interactable == null || !interactable.CanInteract(gameObject))
                    continue;

                var distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
                closest = interactable;
            }

            return closest;
        }

        static IInteractable GetInteractable(Collider2D collider)
        {
            var behaviours = collider.GetComponents<MonoBehaviour>();
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IInteractable interactable)
                    return interactable;
            }

            return collider.GetComponentInParent<IInteractable>();
        }

        void OnGameplayInputBlocked(bool blocked) => gameplayBlocked = blocked;

        void OnInventoryOpenChanged(bool open) => inventoryOpen = open;

        bool IsInputBlocked() => gameplayBlocked || inventoryOpen;

        void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos)
                return;

            Gizmos.color = currentTarget != null ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
