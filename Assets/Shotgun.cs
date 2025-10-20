using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Shotgun: WeaponBase
{
    [Header("Launch Settings")]
    public float launchForce = 15f;
    public float upwardBoost = 3f;
    public float cooldownTime = 1f;
    public AudioClip fireSound;

    private float nextFireTime;
    private Rigidbody2D playerRb;
    private AudioSource audioSource;

    void Start()
    {
        weaponName = "Shotgun Launcher";
        playerRb = GetComponentInParent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Fire()
    {
        if (Time.time < nextFireTime) return; // cooldown
        if (playerRb == null) return;

        // Get mouse direction
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDir = (mouseWorld - playerRb.transform.position);
        aimDir.Normalize();

        // Apply impulse in opposite direction
        Vector2 launchDir = -aimDir;
        launchDir.y += upwardBoost * 0.1f; // optional slight upward lift
        playerRb.linearVelocity = Vector2.zero; // reset before launch for consistency
        playerRb.AddForce(launchDir.normalized * launchForce, ForceMode2D.Impulse);

        // Optional: recoil sound or screen shake
        if (fireSound)
            audioSource.PlayOneShot(fireSound);

        nextFireTime = Time.time + cooldownTime;
    }
}
