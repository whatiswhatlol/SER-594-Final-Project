using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public List<WeaponBase> weapons = new List<WeaponBase>();
    public int currentWeaponIndex = 0;
    public PlayerInput input;
    private InputAction fireAction;

    private void Awake()
    {
        fireAction = input.actions["Attack"];

    }
    void Start()
    {
        weapons.AddRange(GetComponentsInChildren<WeaponBase>(true));
        SelectWeapon(0);

    }
    void OnEnable()
    {
        fireAction.performed += _ => Fire();
    }

    void OnDisable()
    {
        fireAction.performed -= _ => Fire();
    }
    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;

        for (int i = 0; i < weapons.Count; i++)
            weapons[i].gameObject.SetActive(i == index);

        currentWeaponIndex = index;
        Debug.Log($"Selected weapon: {weapons[index].weaponName}");
    }

    public int GetWeaponIndexByAngle(float angle)
    {
        if (weapons.Count == 0) return 0;
        float slice = 360f / weapons.Count;
        int index = Mathf.FloorToInt((angle + 360f) % 360f / slice);
        return index;
    }

    public WeaponBase getWeaponByIndex(int index)
    {
        return weapons[index];
    }
    private void Fire()
    {
        if (Time.timeScale > 0.5f) // Don't fire during wheel
        {
            Debug.Log(weapons[currentWeaponIndex].name + " is supposed to shoot");

            weapons[currentWeaponIndex].Fire();
        }
    }
}
