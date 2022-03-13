using System;
using UnityEngine;

public class Punchable : MonoBehaviour
{
    public ParticleSystem effect;
    public float punchForce = 1;
    private Vector3 _requiredDirection = Vector3.up;

    public Action OnPunchEvent;

    public void Punch()
    {
        if (effect)
        {
            ParticleSystem particles = Instantiate(effect, transform.position, transform.rotation);
            particles.Play();
        }

        OnPunchEvent?.Invoke();
    }

    public bool IsRightDirection(Vector3 direction)
    {
        return Vector3.Dot(direction, _requiredDirection) >= 1;
    }
}