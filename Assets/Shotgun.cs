using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Shotgun : WeaponBase
{
    [Header("Launch Settings")]
    public float launchForce = 15f;
    public float upwardBoost = 0f;
    public float cooldownTime = 1f;              // normal time between shots
    public float reloadTime = 3f;                // long cooldown every 2nd shot

    [Header("Refs")]
    public AudioClip fireSound;
    public Rigidbody2D playerRb;
    public PlayerInput input;
    public Camera cam;
    public Image radialAnim;
    [Header("Input")]
    public string lookActionName = "Look";
    public float stickDeadzone = 0.15f;

    private InputAction lookAction;
    private float nextFireTime;
    private AudioSource audioSource;
    private Vector2 lastNonZeroAim = Vector2.right;

    private int shotCount = 0;  

    private void Awake()
    {
        if (input == null) input = GetComponent<PlayerInput>();
        if (input != null && !string.IsNullOrEmpty(lookActionName))
        {
            lookAction = input.actions[lookActionName];
            lookAction.Enable();
        }

        if (cam == null) cam = Camera.main;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        weaponName = "Shotgun";
    }

    public override void Fire()
    {
        if (Time.time < nextFireTime || playerRb == null || lookAction == null)
            return;

        shotCount++; 

        Vector2 aimDir = GetAimDirection();
        if (aimDir.sqrMagnitude < 1e-6f)
            aimDir = lastNonZeroAim;

        lastNonZeroAim = aimDir;

        Vector2 impulse = (-aimDir.normalized * launchForce) + (Vector2.up * upwardBoost);

        playerRb.linearVelocity = Vector2.zero;
        playerRb.AddForce(impulse, ForceMode2D.Impulse);

        if (fireSound)
            audioSource.PlayOneShot(fireSound);

        bool isReloadShot = (shotCount % 2 == 0);
        if ( isReloadShot)
        {
            radialAnim.DOFillAmount(1, reloadTime)
                .OnComplete(() => radialAnim.fillAmount = 0f);
        }
        nextFireTime = Time.time + (isReloadShot ? reloadTime : cooldownTime);
    }

    public void reloadAnim()
    {

    }

    private Vector2 GetAimDirection()
    {
        if (input == null) return lastNonZeroAim;

        var scheme = input.currentControlScheme ?? "";
        if (scheme.Contains("Gamepad"))
        {
            Vector2 stick = lookAction.ReadValue<Vector2>();
            if (stick.magnitude >= stickDeadzone)
                return stick.normalized;
            return Vector2.zero;
        }

        Vector2 screenPos = (Mouse.current != null)
            ? Mouse.current.position.ReadValue()
            : lookAction.ReadValue<Vector2>();

        if (cam == null) cam = Camera.main;
        if (cam == null) return Vector2.zero;

        float z = Mathf.Abs(playerRb.transform.position.z - cam.transform.position.z);
        if (cam.orthographic) z = Mathf.Abs(cam.transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
        Vector2 dir = (Vector2)world - playerRb.position;

        if (dir.sqrMagnitude < 1e-6f) return Vector2.zero;
        return dir.normalized;
    }
}
