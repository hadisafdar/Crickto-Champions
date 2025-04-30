using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using System;

public class WicketsOutAnimation : MonoBehaviourPun
{
    [Header("Wicket Components")]
    public Transform leftStump;
    public Transform middleStump;
    public Transform rightStump;
    public Transform leftBail;
    public Transform rightBail;

    [Header("Stump Rotation Settings")]
    public float leftStumpRotationAngle = 45f;
    public float middleStumpRotationAngle = 30f;
    public float rightStumpRotationAngle = 60f;
    public float leftStumpFallDuration = 0.5f;
    public float middleStumpFallDuration = 0.6f;
    public float rightStumpFallDuration = 0.4f;

    [Header("Bail Fly Settings")]
    public float bailFlyDuration = 0.5f;
    public float bailFlyHeight = 1f;

    [Header("Physics Detection Settings")]
    public Vector3 boxSize = new Vector3(0.5f, 1f, 0.5f); // Adjust as needed
    public LayerMask ballLayer; // Assign only the "Ball" layer in Unity

    private bool animationPlayed = false;
    private bool wicketOutTriggered = false;
    public Ease easyType;

    private Vector3 leftStumpStartPos, middleStumpStartPos, rightStumpStartPos;
    private Quaternion leftStumpStartRot, middleStumpStartRot, rightStumpStartRot;
    private Vector3 leftBailStartPos, rightBailStartPos;
    private Quaternion leftBailStartRot, rightBailStartRot;

    public static event Action OnWicketOut;

    private void Start()
    {
        SaveInitialPositions();
    }

    private void Update()
    {
        if (!wicketOutTriggered)
        {
            CheckForBallHit();
        }
    }

    /// <summary>
    /// Saves the initial positions and rotations of all wickets.
    /// </summary>
    private void SaveInitialPositions()
    {
        if (leftStump) { leftStumpStartPos = leftStump.position; leftStumpStartRot = leftStump.rotation; }
        if (middleStump) { middleStumpStartPos = middleStump.position; middleStumpStartRot = middleStump.rotation; }
        if (rightStump) { rightStumpStartPos = rightStump.position; rightStumpStartRot = rightStump.rotation; }
        if (leftBail) { leftBailStartPos = leftBail.position; leftBailStartRot = leftBail.rotation; }
        if (rightBail) { rightBailStartPos = rightBail.position; rightBailStartRot = rightBail.rotation; }
    }

    /// <summary>
    /// Checks if the ball is inside the detection area using Physics.OverlapBox.
    /// </summary>
    private void CheckForBallHit()
    {
        Collider[] hitColliders = Physics.OverlapBox(transform.position, boxSize / 2, Quaternion.identity, ballLayer);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Ball") && !wicketOutTriggered)
            {
                if (photonView.IsMine) // Ensure only one client triggers the RPC
                {
                    wicketOutTriggered = true;
                    photonView.RPC(nameof(WicketOutRPC), RpcTarget.All);
                }
                break; // Stop checking after detecting a ball
            }
        }
    }

    /// <summary>
    /// Plays the wicket out animation.
    /// </summary>
    public void PlayWicketsOutAnimation()
    {
        if (animationPlayed) return;
        animationPlayed = true;

        AnimateStumpRotation(leftStump, Vector3.right * leftStumpRotationAngle, leftStumpFallDuration);
        AnimateStumpRotation(middleStump, Vector3.right * middleStumpRotationAngle, middleStumpFallDuration);
        AnimateStumpRotation(rightStump, Vector3.right * rightStumpRotationAngle, rightStumpFallDuration);

        AnimateBailFly(leftBail, new Vector3(-0.5f, bailFlyHeight, 0));
        AnimateBailFly(rightBail, new Vector3(0.5f, bailFlyHeight, 0));
    }

    private void AnimateStumpRotation(Transform stump, Vector3 rotationAxis, float duration)
    {
        if (stump == null) return;
        stump.DORotate(rotationAxis, duration, RotateMode.LocalAxisAdd).SetEase(easyType);
    }

    private void AnimateBailFly(Transform bail, Vector3 flyDirection)
    {
        if (bail == null) return;
        Vector3 targetPosition = bail.position + flyDirection;

        bail.DOMove(targetPosition, bailFlyDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                bail.DOMoveY(bail.position.y - 0.5f, 0.5f).SetEase(easyType);
            });
    }

    /// <summary>
    /// Resets the wicket animation back to its original position and rotation.
    /// </summary>
    public void ResetWicketsAnimation()
    {
        animationPlayed = false;
        wicketOutTriggered = false;

        leftStump?.DORotateQuaternion(leftStumpStartRot, 0.5f).SetEase(Ease.OutQuad);
        middleStump?.DORotateQuaternion(middleStumpStartRot, 0.5f).SetEase(Ease.OutQuad);
        rightStump?.DORotateQuaternion(rightStumpStartRot, 0.5f).SetEase(Ease.OutQuad);

        leftStump?.DOMove(leftStumpStartPos, 0.5f).SetEase(Ease.OutQuad);
        middleStump?.DOMove(middleStumpStartPos, 0.5f).SetEase(Ease.OutQuad);
        rightStump?.DOMove(rightStumpStartPos, 0.5f).SetEase(Ease.OutQuad);

        leftBail?.DOMove(leftBailStartPos, 0.5f).SetEase(Ease.OutQuad);
        rightBail?.DOMove(rightBailStartPos, 0.5f).SetEase(Ease.OutQuad);

        leftBail?.DORotateQuaternion(leftBailStartRot, 0.5f).SetEase(Ease.OutQuad);
        rightBail?.DORotateQuaternion(rightBailStartRot, 0.5f).SetEase(Ease.OutQuad);
    }

    [PunRPC]
    public void WicketOutRPC()
    {
        if (animationPlayed) return;
        PlayWicketsOutAnimation();
        OnWicketOut?.Invoke();
        CricketScoreManager.Instance.AnimateOut();
    }

    /// <summary>
    /// Draws the OverlapBox in Scene View for debugging.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
