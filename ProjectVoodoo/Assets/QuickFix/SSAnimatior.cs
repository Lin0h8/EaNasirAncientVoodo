using UnityEngine;
using UnityEngine.UI;

public class SSAnimator : MonoBehaviour
{
    [Header("Frames")]
    public Sprite[] frames;

    [Header("Playback")]
    public float framesPerSecond = 8f;

    public bool loop = true;
    public bool playOnAwake = true;
    public bool startPaused = false;

    [Header("Input Control")]
    public bool pauseWhenNoInput = false;

    private bool flipXWithDirection = true;

    private SpriteRenderer _sr;
    private Image _image;
    private bool _isUIImage;
    private Sprite[] _runtimeFrames;
    private int _frameCount;
    private float _time;
    private int _currentFrame = 0;
    private bool _playing = true;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _sr = GetComponent<SpriteRenderer>();
        _isUIImage = _image != null;

        if (!_isUIImage && _sr == null)
        {
            Debug.LogError("SSAnimator requires either a SpriteRenderer or UI Image component!");
            enabled = false;
            return;
        }

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

        if (pauseWhenNoInput && !HasKeyboardInput())
        {
            return;
        }

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

    private bool HasKeyboardInput()
    {
        return Input.anyKey;
    }

    private void ApplyFrameInstant()
    {
        if (_frameCount == 0) return;
        var sp = _runtimeFrames[Mathf.Clamp(_currentFrame, 0, _frameCount - 1)];
        if (sp != null)
        {
            if (_isUIImage)
                _image.sprite = sp;
            else
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

        if (_isUIImage)
        {
            if (_image == null) _image = GetComponent<Image>();
            var scale = _image.transform.localScale;
            if (velocity.x > 0.01f) scale.x = Mathf.Abs(scale.x);
            else if (velocity.x < -0.01f) scale.x = -Mathf.Abs(scale.x);
            _image.transform.localScale = scale;
        }
        else
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (velocity.x > 0.01f) _sr.flipX = false;
            else if (velocity.x < -0.01f) _sr.flipX = true;
        }
    }
}