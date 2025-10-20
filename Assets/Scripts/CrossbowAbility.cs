using System.Collections;
using UnityEngine;

public class CrossbowAbility : MonoBehaviour, IAbility
{
    public string abilityName = "Crossbow";
    public Rigidbody2D rb;
    public GameObject arrowPrefab;
    public Transform shootPoint;
    public GameObject weaponRoot;
    public float shootSpeed = 12f;
    public float cooldown = 0.35f;
    public float muzzleOffset = 0.2f;

    private bool onCooldown;

    public string AbilityName => abilityName;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (weaponRoot) weaponRoot.SetActive(false);
    }

    public bool CanUse()
    {
        return !onCooldown && arrowPrefab && shootPoint;
    }

    public void Use(Vector2 aimDir)
    {
        if (!CanUse()) return;

        float sign = Mathf.Sign(transform.localScale.x == 0 ? 1f : transform.localScale.x);
        Vector2 dir = new Vector2(sign, 0f).normalized;

        Vector3 spawnPos = shootPoint.position + (Vector3)(dir * muzzleOffset);
        Quaternion rot = Quaternion.FromToRotation(Vector3.right, new Vector3(dir.x, dir.y, 0f));

        var go = Object.Instantiate(arrowPrefab, spawnPos, rot);
        var prb = go.GetComponent<Rigidbody2D>();
        if (prb) prb.linearVelocity = dir * shootSpeed;

        var arrowCol = go.GetComponent<Collider2D>();
        var playerCols = GetComponentsInChildren<Collider2D>();
        if (arrowCol != null)
        {
            for (int i = 0; i < playerCols.Length; i++)
                Physics2D.IgnoreCollision(arrowCol, playerCols[i], true);
        }

        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
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
