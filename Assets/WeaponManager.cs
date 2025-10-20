using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    public List<WeaponBase> weapons = new List<WeaponBase>();
    public int currentWeaponIndex = 0;

    void Start()
    {
        weapons.AddRange(GetComponentsInChildren<WeaponBase>(true));
        SelectWeapon(0);
    }

    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;

        for (int i = 0; i < weapons.Count; i++)
            weapons[i].gameObject.SetActive(i == index);

        currentWeaponIndex = index;
        Debug.Log($"Selected weapon: {weapons[index].weaponName}");
    }

    public void HighlightWeapon(int index)
    {
        // Optional: update UI highlight state
    }

    public int GetWeaponIndexByAngle(float angle)
    {
        if (weapons.Count == 0) return 0;
        float slice = 360f / weapons.Count;
        int index = Mathf.FloorToInt((angle + 360f) % 360f / slice);
        return index;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.timeScale > 0.5f) // Don't fire during wheel
        {
            weapons[currentWeaponIndex].Fire();
        }
    }
}
