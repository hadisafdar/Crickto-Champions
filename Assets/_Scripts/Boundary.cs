using Photon.Pun;
using System;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    public static event Action OnSixerAndFour; // Pass score when event triggers
   
   public static void OnSixerOrFour()
    {
        OnSixerAndFour?.Invoke();
    }
    
}
