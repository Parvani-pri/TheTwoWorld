using System;
using System.Collections;
using TwoWorlds.Combat;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackUI : MonoBehaviour
{
    [SerializeField] Image attack1;
    [SerializeField] Image attack2;
    [SerializeField] Image attack2Charge;
    [SerializeField] Image attack3;

    [SerializeField] AttackData attack1Data;
    [SerializeField] AttackData attack2Data;
    [SerializeField] float chargeMaxTimer;
    [SerializeField] AttackData attack3Data;

    static event Action<float> OnAttack1Launched;
    static event Action<float> OnAttack2Launched;
    static event Action OnAttack2Charging;
    static event Action OnAttack2Released;
    static event Action<float> OnAttack3Launched;

    static bool shouldCharge;
    private void Awake()
    {
        attack1.fillAmount = 0f;
        attack2.fillAmount = 0f;
        attack2Charge.fillAmount = 0f;
        attack3.fillAmount = 0f;
    }

    private void OnEnable()
    {
        OnAttack1Launched += PlayerAttackUI_OnAttack1Launched;
        OnAttack2Launched += PlayerAttackUI_OnAttack2Launched;
        OnAttack2Charging += PlayerAttackUI_OnAttack2Charging;
        OnAttack2Released += PlayerAttackUI_OnAttack2Released;
        OnAttack3Launched += PlayerAttackUI_OnAttack3Launched;
    }


    private void OnDisable()
    {
        OnAttack1Launched -= PlayerAttackUI_OnAttack1Launched;
        OnAttack2Launched -= PlayerAttackUI_OnAttack2Launched;
        OnAttack2Charging -= PlayerAttackUI_OnAttack2Charging;
        OnAttack2Released -= PlayerAttackUI_OnAttack2Released;
        OnAttack3Launched -= PlayerAttackUI_OnAttack3Launched;
    }
    private void PlayerAttackUI_OnAttack1Launched(float cooldown)
    {
        attack1.fillAmount = 1;
        StartCoroutine(StartAttackCD(attack1Data.Cooldown, attack1));

    }
    private void PlayerAttackUI_OnAttack2Launched(float cooldown)
    {
        attack2.fillAmount = 1;
        StartCoroutine(StartAttackCD(attack2Data.Cooldown, attack2));
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
        StartCoroutine(StartAttackCD(attack3Data.Cooldown, attack3));
    }

    IEnumerator StartAttackCD(float cooldown, Image attack)
    {
        float timeLapsed = 0f;
        float initialFill = 1f;
        while (timeLapsed < cooldown)
        {
            attack.fillAmount = Mathf.Lerp(initialFill, 0f, timeLapsed / cooldown);
            timeLapsed += Time.deltaTime;
            yield return null;
        }
        attack.fillAmount = 0f;
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
}
