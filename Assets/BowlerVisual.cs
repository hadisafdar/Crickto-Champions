using UnityEngine;

public class BowlerVisual : MonoBehaviour
{
    public Avatar Avatar;
    

    public BowlerController Controller;

    public Animator Animator;


    public Transform ballParent;

    public void SetBowler()
    {
        Animator.avatar = Avatar;
        Controller.ballParent = ballParent;
        Controller.SetBowler();

    }
}
