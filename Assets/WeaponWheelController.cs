using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UI;

public class WeaponWheelController : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup wheelUI;
    public Image[] Select_Weapons;
    public WeaponManager weaponManager;

    [Header("Settings")]
    public float slowTimeScale = 0.2f;
    public float fadeDuration = 0.2f;

    private bool isWheelOpen = false;
    private float originalTimeScale = 1f;

    private PlayerInput playerInput;
    private InputAction wheelAction;
    private InputAction fireAction;
    private InputAction pointAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        wheelAction = playerInput.actions["Wheel"];   
        fireAction = playerInput.actions["Fire"];    
        pointAction = playerInput.actions["Point"];   
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
        HighlightWeapon(index);
        if (fireAction.WasPerformedThisFrame())
            weaponManager.SelectWeapon(index);
    }

    private void HighlightWeapon(int index)
    {
        foreach (Image img in Select_Weapons)
        {
            img.DOFade(0, 0.15f).SetEase(Ease.OutSine);
        }
        Select_Weapons[index].DOFade(0.95f,0.2f).SetEase(Ease.InSine);
    }

    private void OpenWheel()
    {
        if (isWheelOpen) return;
        isWheelOpen = true;

        originalTimeScale = Time.timeScale;
        Time.timeScale = slowTimeScale;

        wheelUI.gameObject.SetActive(true);
        wheelUI.alpha = 0f;

        wheelUI.DOFade(0.95f, fadeDuration).SetUpdate(true);
    }

    private void CloseWheel()
    {
        if (!isWheelOpen) return;
        isWheelOpen = false;

        Time.timeScale = originalTimeScale;

        wheelUI.DOFade(0f, fadeDuration)
               .SetUpdate(true)
               .OnComplete(() => wheelUI.gameObject.SetActive(false));
    }
}
