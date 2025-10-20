using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class WeaponWheelController : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup wheelUI;          // Add a CanvasGroup to your UI panel
    public WeaponManager weaponManager;

    [Header("Settings")]
    public float slowTimeScale = 0.2f;
    public float fadeDuration = 0.2f;

    private bool isWheelOpen = false;
    private float originalTimeScale = 1f;

    // Input actions
    private PlayerInput playerInput;
    private InputAction wheelAction;
    private InputAction fireAction;
    private InputAction pointAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Input Actions: must exist in your Input Actions asset
        wheelAction = playerInput.actions["Wheel"];   // e.g. Tab key
        fireAction = playerInput.actions["Fire"];     // e.g. Left Mouse Button
        pointAction = playerInput.actions["Point"];   // e.g. Mouse Position
    }

    void OnEnable()
    {
        wheelAction.performed += _ => OpenWheel();
        wheelAction.canceled += _ => CloseWheel();
    }

    void OnDisable()
    {
        wheelAction.performed -= _ => OpenWheel();
        wheelAction.canceled -= _ => CloseWheel();
    }

    void Update()
    {
        if (!isWheelOpen) return;

        // Read pointer position
        Vector2 mousePos = pointAction.ReadValue<Vector2>();
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        float angle = Mathf.Atan2(mousePos.y - center.y, mousePos.x - center.x) * Mathf.Rad2Deg;

        int index = weaponManager.GetWeaponIndexByAngle(angle);
        weaponManager.HighlightWeapon(index);

        if (fireAction.WasPerformedThisFrame())
            weaponManager.SelectWeapon(index);
    }

    private void OpenWheel()
    {
        if (isWheelOpen) return;
        isWheelOpen = true;

        originalTimeScale = Time.timeScale;
        Time.timeScale = slowTimeScale;

        wheelUI.gameObject.SetActive(true);
        wheelUI.alpha = 0f;

        // Fade-in UI (independent of timescale)
        wheelUI.DOFade(1f, fadeDuration).SetUpdate(true);
    }

    private void CloseWheel()
    {
        if (!isWheelOpen) return;
        isWheelOpen = false;

        Time.timeScale = originalTimeScale;

        // Fade-out UI and disable when done
        wheelUI.DOFade(0f, fadeDuration)
               .SetUpdate(true)
               .OnComplete(() => wheelUI.gameObject.SetActive(false));
    }
}
