using Photon.Pun;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviourPun
{


    public static CameraManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    public CinemachineCamera batsmanCamera;
    public CinemachineCamera bowlerCamera;
    public CinemachineCamera bowlCamera;
    public Transform batsman;
    public Transform bowler;
    public Transform bowl;



    public void LaunchBallCamera()
    {
       // bowlerCamera.LookAt = batsman;
        //bowlerCamera.Follow = batsman;

    }

    public void BallHitCamera()
    {

    }

    public void ResetCamera()
    {
        bowlerCamera.LookAt = bowler;
        bowlerCamera.Follow = bowler;
    }


}
