using System;
using Autohand;
using Autohand.Demo;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Rigidbody))]
public class Puncher : MonoBehaviour
{
    public delegate void PunchEvent(Puncher puncher, Punchable punchable);


    [Tooltip(
        "Can be left empty - The center of mass point to calculate velocity magnitude - for example: the camera of the hammer is a better point vs the pivot center of the hammer object")]
    [SerializeField] private Transform centerOfMassPoint;
    [SerializeField] private float forceMulti = 1;
    [SerializeField] private HandStateLinker _handStateLinker;

    //Progammer Events <3
    public PunchEvent OnPunchEvent;

    private Rigidbody _rb;
    private readonly Vector3[] _velocityOverTime = new Vector3[3];
    private Vector3 _lastPos;
    private float _nextPunchTime = 0;
    public bool InPunchMode => _handStateLinker.IsPunchPose;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate()
    {
        if (!_handStateLinker.IsPunchPose)
            return;
        
        for (int i = 1; i < _velocityOverTime.Length; i++)
        {
            _velocityOverTime[i] = _velocityOverTime[i - 1];
        }

        _velocityOverTime[0] = _lastPos - (centerOfMassPoint ? centerOfMassPoint.position : _rb.position);

        _lastPos = centerOfMassPoint ? centerOfMassPoint.position : _rb.position;
        _nextPunchTime -= Time.deltaTime * 100;
        _nextPunchTime = Math.Max(_nextPunchTime, 0);

    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!_handStateLinker.IsPunchPose || _nextPunchTime != 0)
            return;

        Punchable punchable;
        if (collision.transform.CanGetComponent(out punchable))
        {
            Vector3 direction = AverageVelocity() * 100;
            if (punchable.IsRightDirection(direction) && GetMagnitude() >= punchable.punchForce) //TODO: Direction
            {
                Debug.Log("Punch!!");
                punchable.Punch();
                OnPunchEvent?.Invoke(this, punchable);
                _nextPunchTime = 30;
            }
        }
    }


    float GetMagnitude()
    {
        Vector3 velocity = Vector3.zero;
        for (int i = 0; i < _velocityOverTime.Length; i++)
        {
            velocity += _velocityOverTime[i];
        }

        return (velocity.magnitude / _velocityOverTime.Length) * forceMulti * 10;
    }

    private Vector3 AverageVelocity()
    {
        Vector3 velocity = Vector3.zero;
        for (int i = 0; i < _velocityOverTime.Length; i++)
        {
            velocity += _velocityOverTime[i];
        }

        return velocity / _velocityOverTime.Length;
    }
}