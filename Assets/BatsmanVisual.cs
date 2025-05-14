using UnityEngine;

public class BatsmanVisual : MonoBehaviour
{
    public Avatar avatar;
    public BallTriggerLauncher ballTriggerLauncher;
    public BoxCollider BoxCollider;


    public Animator animator;
    public BatsmanController controller;
    public void SetBatsman()
    {
        controller.ballTriggerLauncher = ballTriggerLauncher;
        animator.avatar = avatar;
        controller.battingTrigger = BoxCollider;
    }
}
