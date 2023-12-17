/*
 * Tiago Martins 2023
 */

using UnityEngine;

public class ToggleActiveOnKey : MonoBehaviour
{
    [SerializeField] protected KeyCode key;
    [SerializeField] protected GameObject targetObject;

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
