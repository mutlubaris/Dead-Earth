using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundEmitter : MonoBehaviour
{
    // Inspector Assigned
    [SerializeField] private float _decayRate = 1f;

    // Internal
    private SphereCollider _collider = null;
    private float _srcRadius = 0f;
    private float _tgtRadius = 0f;
    private float _interpolator = 0f;
    private float _interpolatorSpeed = 0f;
    
    void Start()
    {
        // Cache Collider Reference
        _collider = GetComponent<SphereCollider>();
        if (!_collider) return;

        // Set Radius Values
        _srcRadius = _tgtRadius = _collider.radius;

        // Setup Interpolator
        _interpolator = 0f;
        if (_decayRate > 0.02f)
            _interpolatorSpeed = 1f / _decayRate;
        else
            _interpolatorSpeed = 0f;
        
    }

    private void FixedUpdate() 
    {
        if (!_collider) return;    

        _interpolator = Mathf.Clamp01(_interpolator + Time.deltaTime * _interpolatorSpeed);
        _collider.radius = Mathf.Lerp(_srcRadius, _tgtRadius, _interpolator);

        if (_collider.radius < Mathf.Epsilon) _collider.enabled = false;
        else _collider.enabled = true;
    }

    public void SetRadius(float newRadius, bool instantResize = false)
    {
        if (!_collider || newRadius == _tgtRadius) return;
        _srcRadius = (instantResize || newRadius > _collider.radius) ? newRadius : _collider.radius;
        _tgtRadius = newRadius;
        _interpolator = 0f;
    }
}
