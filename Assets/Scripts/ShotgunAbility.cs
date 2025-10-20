using System.Collections;
using UnityEngine;

public class ShotgunAbility : MonoBehaviour, IAbility
{
    public string abilityName = "Shotgun";
    public Rigidbody2D rb;
    public GroundCheck groundCheck;

    public Transform muzzlePoint;
    public GameObject muzzleVFXPrefab;

    public GameObject weaponRoot;

    public int maxShots = 2;
    public float reloadTime = 1.25f;

    private int shotsLeft;
    private bool reloading;

    public string AbilityName => abilityName;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!groundCheck) groundCheck = GetComponent<GroundCheck>();
        if (weaponRoot) weaponRoot.SetActive(false);
        shotsLeft = maxShots;
    }

    void Update()
    {
        if (groundCheck && groundCheck.IsGrounded())
        {
            if (shotsLeft < maxShots && !reloading)
                StartCoroutine(ReloadRoutine());
        }
    }

    public bool CanUse() => shotsLeft > 0;

    public void Use(Vector2 aimDir)
    {
        shotsLeft = Mathf.Max(0, shotsLeft - 1);

        if (muzzleVFXPrefab && muzzlePoint)
        {
            var rot = Quaternion.FromToRotation(Vector3.right, aimDir.normalized);
            var go = Instantiate(muzzleVFXPrefab, muzzlePoint.position, rot);

            float sign = (aimDir.x < 0f) ? -1f : 1f;
            var s = go.transform.localScale;
            s.x = Mathf.Abs(s.x) * sign;
            go.transform.localScale = s;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr) sr.flipX = (aimDir.x < 0f);
        }
    }

    IEnumerator ReloadRoutine()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        shotsLeft = maxShots;
        reloading = false;
    }

    public void OnAbilitySelected()
    {
        if (weaponRoot) weaponRoot.SetActive(true);
    }

    public void OnAbilityDeselected()
    {
        if (weaponRoot) weaponRoot.SetActive(false);
    }
}
