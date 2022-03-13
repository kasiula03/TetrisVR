using UnityEngine;


public class BoardScaller : MonoBehaviour
{
    [SerializeField] private Transform _objectToScale;
    [SerializeField] [Range(0.5f, 3f)] private float _minScaleRange;
    [SerializeField] [Range(0.5f, 3f)] private float _maxScaleRange;
    [SerializeField] private float _step;
    
    private float _currentScaleChange;
    
    public void IncreaseScale()
    {
        ChangeScale(_step);
    }

    public void DecreaseScale()
    {
        ChangeScale(-_step);
    }

    private void ChangeScale(float value)
    {
        _currentScaleChange = Mathf.Clamp(_currentScaleChange + value, _minScaleRange, _maxScaleRange);
        _objectToScale.localScale = Vector3.one * _currentScaleChange;
        
    }
}