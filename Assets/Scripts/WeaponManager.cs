using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    public List<WeaponBase> weapons = new List<WeaponBase>();
    public int currentWeaponIndex = 0;
    public PlayerInput input;

    private InputAction fireAction;

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInput>();
        fireAction = input != null ? input.actions["Attack"] : null;
        RebuildWeapons();
    }

    void OnEnable()
    {
        if (fireAction != null)
        {
            fireAction.Enable();
            fireAction.performed += OnFirePerformed;   // named handler
        }
        SceneManager.sceneLoaded += OnSceneLoaded;      // rebuild list if this object persists
    }

    void OnDisable()
    {
        if (fireAction != null)
        {
            fireAction.performed -= OnFirePerformed;
            fireAction.Disable();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (fireAction != null) fireAction.performed -= OnFirePerformed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // If this manager is kept with DontDestroyOnLoad, refresh children from the new scene
        RebuildWeapons();
    }

    private void RebuildWeapons()
    {
        weapons.Clear();
        weapons.AddRange(GetComponentsInChildren<WeaponBase>(true));

        // Clamp and activate
        if (weapons.Count == 0)
        {
            currentWeaponIndex = 0;
            return;
        }

        currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, weapons.Count - 1);
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] != null)
                weapons[i].gameObject.SetActive(i == currentWeaponIndex);
        }
    }

    private void OnFirePerformed(InputAction.CallbackContext _)
    {
        if (Time.timeScale <= 0.5f) return;                 // don't fire during wheel
        if (weapons.Count == 0) return;

        // Skip destroyed/null entries safely
        var w = weapons[currentWeaponIndex];
        if (w == null) { RebuildWeapons(); return; }

        w.Fire();
    }

    public void SelectWeapon(int index)
    {
        if (weapons.Count == 0) return;
        if (index < 0 || index >= weapons.Count) return;

        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i] != null) weapons[i].gameObject.SetActive(i == index);

        currentWeaponIndex = index;
        Debug.Log($"Selected weapon: {weapons[index].weaponName}");
    }

    public int GetWeaponIndexByAngle(float angle)
    {
        if (weapons.Count == 0) return 0;
        float slice = 360f / weapons.Count;
        return Mathf.Clamp(Mathf.FloorToInt((angle + 360f) % 360f / slice), 0, weapons.Count - 1);
    }

    public WeaponBase getWeaponByIndex(int index) => (index >= 0 && index < weapons.Count) ? weapons[index] : null;
}
