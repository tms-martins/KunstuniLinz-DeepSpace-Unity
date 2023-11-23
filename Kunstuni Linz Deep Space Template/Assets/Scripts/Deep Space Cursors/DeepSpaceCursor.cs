/*
 * Tiago Martins 2023
 * For the Deep Space at the University of Arts in Linz.
 * Thanks to Axel Bräuer for the original example project and code.
 */

using UnityEngine;
using UnityEngine.Events;

public class DeepSpaceCursor : MonoBehaviour
{
    [Tooltip("When true, this game object will self-destruct immediately when removed by the DeepSpaceCursorManager. If you wish to animate the cursor disappearing, you should set this to \"false\" and subscribe to the OnCursorRemoved event to trigger the intended behaviour.")]
    [SerializeField] protected bool destroyWhenRemoved = true;
    [Tooltip("Called when the cursor is removed by the DeepSpaceCursorManager, and before the object self-destructs (in case DestroyWhenRemoved is set to \"true\").")]
    [SerializeField] protected DeepSpaceCursorEvent onCursorRemoved = new DeepSpaceCursorEvent();

    protected int id;
    protected float x;
    protected float y;

    public float X { get => x; }
    public float Y { get => y; }

    public int Id { get => id; }

    [System.Serializable]
    public class DeepSpaceCursorEvent : UnityEvent<DeepSpaceCursor> { }

    public DeepSpaceCursorEvent OnCursorRemoved { get => onCursorRemoved; }

    public void SetId(int id)
    {
        this.id = id;
    }

    public void SetPosition(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public void CursorRemoved()
    {
        onCursorRemoved.Invoke(this);

        if (destroyWhenRemoved)
        {
            Destroy(this.gameObject);
        }
    }
}
