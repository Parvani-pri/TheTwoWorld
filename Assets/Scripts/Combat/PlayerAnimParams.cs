using UnityEngine;

public static class PlayerAnimParams
{
    public static readonly int SPEED = Animator.StringToHash("speed");
    public static readonly int ATTACK = Animator.StringToHash("attack");
    public static readonly int ATTACK_INDEX = Animator.StringToHash("attackIndex");

    public static readonly int ON_HURT = Animator.StringToHash("onHurt");
    public static readonly int ON_DIE = Animator.StringToHash("onDie");
    public static readonly int IS_WALK = Animator.StringToHash("isWalk");
    public static readonly int IS_RUN = Animator.StringToHash("isRun");
}