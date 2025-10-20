using UnityEngine;
using UnityEngine.InputSystem;

public interface IAbility
{
    string AbilityName { get; }
    bool CanUse();
    void Use(Vector2 aimDir);
    void OnAbilitySelected();
    void OnAbilityDeselected();
}

[RequireComponent(typeof(Rigidbody2D))]
public class AbilityManager : MonoBehaviour
{
    public Camera mainCamera;
    public Rigidbody2D rb;
    public GroundCheck groundCheck;
    public ShotgunAbility shotgun;
    public GrappleAbility grapple;
    public CrossbowAbility crossbow;
    public bool useMouseAim = false;

    private IAbility[] _abilities;
    private int _currentIndex = -1;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!mainCamera) mainCamera = Camera.main;
        if (!groundCheck) groundCheck = GetComponent<GroundCheck>();
        if (!shotgun) shotgun = GetComponent<ShotgunAbility>();
        if (!grapple) grapple = GetComponent<GrappleAbility>();
        if (!crossbow) crossbow = GetComponent<CrossbowAbility>();

        _abilities = new IAbility[3];
        _abilities[0] = shotgun;
        _abilities[1] = grapple;
        _abilities[2] = crossbow;

        for (int i = 0; i < _abilities.Length; i++)
            _abilities[i]?.OnAbilityDeselected();

        _currentIndex = -1;
        LogCurrent();
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        if (kb.digit1Key.wasPressedThisFrame) Select(0);
        if (kb.digit2Key.wasPressedThisFrame) Select(1);
        if (kb.digit3Key.wasPressedThisFrame) Select(2);
        if (kb.digit4Key.wasPressedThisFrame) DeselectAll();

        if (_currentIndex >= 0 && mouse.leftButton.wasPressedThisFrame)
            TryUseCurrent(mouse);
    }

    public void Select(int idx)
    {
        if (idx < 0 || idx >= _abilities.Length) return;
        if (_currentIndex == idx) return;

        if (_currentIndex >= 0)
            _abilities[_currentIndex]?.OnAbilityDeselected();

        _currentIndex = idx;
        _abilities[_currentIndex]?.OnAbilitySelected();
        LogCurrent();
    }

    public void DeselectAll()
    {
        if (_currentIndex >= 0)
            _abilities[_currentIndex]?.OnAbilityDeselected();

        _currentIndex = -1;
        LogCurrent();
    }

    private void TryUseCurrent(Mouse mouse)
    {
        var abil = _abilities[_currentIndex];
        if (abil == null || !abil.CanUse()) return;

        Vector2 aimDir;
        if (useMouseAim && mainCamera)
        {
            Vector2 mouseScreen = mouse.position.ReadValue();
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(mainCamera.transform.position.z)));
            aimDir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        }
        else
        {
            float s = Mathf.Sign(transform.localScale.x == 0 ? 1f : transform.localScale.x);
            aimDir = s >= 0 ? Vector2.right : Vector2.left;
        }

        abil.Use(aimDir);
    }

    public bool IsGrounded()
    {
        return groundCheck != null && groundCheck.IsGrounded();
    }

    private void LogCurrent()
    {
        string name = (_currentIndex >= 0 && _abilities[_currentIndex] != null) ? _abilities[_currentIndex].AbilityName : "(None)";
        Debug.Log($"[AbilityManager] Selected: {name}");
    }
}
