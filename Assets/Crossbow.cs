using UnityEngine;
using UnityEngine.InputSystem;

public class Crossbow : WeaponBase
{
    [Header("Refs")]
    public Rigidbody2D playerRb;         // used only to find position if needed
    public PlayerInput input;            // for "Look" action (mouse or right stick)
    public Camera cam;                   // if null, uses Camera.main

    [Header("Firing")]
    public Transform firePoint;          // where arrows spawn
    public Arrow arrowPrefab;            // prefab with Arrow script + Rigidbody2D + Collider2D
    public float launchSpeed = 18f;
    public float cooldownTime = 0.25f;

    [Header("Input")]
    public string lookActionName = "Look"; // Value/Vector2 (Pointer.position or rightStick)
    public float stickDeadzone = 0.15f;

    [Header("FX (optional)")]
    public AudioSource audioSource;
    public AudioClip shootSfx;

    private InputAction lookAction;
    private float nextFireTime;

    private void Awake()
    {
        if (input == null) input = GetComponentInParent<PlayerInput>();
        if (cam == null) cam = Camera.main;

        if (input != null && !string.IsNullOrEmpty(lookActionName))
        {
            lookAction = input.actions[lookActionName];
            lookAction.Enable();
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public override void Fire()
    {
        if (Time.time < nextFireTime || arrowPrefab == null || firePoint == null) return;

        Vector2 dir = GetAimDirection();
        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right;

        Arrow arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        arrow.Launch(dir.normalized * launchSpeed, owner: playerRb ? playerRb.GetComponent<Collider2D>() : null);

        if (shootSfx && audioSource) audioSource.PlayOneShot(shootSfx);
        nextFireTime = Time.time + cooldownTime;
    }

    private Vector2 GetAimDirection()
    {
        if (input == null || lookAction == null)
        {
            // fallback: face right
            return Vector2.right;
        }

        string scheme = input.currentControlScheme ?? "";
        if (scheme.Contains("Gamepad"))
        {
            Vector2 stick = lookAction.ReadValue<Vector2>();
            if (stick.magnitude >= stickDeadzone) return stick.normalized;
            return Vector2.right;
        }

        Vector2 screenPos;
        if (Mouse.current != null)
            screenPos = Mouse.current.position.ReadValue();
        else
            screenPos = lookAction.ReadValue<Vector2>();

        if (cam == null) cam = Camera.main;
        if (cam == null) return Vector2.right;

        float z = Mathf.Abs((playerRb ? playerRb.transform.position.z : 0f) - cam.transform.position.z);
        if (cam.orthographic) z = Mathf.Abs(cam.transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
        Vector2 from = (Vector2)(firePoint ? firePoint.position : transform.position);
        Vector2 dir = ((Vector2)world - from);
        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right;
        return dir.normalized;
    }
}
