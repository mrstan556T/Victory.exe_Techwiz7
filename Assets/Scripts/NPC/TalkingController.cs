using UnityEngine;

public class TalkingController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private bool isTalking = true; 
    void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        animator.SetBool("isTalking", isTalking);
    }
}
