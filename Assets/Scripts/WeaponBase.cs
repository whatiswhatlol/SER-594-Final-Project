using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public string weaponName = "Default Weapon";

    public virtual void Fire()
    {
        Debug.Log($"{weaponName} fired!");
    }
}
