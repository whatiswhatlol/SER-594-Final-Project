using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class WeaponWheelController : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup wheelUI;
    public Image[] Select_Weapons;
    public WeaponManager weaponManager;

    [Header("Settings")]
    public float slowTimeScale = 0.2f;
    public float fadeDuration = 0.2f;
    public float unselectedAlpha = 0f;
    public float selectedAlpha = 0.95f;

    [Tooltip("Rotate the wheel mapping if your slice 0 should be up instead of right, etc.")]
    public float angleOffsetDeg = 0f;

    [Tooltip("Flip angle direction to match clockwise UI layouts.")]
    public bool invertClockwise = true;

    public PlayerInput playerInput;

    private bool isWheelOpen = false;
    private float originalTimeScale = 1f;

    private InputAction wheelAction;
    private InputAction pointAction;

    private int currentIndex = 0;
    private int lastHighlightedIndex = -1;

    public RectTransform wheelRect;
    public Canvas rootCanvas;

    void Awake()
    {
        if (playerInput == null) playerInput = GetComponentInParent<PlayerInput>();
        wheelAction = playerInput != null ? playerInput.actions["OpenWeaponWheel"] : null;
        pointAction = playerInput != null ? playerInput.actions["Look"] : null;
    }

    void OnEnable()
    {
        if (wheelAction != null)
        {
            wheelAction.Enable();
            wheelAction.performed += OnWheelPerformed;
            wheelAction.canceled += OnWheelCanceled;
        }
        if (pointAction != null) pointAction.Enable();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (wheelAction != null)
        {
            wheelAction.performed -= OnWheelPerformed;
            wheelAction.canceled -= OnWheelCanceled;
            wheelAction.Disable();
        }
        if (pointAction != null) pointAction.Disable();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (wheelAction != null)
        {
            wheelAction.performed -= OnWheelPerformed;
            wheelAction.canceled -= OnWheelCanceled;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // Safety reset so we don't leave the game paused if a scene changes while the wheel is open
        if (isWheelOpen)
        {
            isWheelOpen = false;
            Time.timeScale = originalTimeScale;
        }

        // If your UI is scene-local, reassign these as needed (optional)
        // wheelUI = FindObjectOfType<CanvasGroup>(); etc.
    }

    private void OnWheelPerformed(InputAction.CallbackContext _) => OpenWheel();
    private void OnWheelCanceled(InputAction.CallbackContext _) => CloseWheel();

    void Update()
    {
        if (!isWheelOpen) return;

        float angleDeg = ReadAimAngleDeg();
        if (invertClockwise) angleDeg = Mathf.Repeat(360f - angleDeg, 360f);
        angleDeg = Mathf.Repeat(angleDeg + angleOffsetDeg, 360f);

        int count = weaponManager != null ? weaponManager.weapons.Count : 0;
        if (count <= 0) return;

        int index = Mathf.FloorToInt(angleDeg / (360f / count));
        index = Mathf.Clamp(index, 0, count - 1);

        if (index != lastHighlightedIndex)
        {
            currentIndex = index;
            HighlightWeapon(currentIndex, count);
            lastHighlightedIndex = currentIndex;
        }
    }

    private float ReadAimAngleDeg()
    {
        if (playerInput == null || pointAction == null) return 0f;

        string scheme = playerInput.currentControlScheme ?? "";
        if (scheme.Contains("Gamepad"))
        {
            Vector2 stick = pointAction.ReadValue<Vector2>();
            if (stick.sqrMagnitude < 0.0001f) return 0f;
            return Mathf.Repeat(Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg + 360f, 360f);
        }

        Vector2 screenMouse = (Mouse.current != null)
            ? Mouse.current.position.ReadValue()
            : pointAction.ReadValue<Vector2>();

        Vector2 center = GetWheelCenterScreenPosition();
        Vector2 delta = screenMouse - center;
        if (delta.sqrMagnitude < 0.0001f) return 0f;

        return Mathf.Repeat(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg + 360f, 360f);
    }

    private Vector2 GetWheelCenterScreenPosition()
    {
        if (wheelRect == null || rootCanvas == null)
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Camera cam = null;
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = rootCanvas.worldCamera;

        return RectTransformUtility.WorldToScreenPoint(cam, wheelRect.position);
    }

    private void HighlightWeapon(int weaponIndex, int weaponCount)
    {
        if (Select_Weapons == null || Select_Weapons.Length == 0) return;
        int hi = Mathf.Clamp(weaponIndex, 0, Select_Weapons.Length - 1);

        for (int i = 0; i < Select_Weapons.Length; i++)
        {
            var img = Select_Weapons[i];
            if (img != null)
                img.DOFade(unselectedAlpha, 0.12f).SetEase(Ease.OutSine).SetUpdate(true);
        }

        var sel = Select_Weapons[hi];
        if (sel != null)
        {
            sel.DOFade(selectedAlpha, 0.18f).SetEase(Ease.InSine).SetUpdate(true);
        }
    }

    private void OpenWheel()
    {
        if (isWheelOpen) return;
        if (wheelUI == null) return;

        isWheelOpen = true;

        originalTimeScale = Time.timeScale;
        Time.timeScale = slowTimeScale;

        wheelUI.gameObject.SetActive(true);
        wheelUI.alpha = 0f;
        lastHighlightedIndex = -1;

        wheelUI.DOFade(selectedAlpha, fadeDuration).SetUpdate(true);
    }

    private void CloseWheel()
    {
        if (!isWheelOpen) return;
        isWheelOpen = false;

        if (weaponManager != null) weaponManager.SelectWeapon(currentIndex);

        Time.timeScale = originalTimeScale;

        if (wheelUI != null)
        {
            wheelUI.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                if (Select_Weapons != null)
                {
                    for (int i = 0; i < Select_Weapons.Length; i++)
                    {
                        var img = Select_Weapons[i];
                        if (img != null)
                        {
                            var c = img.color; c.a = unselectedAlpha; img.color = c;
                        }
                    }
                }
                wheelUI.gameObject.SetActive(false);
            });
        }
    }
}
