using System;
using System.Collections.Generic;
using System.Linq;
using Autohand;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class Block : MonoBehaviour
{
    [SerializeField] private Grabbable _grabbable;
    [SerializeField] private HandTouchEvent _handTouchEvent;
    [SerializeField] private BlockEffect _blockEffect;
    [SerializeField] private Transform _centerOfBlock;
    [SerializeField] private Punchable _punchable;

    private Action<Block> _moveToBottom;
    private Action<Block, Vector2> _moveBlock;
    private Action<Block, int> _rotateBlock;
    
    private Action<Vector2> _onBlockMoved;

    private float _handCheckRadius = 0.15f;
    private BlockProperties _properties;
    private readonly Dictionary<Vector2, Transform> _segments = new Dictionary<Vector2, Transform>();
    private MeshRenderer[] _segmentsMeshes;

    private Hand _handReference;
    private Puncher _puncher;

    private Sequence _sequence;
    private const string GrabbableLayerNameDefault = "Grabbable";
    private const string GrabbingLayerName = "Grabbing";

    private float timeOfForceToRotate = 0.5f;
    private float _rotateLoadingProgress = 0;

    private Vector3 _applyingDirection = Vector3.zero;
    private float _angle;
    private bool _isActive;

    private Color _blockColor;

    private bool IsMovementPossible => _isActive && _handReference && !_puncher.InPunchMode;

    public Dictionary<Vector2, Transform> Segments => _segments;

    public void Initialize(BlockProperties properties, Action<Vector2> onBlockMoved, Action<Block> moveToBottom, Action<Block, Vector2> moveBlock, Action<Block, int> rotateBlock)
    {
        _properties = properties;
        _onBlockMoved = onBlockMoved;
        _moveToBottom = moveToBottom;
        _moveBlock = moveBlock;
        _rotateBlock = rotateBlock;
        
        Transform[] transformSegments = GetComponentsInChildren<Transform>().Where(seg => seg != transform).ToArray();
        foreach (Vector2 propertiesSegmentsCoord in _properties.SegmentsCoords)
        {
            Transform segment = transformSegments.First(seg =>
                Math.Abs(seg.localPosition.x - propertiesSegmentsCoord.x) < 0.001f &&
                Math.Abs(seg.localPosition.y - propertiesSegmentsCoord.y) < 0.001f);
            _segments[propertiesSegmentsCoord] = segment;
        }

        _segmentsMeshes = new MeshRenderer[_segments.Count];
        int index = 0;
        foreach (KeyValuePair<Vector2, Transform> seg in _segments)
        {
            _segmentsMeshes[index] = seg.Value.GetComponent<MeshRenderer>();
            index++;
        }

        _blockColor = _segmentsMeshes.First().material.color;

        _handTouchEvent.HandStartTouchEvent += OnTouch;
        _grabbable.OnGrabEvent += OnGrab;
        _grabbable.OnReleaseEvent += OnRelease;
        _punchable.OnPunchEvent += OnPunch;

        _isActive = true;
    }

    public void Release()
    {
        _sequence?.Complete(true);
        _grabbable.ForceHandsRelease();
        _isActive = false;
        foreach (Transform segmentsValue in _segments.Values)
        {
            Collider collider = segmentsValue.GetComponent<Collider>();
            collider.enabled = false;
        }
    }

    public BlockProperties GetPropertiesCopy()
    {
        return _properties.Copy();
    }

    public void OnPunch()
    {
        _moveToBottom?.Invoke(this);
    }

    private void OnGrab(Hand hand, Grabbable grabbable)
    {
        _handReference = hand;
        _puncher = hand.GetComponent<Puncher>();
    }
    
    private void OnRelease(Hand hand, Grabbable grabbable)
    {
        if (hand != _handReference)
            return;
        
        _blockEffect.SetDisplacementX(0);
        _blockEffect.SetDisplacementY(0);
    }

    private void OnTouchStop(Hand hand)
    {
        if (hand != _handReference)
            return;

        _handReference = null;
        _puncher = null;
        _rotateLoadingProgress = 0;
        _blockEffect.SetDisplacementY(0);
        _blockEffect.SetDisplacementX(0);
        ClearRotation();
    }

    private void OnTouch(Hand hand)
    {
        if (_handReference == null)
        {
            _handReference = hand;
            _puncher = hand.GetComponent<Puncher>();
        }
    }

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }
        if (IsMovementPossible)
        {
            if (_grabbable.IsHeld())
            {
                ProcessMovingDown();
            }
            else
            {
                ProcessRotation();
            }
        }
        
        Vector3 offset = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            offset = new Vector2(-1, 0);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            offset = new Vector2(1, 0);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            offset = new Vector2(0, -1);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            _sequence?.Kill(true);
            TryRotate(90);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _sequence?.Kill(true);
            TryRotate(-90);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _moveToBottom?.Invoke(this);
        }
        
        if (_isActive && offset != Vector3.zero)
        {
            TryMove(offset);
        }

    }

    private void FixedUpdate()
    {
        if (_handReference != null && !_grabbable.beingGrabbed)
        {
            Ray ray = new Ray(_centerOfBlock.position, Vector3.forward);
            RaycastHit[] casts = Physics.SphereCastAll(ray, _handCheckRadius);

            if (casts.All(rayCast => rayCast.transform.gameObject != _handReference.gameObject))
            {
                OnTouchStop(_handReference);
            }
        }
    }


    private void ProcessMovingDown()
    {
        ClearRotation();
        bool isMoving = _sequence?.IsPlaying() ?? false;
        if (isMoving)
        {
            return;
        }

        Vector3 direction = _handReference.moveTo.position - _handReference.transform.position;

        if (direction.y <= -0.01f || Math.Abs(direction.x) >= 0.01f)
        {
            float yMovePercent = direction.y > 0 ? 0f : (Math.Abs(direction.y) / 0.04f);
            float xMovePercent = direction.x / 0.04f;

            float absXMovePercent = Mathf.Abs(xMovePercent);

            if (yMovePercent > absXMovePercent)
            {
                _blockEffect.SetDisplacementX(0);
                _blockEffect.SetDisplacementY(yMovePercent * -1);
            }
            else if (absXMovePercent > yMovePercent)
            {
                _blockEffect.SetDisplacementX(xMovePercent);
                _blockEffect.SetDisplacementY(0);
            }
        }
        else
        {
            _blockEffect.SetDisplacementX(0);
            _blockEffect.SetDisplacementY(0);
        }

        Vector2 offset = Vector2.zero;

        if (direction.y <= -0.04f)
        {
             offset = new Vector2(0, -1);

        }
        else if (direction.x > 0.04f)
        {
             offset = new Vector2(1, 0);

        }
        else if (direction.x < -0.04f)
        {
             offset = new Vector2(-1, 0);
       
        }

        if (offset != Vector2.zero)
        {
            TryMoveBy(offset);
        }
    }

    private void TryMoveBy(Vector2 offset)
    {
        _moveBlock?.Invoke(this, offset);
    }

    private void ProcessRotation()
    {
        if (_grabbable.IsHeld())
        {
            ClearRotation();
            return;
        }

        //TODO: Cooldown before another rotation

        if (_handReference.HandClosestHit(out RaycastHit closestHit, out Grabbable grabbable, 0.1f,
            LayerMask.GetMask(GrabbableLayerNameDefault, GrabbingLayerName), _grabbable) != Vector3.zero)
        {
            if (_rotateLoadingProgress > timeOfForceToRotate)
            {
            
                TryRotate(_angle);
            }
            else
            {
                float newAngle = GetAngle(closestHit);
                ApplyRotationPressure(newAngle);
            }
        }
        else
        {
            ClearRotation();
        }
    }

    private void ApplyRotationPressure(float newAngle)
    {
        _rotateLoadingProgress += Time.deltaTime;

        Vector3 currentDirection =
            (_handReference.moveTo.transform.position - _handReference.transform.position)
            .normalized;
        
        if ((_angle < 0 && newAngle > 0) || (_angle > 0 && newAngle < 0))
        {
            ClearRotation();
            return;
        }

        _angle = newAngle;
        _applyingDirection = currentDirection;

        float rotationProgress = 0;
        if (newAngle > 0)
        {
            rotationProgress = (_rotateLoadingProgress / timeOfForceToRotate);
        }
        else if (newAngle < 0)
        {
            rotationProgress = -(_rotateLoadingProgress / timeOfForceToRotate);
        }

        float degreeExponentialProgress = 90 * Mathf.Pow(Mathf.Clamp(rotationProgress, -1, 1), 5);

        _blockEffect.SetRotation(degreeExponentialProgress);
    }

    private void TryRotate(float angle)
    {
        if (!_sequence?.IsPlaying() ?? true)
        {
            int angleToRotate = 0;
            if (angle > 0)
            {
                angleToRotate = 90;
            }
            else if (angle < 0)
            {
                angleToRotate = -90;
            }
            
            if (angleToRotate != 0)
            {
                _rotateBlock?.Invoke(this, angleToRotate);
            }
         
        }

        _rotateLoadingProgress = 0;
    }

    private void TryMove(Vector2 offset)
    {
        _moveBlock?.Invoke(this, offset);
    }

    private void ClearRotation()
    {
        _angle = 0;
        _rotateLoadingProgress = 0;
        _blockEffect.SetRotation(0);
    }

    private float GetAngle(RaycastHit closestHit)
    {
        Vector3 direction = _applyingDirection;
        direction.z = 0;
        Vector3 positionOnSegment = closestHit.point;
        positionOnSegment.z = 0;
        Vector3 origin = transform.position;
        origin.z = 0;

        Vector3 toSegmentDirection = positionOnSegment - origin;
        Vector3 resultVec = (toSegmentDirection + direction);

        float angle = Vector3.SignedAngle(toSegmentDirection, resultVec, Vector3.forward);
        return angle;
    }
    
    public void Rotate(float value)
    {
        _properties.AddRotation(value);
        _blockEffect.SetRotation(0);

        _sequence?.Kill(true);
        _sequence = DOTween.Sequence();
        _sequence.Append(transform.DOLocalRotateQuaternion(Quaternion.Euler(0, 0, _properties.GetRotation()), 0.3f));
    }
    
    public void MoveTo(Vector3 coords, Vector2 offset, bool withAnimation)
    {
        _sequence?.Kill(true);
        if (withAnimation)
        {
            _sequence = DOTween.Sequence();
            _sequence.Append(transform.DOMove(coords, 0.3f));
        }
        else
        {
            transform.position = coords;
        }

        _blockEffect.SetDisplacementY(0);
        _blockEffect.SetDisplacementX(0);
        _onBlockMoved?.Invoke(offset);
    }

    #region PropertyAccessor

    public Vector2 GetRotatedSegment(Vector2 segment) => _properties.GetRotatedSegment(segment);

    public List<Vector2> GetSegments() => _properties.GetSegments();

    public List<Vector2> GetSegments(BlockProperties properties) => _properties.GetSegments(properties);

    public float GetMinX(float originX) => _properties.GetMinX(originX);

    public float GetPredictMinX(float originX, BlockProperties predictedProperties) =>
        _properties.GetPredictMinX(originX, predictedProperties);

    public float GetMaxX(float originX) => _properties.GetMaxX(originX);

    public float GetPredictMaxX(float originX, BlockProperties predictedProperties) =>
        _properties.GetPredictMaxX(originX, predictedProperties);

    public float GetMinY(float originY) => _properties.GetMinY(originY);

    public float GetPredictMinY(float originY, BlockProperties predictedProperties) =>
        _properties.GetPredictMinY(originY, predictedProperties);

    public float GetWidth() => _properties.GetWidth();

    public Color GetColor()
    {
        return _blockColor;
    }

    #endregion
}