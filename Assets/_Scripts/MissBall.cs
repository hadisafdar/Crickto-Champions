using UnityEngine;

public class MissBall : MonoBehaviour
{

    public static bool IsBallMiss= false;

    private void OnTriggerEnter(Collider other)
    {
        if (!CricketGameManager.Instance.batsmanController.photonView.IsMine) return;
        if (other.CompareTag("Ball"))
        {
            Debug.Log("Miss Ball");
            IsBallMiss = true;
            if (!GameManager.IsGameReset)
            {
                Invoke(nameof(StopTheBall), 2f);

            }
        }
    }


    private void Start()
    {
        GameManager.OnResetGame += OnGameReset;
        WicketsOutAnimation.OnWicketOut += OnGameReset;
    }

    private void OnGameReset()
    {
        CancelInvoke();
    }

    public void StopTheBall()
    {
        Debug.Log("Reset called on missball");
        //GameManager.Instance.ResetAll();
        CricketScoreManager.Instance.AddScore(0);
    }
}
