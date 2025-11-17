using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

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

    [Header("Input")]
    public PlayerInput playerInput;

    private bool isWheelOpen = false;
    private float originalTimeScale = 1f;

    private InputAction wheelAction;
    private InputAction pointAction;

    private int currentIndex = 0;
    private int lastHighlightedIndex = -1;

    private RectTransform wheelRect;
    private Canvas rootCanvas;

    void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput != null)
        {
            wheelAction = playerInput.actions["OpenWeaponWheel"];
            pointAction = playerInput.actions["Look"];
        }

        if (wheelUI != null)
        {
            wheelRect = wheelUI.GetComponent<RectTransform>();
            rootCanvas = wheelUI.GetComponentInParent<Canvas>();
        }
    }

    void OnEnable()
    {
        // Rebind in case refs were lost on reload
        if (playerInput == null)
            playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput != null && (wheelAction == null || pointAction == null))
        {
            wheelAction = playerInput.actions["OpenWeaponWheel"];
            pointAction = playerInput.actions["Look"];
        }

        if (wheelAction != null)
        {
            wheelAction.Enable();
            wheelAction.performed += OnWheelPerformed;
            wheelAction.canceled += OnWheelCanceled;
        }

        if (pointAction != null)
            pointAction.Enable();
    }

    void OnDisable()
    {
        // Close safely if we got disabled mid-wheel
        if (isWheelOpen)
        {
            isWheelOpen = false;
            Time.timeScale = originalTimeScale;
        }

        // Kill active tweens so they don't try to run on a destroyed object
        if (wheelUI != null)
            wheelUI.DOKill();

        if (Select_Weapons != null)
        {
            foreach (var img in Select_Weapons)
                if (img != null) img.DOKill();
        }

        if (wheelAction != null)
        {
            wheelAction.performed -= OnWheelPerformed;
            wheelAction.canceled -= OnWheelCanceled;
            wheelAction.Disable();
        }

        if (pointAction != null)
            pointAction.Disable();
    }

    void OnDestroy()
    {
        // Extra safety – in case OnDisable wasn't called
        if (isWheelOpen)
        {
            isWheelOpen = false;
            Time.timeScale = originalTimeScale;
        }

        if (wheelAction != null)
        {
            wheelAction.performed -= OnWheelPerformed;
            wheelAction.canceled -= OnWheelCanceled;
        }
    }

    private void OnWheelPerformed(InputAction.CallbackContext ctx)
    {
        OpenWheel();
    }

    private void OnWheelCanceled(InputAction.CallbackContext ctx)
    {
        CloseWheel();
    }

    void Update()
    {
        if (!isWheelOpen) return;
        if (weaponManager == null) return;

        float angleDeg = ReadAimAngleDeg(); // [0,360)

        if (invertClockwise)
            angleDeg = Mathf.Repeat(360f - angleDeg, 360f);

        angleDeg = Mathf.Repeat(angleDeg + angleOffsetDeg, 360f);

        int weaponCount = weaponManager.weapons != null ? weaponManager.weapons.Count : 0;
        if (weaponCount <= 0) return;

        float slice = 360f / weaponCount;
        int index = Mathf.FloorToInt(angleDeg / slice);
        index = Mathf.Clamp(index, 0, weaponCount - 1);

        if (index != lastHighlightedIndex)
        {
            currentIndex = index;
            HighlightWeapon(currentIndex, weaponCount);
            lastHighlightedIndex = currentIndex;
        }
    }

    private float ReadAimAngleDeg()
    {
        if (playerInput == null || pointAction == null)
            return 0f;

        string scheme = playerInput.currentControlScheme ?? "";
        if (scheme.Contains("Gamepad"))
        {
            Vector2 stick = pointAction.ReadValue<Vector2>();
            if (stick.sqrMagnitude < 0.0001f) return 0f;

            float a = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg;
            return Mathf.Repeat(a + 360f, 360f);
        }

        Vector2 screenMouse = (Mouse.current != null)
            ? Mouse.current.position.ReadValue()
            : pointAction.ReadValue<Vector2>();

        Vector2 center = GetWheelCenterScreenPosition();
        Vector2 delta = screenMouse - center;
        if (delta.sqrMagnitude < 0.0001f) return 0f;

        float angleDeg = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        return Mathf.Repeat(angleDeg + 360f, 360f);
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
            if (img == null) continue;

            img.DOFade(unselectedAlpha, 0.12f)
               .SetEase(Ease.OutSine)
               .SetUpdate(true);
        }

        var sel = Select_Weapons[hi];
        if (sel != null)
        {
            sel.DOFade(selectedAlpha, 0.18f)
               .SetEase(Ease.InSine)
               .SetUpdate(true);
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

        wheelUI.DOFade(selectedAlpha, fadeDuration)
               .SetUpdate(true);
    }

    private void CloseWheel()
    {
        if (!isWheelOpen) return;
        isWheelOpen = false;

        if (weaponManager != null)
            weaponManager.SelectWeapon(currentIndex);

        Time.timeScale = originalTimeScale;

        if (wheelUI == null) return;

        wheelUI.DOFade(0f, fadeDuration)
               .SetUpdate(true)
               .OnComplete(() =>
               {
                   if (Select_Weapons != null)
                   {
                       for (int i = 0; i < Select_Weapons.Length; i++)
                       {
                           var img = Select_Weapons[i];
                           if (img != null)
                           {
                               var c = img.color;
                               c.a = unselectedAlpha;
                               img.color = c;
                           }
                       }
                   }
                   wheelUI.gameObject.SetActive(false);
               });
    }
}
