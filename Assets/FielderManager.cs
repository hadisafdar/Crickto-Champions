using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FielderManager : MonoBehaviourPun
{
    public static FielderManager Instance { get; private set; }

    [HideInInspector] public List<FielderController> fielders = new List<FielderController>();
    [HideInInspector] public Transform ballTransform;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Register(FielderController f) => fielders.Add(f);
    public void Unregister(FielderController f) => fielders.Remove(f);

    public void EvaluateChaser()
    {
        if (!PhotonNetwork.IsMasterClient || ballTransform == null) return;

        FielderController closest = null;
        float minDist = float.MaxValue;

        // Find the closest fielder whose trigger is “active”
        foreach (var f in fielders)
        {
            if (!f.isBallInTriggerArea) continue;
            float d = Vector3.Distance(f.transform.position, ballTransform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = f;
            }
        }

        // Tell each fielder whether they should chase or not
        foreach (var f in fielders)
            f.SetChasePermission(f == closest);
    }
}
