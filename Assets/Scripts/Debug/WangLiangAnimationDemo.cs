using System.Collections;
using UnityEngine;

/// <summary>
/// Debug-only reference for driving every state in WangLiangAnimator.
///
/// Add this component to WangLiang, enter Play Mode, and use the on-screen buttons.
/// Gameplay code can call the public methods below or copy their Animator calls.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class WangLiangAnimationDemo : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool showDebugPanel = true;
    [SerializeField, Min(0.1f)] private float locomotionPreviewSeconds = 2f;
    [SerializeField, Min(0.1f)] private float attackClipSeconds = 2.083f;

    private static readonly int OnHurt = Animator.StringToHash("onHurt");
    private static readonly int OnDie = Animator.StringToHash("onDie");
    private static readonly int IsJump = Animator.StringToHash("isJump");
    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int IsWalk = Animator.StringToHash("isWalk");
    private static readonly int CanCombo = Animator.StringToHash("canCombo");

    private Coroutine demoRoutine;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    // idle: clear every persistent parameter. The controller returns to idle.
    public void PlayIdle()
    {
        StopDemoRoutine();
        ResetParameters();
    }

    // walk: isWalk=true; return to idle with isWalk=false.
    public void PlayWalk()
    {
        StopDemoRoutine();
        ResetParameters();
        animator.SetBool(IsWalk, true);
    }

    // run: isRun=true; return to idle with isRun=false.
    public void PlayRun()
    {
        StopDemoRoutine();
        ResetParameters();
        animator.SetBool(IsRun, true);
    }

    // jump: isJump=true and isGrounded=false. Call Land() to return to idle.
    public void PlayJump()
    {
        StopDemoRoutine();
        ResetParameters();
        animator.SetBool(IsGrounded, false);
        animator.SetBool(IsJump, true);
    }

    public void Land()
    {
        animator.SetBool(IsJump, false);
        animator.SetBool(IsGrounded, true);
    }

    // hurt and die are Animator triggers.
    public void PlayHurt()
    {
        StopDemoRoutine();
        ResetParameters();
        animator.SetTrigger(OnHurt);
    }

    public void PlayDie()
    {
        StopDemoRoutine();
        ResetParameters();
        animator.SetTrigger(OnDie);
    }

    // Attack=1/2/3 selects the matching attack from idle.
    public void PlayAttack1() => StartAttack(1);
    public void PlayAttack2() => StartAttack(2);
    public void PlayAttack3() => StartAttack(3);

    // canCombo=true plus Attack 1 -> 2 -> 3 demonstrates chained transitions.
    public void PlayThreeHitCombo()
    {
        StartDemoRoutine(ThreeHitComboRoutine());
    }

    // Runs every animation once for presentations and smoke testing.
    public void PlayAllAnimations()
    {
        StartDemoRoutine(AllAnimationsRoutine());
    }

    private void StartAttack(int attackNumber)
    {
        StopDemoRoutine();
        ResetParameters();
        demoRoutine = StartCoroutine(PulseAttackRoutine(attackNumber));
    }

    private IEnumerator PulseAttackRoutine(int attackNumber)
    {
        animator.SetInteger(Attack, attackNumber);
        yield return null; // Leave the value set for one Animator update.
        animator.SetInteger(Attack, 0);
        demoRoutine = null;
    }

    private IEnumerator ThreeHitComboRoutine()
    {
        ResetParameters();
        animator.SetBool(CanCombo, true);
        animator.SetInteger(Attack, 1);

        yield return new WaitForSeconds(attackClipSeconds * 0.75f);
        animator.SetInteger(Attack, 2);

        yield return new WaitForSeconds(attackClipSeconds);
        animator.SetInteger(Attack, 3);

        yield return new WaitForSeconds(attackClipSeconds);
        ResetParameters();
        demoRoutine = null;
    }

    private IEnumerator AllAnimationsRoutine()
    {
        ResetParameters();
        yield return new WaitForSeconds(0.75f);

        animator.SetBool(IsWalk, true);
        yield return new WaitForSeconds(locomotionPreviewSeconds);
        animator.SetBool(IsWalk, false);
        yield return new WaitForSeconds(0.25f);

        animator.SetBool(IsRun, true);
        yield return new WaitForSeconds(locomotionPreviewSeconds);
        animator.SetBool(IsRun, false);
        yield return new WaitForSeconds(0.25f);

        animator.SetBool(IsGrounded, false);
        animator.SetBool(IsJump, true);
        yield return new WaitForSeconds(locomotionPreviewSeconds);
        Land();
        yield return new WaitForSeconds(0.5f);

        animator.SetTrigger(OnHurt);
        yield return new WaitForSeconds(attackClipSeconds + 0.25f);

        animator.SetInteger(Attack, 1);
        yield return null;
        animator.SetInteger(Attack, 0);
        yield return new WaitForSeconds(attackClipSeconds + 0.25f);

        animator.SetInteger(Attack, 2);
        yield return null;
        animator.SetInteger(Attack, 0);
        yield return new WaitForSeconds(attackClipSeconds + 0.25f);

        animator.SetInteger(Attack, 3);
        yield return null;
        animator.SetInteger(Attack, 0);
        yield return new WaitForSeconds(attackClipSeconds + 0.25f);

        animator.SetTrigger(OnDie);
        demoRoutine = null;
    }

    private void ResetParameters()
    {
        animator.ResetTrigger(OnHurt);
        animator.ResetTrigger(OnDie);
        animator.SetBool(IsJump, false);
        animator.SetBool(IsRun, false);
        animator.SetBool(IsGrounded, true);
        animator.SetInteger(Attack, 0);
        animator.SetBool(IsWalk, false);
        animator.SetBool(CanCombo, false);
    }

    private void StartDemoRoutine(IEnumerator routine)
    {
        StopDemoRoutine();
        demoRoutine = StartCoroutine(routine);
    }

    private void StopDemoRoutine()
    {
        if (demoRoutine == null)
            return;

        StopCoroutine(demoRoutine);
        demoRoutine = null;
    }

    private void OnGUI()
    {
        if (!showDebugPanel || animator == null)
            return;

        GUILayout.BeginArea(new Rect(16f, 16f, 190f, 390f), GUI.skin.box);
        GUILayout.Label("Wang Liang Animations");

        if (GUILayout.Button("Idle / Reset")) PlayIdle();
        if (GUILayout.Button("Walk")) PlayWalk();
        if (GUILayout.Button("Run")) PlayRun();
        if (GUILayout.Button("Jump")) PlayJump();
        if (GUILayout.Button("Land")) Land();
        if (GUILayout.Button("Hurt")) PlayHurt();
        if (GUILayout.Button("Attack 1")) PlayAttack1();
        if (GUILayout.Button("Attack 2")) PlayAttack2();
        if (GUILayout.Button("Attack 3")) PlayAttack3();
        if (GUILayout.Button("3-Hit Combo")) PlayThreeHitCombo();
        if (GUILayout.Button("Die")) PlayDie();

        GUILayout.Space(8f);
        if (GUILayout.Button("Play All")) PlayAllAnimations();
        GUILayout.EndArea();
    }
}
