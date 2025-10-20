using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // ★ New Input System

[RequireComponent(typeof(LineRenderer))]
public class GrappleAbility : MonoBehaviour, IAbility
{
    public string abilityName = "Grapple";
    public Rigidbody2D rb;
    public LayerMask grappleMask;
    public float maxDistance = 18f;
    public float cooldown = 1.2f;

    private DistanceJoint2D joint;
    private LineRenderer rope;
    private Vector2 anchor;
    private bool attached;
    private bool onCooldown;

    public string AbilityName => abilityName;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        rope = GetComponent<LineRenderer>();
        rope.positionCount = 0;
    }

    void Update()
    {
        if (!attached) return;

        rope.SetPosition(0, transform.position);
        rope.SetPosition(1, anchor);

        // --- New Input System polling ---
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame || (kb != null && kb.rKey.wasPressedThisFrame))
        {
            Detach();
            StartCoroutine(CooldownRoutine());
        }
    }

    public bool CanUse() => !attached && !onCooldown;

    public void Use(Vector2 aimDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDir, maxDistance, grappleMask);
        if (!hit) return;

        anchor = hit.point;
        Attach(anchor);
    }

    private void Attach(Vector2 point)
    {
        if (joint == null) joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.autoConfigureDistance = false;
        joint.connectedAnchor = point;
        float dist = Vector2.Distance(transform.position, point);
        joint.distance = Mathf.Max(0.5f, dist * 0.9f);
        joint.enableCollision = true;
        joint.enabled = true;

        attached = true;
        rope.positionCount = 2;
        rope.SetPosition(0, transform.position);
        rope.SetPosition(1, point);
    }

    private void Detach()
    {
        attached = false;
        if (joint) joint.enabled = false;
        rope.positionCount = 0;
    }

    private IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    public void OnAbilitySelected() { }
    public void OnAbilityDeselected() { if (attached) Detach(); }
}
