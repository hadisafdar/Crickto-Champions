using DG.Tweening;
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class CricketScoreManager : MonoBehaviourPunCallbacks
{
    public static CricketScoreManager Instance;

    [SerializeField] private int ballsPerOver = 6; // Standard over size
    private int[] ballScores; // Stores individual ball scores

    public TextMeshProUGUI[] ballScoreTexts; // TMP texts used during the innings
    public TextMeshProUGUI animatedScoreText;  // For the DOTween score animation
    public TextMeshProUGUI totalScoreText;       // Displays the running total score
    public TextMeshProUGUI targetScoreText;      // Displays the target score (set after first innings)
    public RectTransform bottomScreenPosition;   // Start position for animations
    public RectTransform midScreenPosition;      // End position for animations

    // Fields for the innings summary display
    [SerializeField] private GameObject animatedCameraObj; // Animated camera to display after first innings
    [SerializeField] private GameObject inningsCanvasObj;  // Canvas containing score summaries
    [SerializeField] private TextMeshProUGUI[] player1ScoreTexts; // 6 TMP texts for player 1's scores
    [SerializeField] private TextMeshProUGUI[] player2ScoreTexts; // 6 TMP texts for player 2's scores
    [SerializeField] private TextMeshProUGUI inningIndicatorText;  // TMP to indicate which inning it is

    private int totalScore = 0;
    private int currentBall = 0;
    private int targetScore = 0;
    private bool isFirstInnings = true; // true = first innings; false = second innings
    private MatchEndPanel matchEndPanel;

    // New variable: each client stores its own role.
    // Set this appropriately during role swaps so that the batting client sets it to true.
    public bool isBatting;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ballScores = new int[ballsPerOver];
        animatedScoreText.text = "";
        totalScoreText.text = "";
        targetScoreText.text = "";
        ResetOver();
        matchEndPanel = GetComponent<MatchEndPanel>();
    }

    /// <summary>
    /// Called by the Master Client to add a score.
    /// </summary>
    public void AddScore(int score)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_AddScore", RpcTarget.All, score);
    }

    [PunRPC]
    private void RPC_AddScore(int score)
    {
        if (currentBall < ballsPerOver)
        {
            ballScores[currentBall] = score;
            totalScore += score;
            totalScoreText.text = totalScore.ToString();
            ballScoreTexts[currentBall].text = score.ToString();

            // Animate the score appearance.
            AnimateScore(score);

            // During the chase (second innings), if the batting side reaches or exceeds the target,
            // end the match immediately.
            if (!isFirstInnings && totalScore >= targetScore)
            {
                EndMatchUnified();
                return;
            }

            currentBall++;

            if (currentBall >= ballsPerOver)
            {
                OverCompleted();
            }
        }
    }

    private void AnimateScore(int score)
    {
        animatedScoreText.text = score.ToString();
        animatedScoreText.rectTransform.anchoredPosition = bottomScreenPosition.anchoredPosition;

        // Animate moving from bottom to mid position using DOTween.
        animatedScoreText.rectTransform.DOAnchorPos(midScreenPosition.anchoredPosition, 1.0f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Fade out after a 0.5-second delay.
                animatedScoreText.DOFade(0, 0.5f)
                    .SetDelay(0.5f)
                    .OnComplete(() =>
                    {
                        animatedScoreText.text = "";
                        animatedScoreText.alpha = 1; // Reset opacity.
                        GameManager.Instance.GameReset();
                    });
            });
    }

    public void AnimateOut()
    {
        animatedScoreText.text = "Wicket Out";
        animatedScoreText.rectTransform.anchoredPosition = bottomScreenPosition.anchoredPosition;

        animatedScoreText.rectTransform.DOAnchorPos(midScreenPosition.anchoredPosition, 1.0f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                animatedScoreText.DOFade(0, 0.5f)
                    .SetDelay(0.5f)
                    .OnComplete(() =>
                    {
                        animatedScoreText.text = "";
                        animatedScoreText.alpha = 1; // Reset opacity.
                        OverCompleted();
                    });
            });
    }

    public void OverCompleted()
    {
        Debug.Log("Over Completed!");
        if (isFirstInnings)
        {
            // End of first innings: set the target score from player 1's total.
            targetScore = totalScore;
            targetScoreText.text = targetScore.ToString();

            // Show the animated camera and score canvas summary.
            ShowInningsSummary();

            // Wait 8 seconds before turning off the summary and proceeding to second innings.
            StartCoroutine(ProceedToSecondInnings());
        }
        else
        {
            // End of second innings: determine the match outcome.
            EndMatchUnified();
        }
    }

    /// <summary>
    /// The master client determines the match outcome by comparing totalScore and targetScore.
    /// It then sends the outcome (didBattingWin) to all players via RPC.
    /// Each client compares the outcome with its local role (isBatting) to decide win or loss.
    /// </summary>
    public void EndMatchUnified()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            bool didBattingWin = totalScore >= targetScore;
            photonView.RPC("RPC_GameEndOutcome", RpcTarget.All, didBattingWin);
        }
    }

    [PunRPC]
    private void RPC_GameEndOutcome(bool didBattingWin)
    {
        // Each client determines its outcome based on its role.
        // If the local role (isBatting) matches the outcome (didBattingWin),
        // then the local player wins; otherwise, they lose.
        bool localPlayerWin = (isBatting == didBattingWin);
        Debug.Log(localPlayerWin);
        //GameManager.Instance.GameEnd(localPlayerWin, false, matchEndPanel);
    }

    /// <summary>
    /// Activates the animated camera and canvas, populating the canvas with player 1's scores and an inning indicator.
    /// </summary>
    private void ShowInningsSummary()
    {
        if (animatedCameraObj != null)
            animatedCameraObj.SetActive(true);
        if (inningsCanvasObj != null)
            inningsCanvasObj.SetActive(true);

        GameManager.Instance.batsmanPanel.SetActive(false);
        GameManager.Instance.bowlerPanel.SetActive(false);
        GameManager.Instance.scoreUI.SetActive(false);

        // Populate player 1's scores.
        for (int i = 0; i < ballsPerOver; i++)
        {
            if (player1ScoreTexts != null && i < player1ScoreTexts.Length)
                player1ScoreTexts[i].text = ballScores[i].ToString();
        }
        // Clear player 2's score texts.
        for (int i = 0; i < ballsPerOver; i++)
        {
            if (player2ScoreTexts != null && i < player2ScoreTexts.Length)
                player2ScoreTexts[i].text = "";
        }
        if (inningIndicatorText != null)
            inningIndicatorText.text = "First Innings Complete";
    }

    /// <summary>
    /// Waits for 8 seconds, then disables the animated camera and canvas and resets for second innings.
    /// </summary>
    private IEnumerator ProceedToSecondInnings()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SwapRoles());
        }
        yield return new WaitForSeconds(8f);

        if (animatedCameraObj != null)
            animatedCameraObj.SetActive(false);
        if (inningsCanvasObj != null)
            inningsCanvasObj.SetActive(false);

        // Prepare for second innings.
        isFirstInnings = false;
        totalScore = 0;
        totalScoreText.text = totalScore.ToString();
        ResetBallUI();
        GameManager.Instance.GameReset();
    }

    public IEnumerator SwapRoles()
    {
        yield return new WaitForSeconds(0.1f);
        MultiplayerManager.Instance.RequestSwapRoles();
        // Ideally, after swapping roles, each client should update its isBatting flag accordingly.
    }

    private void ResetOver()
    {
        currentBall = 0;
        for (int i = 0; i < ballsPerOver; i++)
        {
            ballScores[i] = 0;
            ballScoreTexts[i].text = ""; // Reset each ball's UI.
        }
    }

    private void ResetBallUI()
    {
        currentBall = 0;
        for (int i = 0; i < ballsPerOver; i++)
        {
            ballScores[i] = 0;
            ballScoreTexts[i].text = "";
        }
    }
}
