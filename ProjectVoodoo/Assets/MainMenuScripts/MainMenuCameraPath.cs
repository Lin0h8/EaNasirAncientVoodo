using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class MainMenuCameraPath : MonoBehaviour
{
    public MazeGenerator maze;
    public float height = 1.6f;
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 2.5f;
    public float lookAheadDistance = 2f;
    public bool loop = true;

    private IReadOnlyList<Vector3> _points;
    private int _segIndex = 0;
    private float _t = 0f;

    private void Start()
    {
        if (maze == null) maze = FindFirstObjectByType<MazeGenerator>();
        if (maze == null) return;
        _points = maze.GetPathPoints();
        if (_points == null || _points.Count < 2) return;
        var pos = _points[0] + Vector3.up * height;
        transform.position = pos;
        if (_points.Count >= 2)
        {
            transform.rotation = Quaternion.LookRotation((_points[1] - _points[0]).normalized, Vector3.up);
        }
    }

    private void Update()
    {
        if (_points == null || _points.Count < 2) return;

        var a = _points[_segIndex % _points.Count];
        var b = _points[(_segIndex + 1) % _points.Count];

        float dist = Vector3.Distance(a, b);
        float segSpeed = Mathf.Max(0.01f, moveSpeed);
        _t += (Time.deltaTime * segSpeed) / Mathf.Max(0.001f, dist);

        if (_t >= 1f)
        {
            _t = 0f;
            _segIndex++;
            if (!loop && _segIndex >= _points.Count - 1)
            {
                _segIndex = _points.Count - 2;
                loop = true;
            }
        }

        var pos = Vector3.Lerp(a, b, _t) + Vector3.up * height;

        float smoothSpeed = 5f;
        float lerpFactor = Mathf.Clamp01(smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, pos, lerpFactor);

        var lookTarget = Vector3.Lerp(a, b, Mathf.Clamp01(_t + (lookAheadDistance / Mathf.Max(0.01f, dist))));
        var dir = (lookTarget - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            var targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }
}