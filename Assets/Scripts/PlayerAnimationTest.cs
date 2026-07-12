using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationTest : MonoBehaviour
{
     [SerializeField] private Animator anim;

    public bool isGrounded;
    //public float speed; 

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
    }

    private void Update()
    {
        anim.SetBool("isGrounded", isGrounded);
        if (Keyboard.current == null) return;
        

        // Locomotion Blend Tree
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            anim.Play("Base Layer.Locomotion", 0, 0f);
            anim.SetFloat("speed", 0f);   // idle
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            anim.Play("Base Layer.Locomotion", 0, 0f);
            anim.SetFloat("speed", 0.5f); // walk
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            anim.Play("Base Layer.Locomotion", 0, 0f);
            anim.SetFloat("speed", 1f);   // run
        }

        // Jump
        if (Keyboard.current.digit4Key.wasPressedThisFrame && isGrounded)
        {
            
            anim.ResetTrigger("jump");
            anim.SetTrigger("jump");
                
        }

        // Attacks
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            anim.SetTrigger("attack");
            anim.SetInteger("attackIndex", 1);
            anim.Play("Base Layer.attack1", 0, 0f);
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            anim.SetTrigger("attack");
            anim.SetInteger("attackIndex", 2);
            anim.Play("Base Layer.attack2", 0, 0f);
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            anim.SetTrigger("attack");
            anim.SetInteger("attackIndex", 3);
            anim.Play("Base Layer.attack3", 0, 0f);
        }
    }
}
