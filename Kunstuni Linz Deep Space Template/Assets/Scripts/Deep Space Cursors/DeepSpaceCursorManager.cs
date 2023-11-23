/*
 * Tiago Martins 2023
 * For the Deep Space at the University of Arts in Linz.
 * Thanks to Axel Bräuer for the original example project and code.
 */

using System.Collections.Generic;
using UnityEngine;

namespace KunstuniLinz.DeepSpace
{
    public class DeepSpaceCursorManager : MonoBehaviour
    {
        [Tooltip("The TuioCursorManager which we will receive events from. When unassigned, the script will try to find one in the scene.")]
        [SerializeField] protected TuioCursorManager tuioCursorManager = null;
        [Tooltip("The conversion range from TUIO position values to local position values in the X axis. A range of 10 would map TUIO's range [0.0, 1.0] to a local position range [-5.0, 5.0]")]
        [SerializeField] protected float posXRange = 10f;
        [Tooltip("The conversion range from TUIO position values to local position values in the Y axis. For the Y axis, the value is inverted. A range of 10 would map TUIO's range [0.0, 1.0] to a local position range [5.0, -5.0]")]
        [SerializeField] protected float posYRange = 10f;
        [Tooltip("The prefab to be instantiated for each cursor. Must have a DeepSpaceCursor script component")]
        [SerializeField] protected DeepSpaceCursor cursorPrefab = null;
        [Tooltip("The parent trasform for cursors. When unassigned, will use this object's transform. Translate and rotate the parent transform to control the cursors' base position and direction of movement (e.g. \"wall\" or \"floor\" behaviour")]
        [SerializeField] protected Transform cursorsParentTransform = null;

        // The Dictionary of instantiated cursor objects, indexed by TUIO id.
        Dictionary<int, DeepSpaceCursor> cursors = new Dictionary<int, DeepSpaceCursor>();

        void OnEnable()
        {
            // If no parent transform for cursors is assigned, use this object's transform.
            if (cursorsParentTransform == null)
            {
                cursorsParentTransform = this.transform;
            }

            // If no TuioCursorManager is assigned, try to find one in the scene.
            if (tuioCursorManager == null)
            {
                tuioCursorManager = FindObjectOfType<TuioCursorManager>(true);
            }

            // If a TuioCursorManager is assigned, register our own listeners/handlers for cursor events.
            if (tuioCursorManager != null)
            {
                tuioCursorManager.OnCursorAdded.AddListener(this.OnCursorAdded);
                tuioCursorManager.OnCursorUpdated.AddListener(this.OnCursorUpdated);
                tuioCursorManager.OnCursorRemoved.AddListener(this.OnCursorRemoved);
            }
            else {
                Debug.LogError($"{GetType().Name}: could not find a TuioCursorManager in the scene");
                return;
            }
            
        }

        void OnDisable()
        {
            // If a TuioCursorManager is assigned, unregister our listeners/handlers for cursor events.
            if (tuioCursorManager != null)
            {
                tuioCursorManager.OnCursorAdded.RemoveListener(this.OnCursorAdded);
                tuioCursorManager.OnCursorUpdated.RemoveListener(this.OnCursorUpdated);
                tuioCursorManager.OnCursorRemoved.RemoveListener(this.OnCursorRemoved);
            }
        }

        protected void OnCursorAdded(TuioCursorManager.Tuio2DCursorInfo cursorInfo)
        {
            // When a cursor is added, we:
            //   instantiate the prefab as a new GameObject in the scene;
            //   store the id and TUIO position of the cursor;
            //   add the cursor to our dictionary of know cursors;
            //   update the cursor's position in the scene. 

            GameObject cursorGameObject = Instantiate(cursorPrefab.gameObject, cursorsParentTransform);
            DeepSpaceCursor cursor = cursorGameObject.GetComponent<DeepSpaceCursor>();
            cursor.SetId(cursorInfo.id);
            cursor.SetPosition(cursorInfo.x, cursorInfo.y);
            cursors.Add(cursorInfo.id, cursor);
            UpdateCursorObjectPosition(cursor);
        }

        protected void OnCursorUpdated(TuioCursorManager.Tuio2DCursorInfo cursorInfo)
        {
            // When information about a cursor is updated, we retrieve the respective cursor object
            // from our Dictionary and update its position in the scene. 

            if (cursors.TryGetValue(cursorInfo.id, out DeepSpaceCursor cursor))
            {
                cursor.SetPosition(cursorInfo.x, cursorInfo.y);
                UpdateCursorObjectPosition(cursor);
            }
            else
            {
                Debug.LogWarning($"{GetType().Name}: cursor id {cursorInfo.id} is not being tracked");
            }
        }

        protected void OnCursorRemoved(TuioCursorManager.Tuio2DCursorInfo cursorInfo)
        {
            // When a cursor is removed, we remove the respective cursor object
            // from our Dictionary and also tell the cursor itself that it has been removed.
            // It is up to the cursor object to immediately destroy itself or forward the event.
            // You may want to animate the cursor disappearing, for instance, rather than being immediately destroyed.
            // See the DeepSpaceCursor.cs script.

            if (cursors.TryGetValue(cursorInfo.id, out DeepSpaceCursor cursor))
            {
                cursors.Remove(cursor.Id);
                cursor.CursorRemoved();
            }
            else
            {
                Debug.LogWarning($"{GetType().Name}: cursor id {cursorInfo.id} is not being tracked");
            }
        }

        protected void UpdateCursorObjectPosition(DeepSpaceCursor cursor)
        {
            // Sets the cursor object's local position (i.e. in relation to its parent transform)
            // based on a specified conversion range for each axis.
            // TUIO position values come in a range of [0.0, 1.0], which we  mapped to a range of
            // [-posXRange/2, posXRange/2] and [posYRange/2, -posYRange/2] for X and Y respectively.
            // The range is reversed for the Y axis, since TUIO's origin seems to be on the top left (screen space),
            // whereas Unity scenes are based on a cartesian coordinate system.

            cursor.transform.localPosition = new Vector3(
                (cursor.X * posXRange) - (posXRange / 2f),
                (cursor.Y * -posYRange) + (posYRange / 2f),
                cursor.transform.localPosition.z);
        }
    }
}
