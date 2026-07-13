using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI popupText;
    private float disappearCountdown;
    private float moveSpeed = 10f;
    private float fadeOutSpeed = 3f;

    private Color popupTextColor;

    public static DamagePopup Create(Vector3 position, int damageAmount, GameObject prefab)
    {
        Transform popupTransform = Instantiate(prefab, position, Quaternion.identity).transform;
        DamagePopup popup = popupTransform.GetComponent<DamagePopup>();

        popup.Setup(damageAmount);
        return popup;
    }


    private void Awake()
    {
        popupText = GetComponentInChildren<TextMeshProUGUI>();
        popupTextColor = popupText.color;
    }

    public void Setup(float damage)
    {
        popupText.SetText(damage.ToString());
        disappearCountdown = 1f;
    }

    private void Update()
    {
        transform.position += new Vector3(0, moveSpeed) * Time.deltaTime;
        disappearCountdown -= Time.deltaTime;
        if (disappearCountdown < 0)
        {
            popupTextColor.a -= fadeOutSpeed * Time.deltaTime;
            popupText.color = popupTextColor;
            if (popupTextColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
