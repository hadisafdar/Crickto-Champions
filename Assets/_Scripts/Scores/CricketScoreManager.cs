using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;
using DG.Tweening;

public class CricketScoreManager : MonoBehaviourPunCallbacks
{
    public static CricketScoreManager Instance;

    [SerializeField] private int ballsPerOver = 6;
    private int[] ballScores;

    [Header("UI References")]
    public TextMeshProUGUI[] ballScoreTexts;
    public TextMeshProUGUI animatedScoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI targetScoreText;
    public RectTransform bottomScreenPosition;
    public RectTransform midScreenPosition;

    [Header("Innings Summary")]
    [SerializeField] private GameObject animatedCameraObj;
    [SerializeField] private GameObject inningsCanvasObj;
    [SerializeField] private TextMeshProUGUI[] player1ScoreTexts;
    [SerializeField] private TextMeshProUGUI[] player2ScoreTexts;
    [SerializeField] private TextMeshProUGUI inningIndicatorText;

    private int totalScore = 0;
    private int currentBall = 0;
    private int targetScore = 0;
    private bool isFirstInnings = true;
    private MatchEndPanel matchEndPanel;

    // Each client sets this when roles are swapped
    public bool isBatting;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ballScores = new int[ballsPerOver];
        animatedScoreText.text = "";
        totalScoreText.text = "";
        targetScoreText.text = "";
        ResetOver();
        matchEndPanel = GetComponent<MatchEndPanel>();
    }

    /// <summary>Called by the master to add runs for a ball.</summary>
    public void AddScore(int score)
    {
        photonView.RPC(nameof(RPC_AddScore), RpcTarget.All, score);
    }

    [PunRPC]
    void RPC_AddScore(int score)
    {
        AddScoreInGame(score);
    }

    void AddScoreInGame(int score)
    {
        if (currentBall >= ballsPerOver) return;

        // record
        ballScores[currentBall] = score;
        totalScore += score;
        totalScoreText.text = totalScore.ToString();
        ballScoreTexts[currentBall].text = score.ToString();

        // animate
        AnimateScore(score);

        if (!isFirstInnings)
        {
            // chase logic: only win if strictly greater
            if (totalScore > targetScore)
            {
                EndMatchUnified(); // batsman wins immediately
                return;
            }
        }

        currentBall++;
        if (currentBall >= ballsPerOver)
        {
            OverCompleted();
        }
    }

    void AnimateScore(int score)
    {
        animatedScoreText.text = score.ToString();
        animatedScoreText.rectTransform.anchoredPosition = bottomScreenPosition.anchoredPosition;

        animatedScoreText.rectTransform
            .DOAnchorPos(midScreenPosition.anchoredPosition, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                animatedScoreText.DOFade(0, 0.5f)
                    .SetDelay(0.5f)
                    .OnComplete(() =>
                    {
                        animatedScoreText.text = "";
                        animatedScoreText.alpha = 1;
                        GameManager.Instance.GameReset();
                    });
            });
    }

    public void AnimateOut()
    {
        animatedScoreText.text = "Wicket Out";
        animatedScoreText.rectTransform.anchoredPosition = bottomScreenPosition.anchoredPosition;

        animatedScoreText.rectTransform
            .DOAnchorPos(midScreenPosition.anchoredPosition, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                animatedScoreText.DOFade(0, 0.5f)
                    .SetDelay(0.5f)
                    .OnComplete(() =>
                    {
                        animatedScoreText.text = "";
                        animatedScoreText.alpha = 1;
                        OverCompleted();
                    });
            });
    }

    void OverCompleted()
    {
        if (isFirstInnings)
        {
            // end of first innings → set target and show summary
            targetScore = totalScore;
            targetScoreText.text = targetScore.ToString();
            ShowInningsSummary();
            StartCoroutine(ProceedToSecondInnings());
        }
        else
        {
            // end of second innings → determine result
            EndMatchUnified();
        }
    }

    /// <summary>
    /// Master decides final result: 1=batsman>target, 0=equal (draw), -1=batsman<target (bowler wins)
    /// </summary>
    void EndMatchUnified()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int result;
        if (totalScore > targetScore) result = 1;
        else if (totalScore == targetScore) result = 0;
        else result = -1;

        photonView.RPC(nameof(RPC_GameEndOutcome), RpcTarget.All, result);
    }

    [PunRPC]
    void RPC_GameEndOutcome(int result)
    {
        // result: 1 → batsman wins, 0 → draw, -1 → bowler wins
        bool isDraw = (result == 0);
        bool localWin = false;

        if (!isDraw)
        {
            // if result==1, batsman wins; if result==-1, bowler wins
            localWin = (result == 1 && isBatting)
                    || (result == -1 && !isBatting);
        }

        GameManager.Instance.GameEnd(localWin, isDraw, matchEndPanel);
    }

    void ShowInningsSummary()
    {
        if (animatedCameraObj != null) animatedCameraObj.SetActive(true);
        if (inningsCanvasObj != null) inningsCanvasObj.SetActive(true);

        GameManager.Instance.batsmanCanvas.SetActive(false);
        GameManager.Instance.bowlerCanvas.SetActive(false);
        GameManager.Instance.inGameScoreUI.SetActive(false);

        for (int i = 0; i < ballsPerOver; i++)
            if (i < player1ScoreTexts.Length)
                player1ScoreTexts[i].text = ballScores[i].ToString();

        for (int i = 0; i < ballsPerOver; i++)
            if (i < player2ScoreTexts.Length)
                player2ScoreTexts[i].text = "";

        if (inningIndicatorText != null)
            inningIndicatorText.text = "First Innings Complete";
    }

    IEnumerator ProceedToSecondInnings()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SwapRoles());
           
        }
        GameManager.Instance.batsmanCanvas.SetActive(false);
        GameManager.Instance.bowlerCanvas.SetActive(false);
        GameManager.Instance.inGameScoreUI.SetActive(false);
        yield return new WaitForSeconds(8f);

        if (animatedCameraObj != null) animatedCameraObj.SetActive(false);
        if (inningsCanvasObj != null) inningsCanvasObj.SetActive(false);

        // reset for second innings
        isFirstInnings = false;
        totalScore = 0;
        totalScoreText.text = "0";
        ResetBallUI();
        GameManager.Instance.GameReset();
    }

    IEnumerator SwapRoles()
    {
        yield return new WaitForSeconds(0.1f);
        CricketGameManager.Instance.SwapRoles();
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.batsmanCanvas.SetActive(false);
        GameManager.Instance.bowlerCanvas.SetActive(false);
        GameManager.Instance.inGameScoreUI.SetActive(false);
    }

    void ResetOver()
    {
        currentBall = 0;
        for (int i = 0; i < ballsPerOver; i++)
        {
            ballScores[i] = 0;
            if (i < ballScoreTexts.Length)
                ballScoreTexts[i].text = "";
        }
    }

    void ResetBallUI()
    {
        currentBall = 0;
        for (int i = 0; i < ballsPerOver; i++)
            if (i < ballScoreTexts.Length)
                ballScoreTexts[i].text = "";
    }
}
