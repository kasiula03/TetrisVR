using System;
using Autohand.Demo;
using UnityEngine;

public class HandStateLinker : MonoBehaviour
{
    [SerializeField] private OVRHandControllerLink _handControllerLink;

    public bool IsPunchPose;
    
    private bool _grabbing;
    private bool _squeezing;

    private void Start()
    {
        _handControllerLink.OnGrabAction += OnGrab;
        _handControllerLink.OnSqueezeAction += OnSqueeze;
    }

    private void OnSqueeze(bool state)
    {
        _squeezing = state;
        MatchState();
    }

    private void OnGrab(bool state)
    {
        _grabbing = state;
        MatchState();
    }

    private void MatchState()
    {
        IsPunchPose = _grabbing && _squeezing;
        if (IsPunchPose)
        {
            _handControllerLink.hand.ForceReleaseGrab();
        }
    }
}