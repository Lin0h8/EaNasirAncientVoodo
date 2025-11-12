using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SSAnimator : MonoBehaviour
{
    [Header("Frames")]
    public Sprite[] frames;

    [Header("Playback")]
    public float framesPerSecond = 8f;

    public bool loop = true;
    public bool playOnAwake = true;
    public bool startPaused = false;

    private bool flipXWithDirection = true;

    private SpriteRenderer _sr;
    private Sprite[] _runtimeFrames;
    private int _frameCount;
    private float _time;
    private int _currentFrame = 0;
    private bool _playing = true;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        InitializeFrames();
        _playing = playOnAwake && !startPaused;
        if (!_playing) _time = 0f;
        ApplyFrameInstant();
    }

    private void InitializeFrames()
    {
        _runtimeFrames = frames != null ? frames : new Sprite[0];
        _frameCount = _runtimeFrames.Length;
    }

    private void Update()
    {
        if (!_playing || _frameCount == 0 || framesPerSecond <= 0f) return;

        _time += Time.deltaTime;
        float frameInterval = 1f / framesPerSecond;
        if (_time >= frameInterval)
        {
            int steps = Mathf.FloorToInt(_time / frameInterval);
            _time -= steps * frameInterval;
            _currentFrame += steps;
            if (loop)
            {
                _currentFrame %= _frameCount;
            }
            else
            {
                if (_currentFrame >= _frameCount)
                {
                    _currentFrame = _frameCount - 1;
                    Pause();
                }
            }
            ApplyFrameInstant();
        }
    }

    private void ApplyFrameInstant()
    {
        if (_frameCount == 0) return;
        var sp = _runtimeFrames[Mathf.Clamp(_currentFrame, 0, _frameCount - 1)];
        if (sp != null)
        {
            _sr.sprite = sp;
        }
    }

    public void Play()
    {
        if (_frameCount == 0) return;
        _playing = true;
    }

    public void Pause()
    {
        _playing = false;
    }

    public void Stop()
    {
        _playing = false;
        _currentFrame = 0;
        _time = 0f;
        ApplyFrameInstant();
    }

    public void SetFrame(int index)
    {
        if (_frameCount == 0) return;
        _currentFrame = Mathf.Clamp(index, 0, _frameCount - 1);
        ApplyFrameInstant();
    }

    public void SetFacingByVelocity(Vector3 velocity)
    {
        if (!flipXWithDirection) return;
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (velocity.x > 0.01f) _sr.flipX = false;
        else if (velocity.x < -0.01f) _sr.flipX = true;
    }
}