using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class BallNetworkSync : MonoBehaviourPun, IPunObservable
{
    Rigidbody _rb;

    // for remote clients:
    Vector3 _networkPosition;
    Quaternion _networkRotation;
    Vector3 _networkVelocity;
    Vector3 _networkAngularVelocity;

    [Tooltip("How quickly non‐owners lerp to the authoritative state")]
    public float lerpRate = 15f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // only the owner (MasterClient) simulates physics
        if (!photonView.IsMine)
        {
            // make it kinematic so no local physics runs
            _rb.isKinematic = true;
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            // smooth interpolation for remote clients
            transform.position = Vector3.Lerp(transform.position, _networkPosition,
                                              Time.fixedDeltaTime * lerpRate);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation,
                                                 Time.fixedDeltaTime * lerpRate);

            // update velocity so that any on‐collision effects remain consistent
            _rb.linearVelocity = _networkVelocity;
            _rb.angularVelocity = _networkAngularVelocity;
        }
    }

    // IPunObservable is called automatically by Photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // this client **owns** the ball → send data
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(_rb.linearVelocity);
            stream.SendNext(_rb.angularVelocity);
        }
        else // remote clients → receive data
        {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            _networkVelocity = (Vector3)stream.ReceiveNext();
            _networkAngularVelocity = (Vector3)stream.ReceiveNext();
        }
    }
}
