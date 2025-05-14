using UnityEngine;

public class Ground : MonoBehaviour
{
    public static bool BowlHit;
    public static bool canSixer = true;
    public static bool GroundHit = false;
    private void OnCollisionStay(Collision collision)
    {
        if(GroundHit) return;
        if (BowlHit && collision.gameObject.CompareTag("Ball"))
        {
            GroundHit = true;
            Debug.Log("Cannot Sixer");
            canSixer = false;
            AudioManager.instance.Play("GroundBowl");
            if (!GameManager.IsGameReset)
            {
               // Invoke(nameof(StopTheBall), 6f);

            }
        }
    }


    private void Start()
    {
        GameManager.OnResetGame += OnGameReset;
    }

    private void OnGameReset()
    {
        GroundHit = false;
        canSixer = true;
    }
    public void StopTheBall()
    {
        BowlHit = false;
        canSixer = true;
        Debug.Log("Reset called on Ground");
       // GameManager.Instance.ResetAll();
    }
}
