using System;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    public static event Action OnSixerAndFour; // Pass score when event triggers
    public static int CurrentScore { get; private set; } // Prevents direct modification

    public bool isOutsideBoundary; // Flag to check if the ball is out
    public int boundaryScore; // Default is a four

    private void OnTriggerExit(Collider other)
    {
        if (!Ground.BowlHit || !other.CompareTag("Ball")) return; // Ensure ball has hit the ground first
        if (FielderController.isBallPicked || isOutsideBoundary) return; // Prevents duplicate scoring
        Debug.Log("score is "+ CurrentScore);
        if (boundaryScore < 4)
        {
            CurrentScore = boundaryScore;
            return;
        }

       

        CurrentScore = Ground.canSixer ? 6 : 4; // Determines if it's a Six or a Four

        CricketScoreManager.Instance.AddScore(CurrentScore);
        Debug.Log(CurrentScore == 6 ? "🏏 SIX!" : "🏏 FOUR!");

        OnSixerAndFour?.Invoke(); // Trigger event for score update
    }
}
