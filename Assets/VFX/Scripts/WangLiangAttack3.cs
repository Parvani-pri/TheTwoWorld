using UnityEngine;

public class WangLiangAttack3 : MonoBehaviour
{
    [Header("Attack FX")]
    [SerializeField] private GameObject waterAttackPrefab;

    [SerializeField] private Transform attackSpawnPoint;

    [Header("Direction")]
    [SerializeField] private bool faceLeft = true;

    /// <summary>
    /// Animation Event 呼叫
    /// </summary>
    public void SpawnWaterAttackFX()
    {
        if (waterAttackPrefab == null)
        {
            Debug.LogWarning("Water Attack Prefab Missing");
            return;
        }

        Transform spawn = attackSpawnPoint != null
            ? attackSpawnPoint
            : transform;

        GameObject fx = Instantiate(
            waterAttackPrefab,
            spawn.position,
            Quaternion.identity
        );

        // 如果角色向右，翻轉 FX
        if (!faceLeft)
        {
            Vector3 scale = fx.transform.localScale;
            scale.x *= -1;
            fx.transform.localScale = scale;
        }
    }
}
