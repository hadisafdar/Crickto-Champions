using System;
using UnityEngine;

public class MissBall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Debug.Log("Miss Ball");
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
