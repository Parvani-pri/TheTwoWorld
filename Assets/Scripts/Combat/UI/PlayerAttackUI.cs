using System;
using System.Collections;
using TwoWorlds.Combat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerAttackUI : MonoBehaviour
{
    [SerializeField] Image attack1;
    [SerializeField] Image attack2;
    [SerializeField] Image attack2Charge;
    [SerializeField] Image attack3;
    [SerializeField] Image mask1;
    [SerializeField] Image mask2;
    [SerializeField] Image mask_No;
    [SerializeField] Image mask1Icon;
    [SerializeField] Image mask2Icon;
    [SerializeField] GameObject mask1_parent;
    [SerializeField] GameObject mask2_parent;
    [SerializeField] GameObject mask_No_parent;
    [SerializeField] EventTrigger mask1Trigger;
    [SerializeField] EventTrigger mask2Trigger;

    public EventTrigger Mask1Trigger => mask1Trigger;
    public EventTrigger Mask2Trigger => mask2Trigger;

    [SerializeField] AttackData attack1Data;
    [SerializeField] AttackData attack2Data;
    [SerializeField] float chargeMaxTimer;
    [SerializeField] AttackData attack3Data;

    static event Action<float> OnAttack1Launched;
    static event Action<float> OnAttack2Launched;
    static event Action OnAttack2Charging;
    static event Action OnAttack2Released;
    static event Action<float> OnAttack3Launched;
    static event Action<float> OnMask1AbilityCast;
    static event Action<float> OnMask2AbilityCast;

    static bool shouldCharge;
    static readonly Vector2 LockedMaskIconSize = new(120f, 120f);
    static readonly Vector2 LockedMaskIconPosition = Vector2.zero;

    Sprite defaultMask1Icon;
    Sprite defaultMask2Icon;
    Vector2 defaultMask1IconSize;
    Vector2 defaultMask2IconSize;
    Vector2 defaultMask1IconPosition;
    Vector2 defaultMask2IconPosition;
    Image.Type defaultMask1IconType;
    Image.Type defaultMask2IconType;
    bool maskIconLayoutCached;

    private void Awake()
    {
        attack1.fillAmount = 0f;
        attack2.fillAmount = 0f;
        attack2Charge.fillAmount = 0f;
        attack3.fillAmount = 0f;

        if (mask1Icon != null)
            defaultMask1Icon = mask1Icon.sprite;

        if (mask2Icon != null)
            defaultMask2Icon = mask2Icon.sprite;

        if (mask1Trigger == null && mask1_parent != null)
            mask1Trigger = mask1_parent.GetComponent<EventTrigger>();

        if (mask2Trigger == null && mask2_parent != null)
            mask2Trigger = mask2_parent.GetComponent<EventTrigger>();

        EnsureMaskIconsResolved();
    }

    void Start()
    {
        EnsureMaskIconsResolved();
        XuFu.MaskSystem.MaskEquipGate.Instance?.RefreshAvailability();
    }

    public void EnsureMaskIconsResolved()
    {
        if (mask1Icon == null && mask1_parent != null)
        {
            var iconTransform = mask1_parent.transform.Find("Mask1 Icon");
            if (iconTransform != null)
                mask1Icon = iconTransform.GetComponent<Image>();
        }

        if (mask2Icon == null && mask2_parent != null)
        {
            var iconTransform = mask2_parent.transform.Find("Mask2 Icon");
            if (iconTransform != null)
                mask2Icon = iconTransform.GetComponent<Image>();
        }

        if (defaultMask1Icon == null && mask1Icon != null)
            defaultMask1Icon = mask1Icon.sprite;

        if (defaultMask2Icon == null && mask2Icon != null)
            defaultMask2Icon = mask2Icon.sprite;

        CacheMaskIconLayout();
    }

    void CacheMaskIconLayout()
    {
        if (maskIconLayoutCached)
            return;

        if (mask1Icon != null)
        {
            var rect = mask1Icon.rectTransform;
            defaultMask1IconSize = rect.sizeDelta;
            defaultMask1IconPosition = rect.anchoredPosition;
            defaultMask1IconType = mask1Icon.type;
        }

        if (mask2Icon != null)
        {
            var rect = mask2Icon.rectTransform;
            defaultMask2IconSize = rect.sizeDelta;
            defaultMask2IconPosition = rect.anchoredPosition;
            defaultMask2IconType = mask2Icon.type;
        }

        maskIconLayoutCached = mask1Icon != null || mask2Icon != null;
    }

    private void OnEnable()
    {
        OnAttack1Launched += PlayerAttackUI_OnAttack1Launched;
        OnAttack2Launched += PlayerAttackUI_OnAttack2Launched;
        OnAttack2Charging += PlayerAttackUI_OnAttack2Charging;
        OnAttack2Released += PlayerAttackUI_OnAttack2Released;
        OnAttack3Launched += PlayerAttackUI_OnAttack3Launched;
        OnMask1AbilityCast += PlayerAttackUI_OnMask1AbilityCast;
        OnMask2AbilityCast += PlayerAttackUI_OnMask2AbilityCast;
    }




    private void OnDisable()
    {
        OnAttack1Launched -= PlayerAttackUI_OnAttack1Launched;
        OnAttack2Launched -= PlayerAttackUI_OnAttack2Launched;
        OnAttack2Charging -= PlayerAttackUI_OnAttack2Charging;
        OnAttack2Released -= PlayerAttackUI_OnAttack2Released;
        OnAttack3Launched -= PlayerAttackUI_OnAttack3Launched;
        OnMask1AbilityCast -= PlayerAttackUI_OnMask1AbilityCast;
        OnMask2AbilityCast -= PlayerAttackUI_OnMask2AbilityCast;
    }
    private void PlayerAttackUI_OnAttack1Launched(float cooldown)
    {
        attack1.fillAmount = 1;
        StartCoroutine(StartCD(attack1Data.Cooldown, attack1));

    }
    private void PlayerAttackUI_OnAttack2Launched(float cooldown)
    {
        attack2.fillAmount = 1;
        StartCoroutine(StartCD(attack2Data.Cooldown, attack2));
    }
    private void PlayerAttackUI_OnAttack2Charging()
    {
        shouldCharge = true;
        StartCoroutine(StartCharging());
    }

    IEnumerator StartCharging()
    {
        float timeElapsed = 0f;
        print(chargeMaxTimer);
        while (timeElapsed < chargeMaxTimer && shouldCharge)
        {

            attack2Charge.fillAmount = Mathf.Lerp(0, 1, timeElapsed / chargeMaxTimer);
            timeElapsed += Time.deltaTime;   
            yield return null;
        }
        if (timeElapsed >= chargeMaxTimer)
        {
            attack2Charge.fillAmount = 1;
        }

    }

   private void PlayerAttackUI_OnAttack2Released()
   { 
        attack2Charge.fillAmount = 0f;
   }

    public static void ReleaseAttack()
    {
        shouldCharge = false;
        OnAttack2Released?.Invoke();
    }

    public static void StartAttackCharge()
    {
        OnAttack2Charging?.Invoke();
    }



    private void PlayerAttackUI_OnAttack3Launched(float cooldown)
    {
        attack3.fillAmount = 1;
        StartCoroutine(StartCD(attack3Data.Cooldown, attack3));
    }

    private void PlayerAttackUI_OnMask1AbilityCast(float cooldown)
    {
        mask1.fillAmount = 1;
        StartCoroutine(StartCD(attack3Data.Cooldown, mask1));
    }
    private void PlayerAttackUI_OnMask2AbilityCast(float cooldown)
    {
        mask2.fillAmount = 1;
        StartCoroutine(StartCD(attack3Data.Cooldown, mask2));
    }


    IEnumerator StartCD(float cooldown, Image item)
    {
        float timeLapsed = 0f;
        float initialFill = 1f;
        while (timeLapsed < cooldown)
        {
            item.fillAmount = Mathf.Lerp(initialFill, 0f, timeLapsed / cooldown);
            timeLapsed += Time.deltaTime;
            yield return null;
        }
        item.fillAmount = 0f;
    }

    public static void OnAttackLaunched(int id, float cooldown)
    {
        switch (id)
        {
            case 1:
                OnAttack1Launched?.Invoke(cooldown);
                break;
            case 2:
                OnAttack2Launched?.Invoke(cooldown);
                break;
            case 3:
                OnAttack3Launched?.Invoke(cooldown);
                break;
            default:
                break;

        }
    }
    public static void OnMaskAbilityCast(int id, float cooldown)
    {
        switch (id)
        {
            case 1:
                OnMask1AbilityCast?.Invoke(cooldown); 
                break;
            case 2:
                OnMask2AbilityCast?.Invoke(cooldown);
                break;
            default:
                break;
        }
    }

    public void SetMaskSlotAvailable(int slotIndex, bool available, Sprite lockedIcon)
    {
        EnsureMaskIconsResolved();
        CacheMaskIconLayout();

        var icon = slotIndex == 1 ? mask1Icon : mask2Icon;
        var defaultIcon = slotIndex == 1 ? defaultMask1Icon : defaultMask2Icon;
        var parent = slotIndex == 1 ? mask1_parent : mask2_parent;

        if (icon == null)
        {
            Debug.LogWarning($"[PlayerAttackUI] Mask icon for slot {slotIndex} is missing.");
            return;
        }

        var rect = icon.rectTransform;
        if (available)
        {
            rect.sizeDelta = slotIndex == 1 ? defaultMask1IconSize : defaultMask2IconSize;
            rect.anchoredPosition = slotIndex == 1 ? defaultMask1IconPosition : defaultMask2IconPosition;
            icon.type = slotIndex == 1 ? defaultMask1IconType : defaultMask2IconType;
            icon.sprite = defaultIcon;
        }
        else
        {
            rect.sizeDelta = LockedMaskIconSize;
            rect.anchoredPosition = LockedMaskIconPosition;
            icon.type = Image.Type.Filled;
            icon.sprite = lockedIcon ?? defaultIcon;
        }

        icon.color = available || lockedIcon != null
            ? Color.white
            : new Color(1f, 1f, 1f, 0.35f);
        icon.enabled = true;

        if (parent == null)
            return;

        var canvasGroup = parent.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = parent.AddComponent<CanvasGroup>();

        canvasGroup.alpha = available ? 1f : 0.55f;
        canvasGroup.interactable = available;
        canvasGroup.blocksRaycasts = available;
    }
}
