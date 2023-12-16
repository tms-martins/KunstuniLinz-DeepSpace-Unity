/*
 * Tiago Martins 2023
 * For the Deep Space at the University of Arts in Linz.
 * Thanks to Axel Bräuer for the original example project and code.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OSCUtils;

namespace KunstuniLinz.DeepSpace
{
    public class TuioCursorManager : OSCReceiver
    {
        [Tooltip("When true, will wait for a valid position before invoking onCursorAdded (which is generally preferable).")]
        [SerializeField] protected bool waitForCursorPosition = true;
        [Tooltip("When true, will print debug messages to the console.")]
        [SerializeField] protected bool debugMessages = false;

        // Events which allow other scripts/components to know when cursors are added, updated or removed.
        [SerializeField] protected Tuio2DCursorEvent onCursorAdded = new Tuio2DCursorEvent();
        [SerializeField] protected Tuio2DCursorEvent onCursorUpdated = new Tuio2DCursorEvent();
        [SerializeField] protected Tuio2DCursorEvent onCursorRemoved = new Tuio2DCursorEvent();

        // Event type for cursor events
        [System.Serializable]
        public class Tuio2DCursorEvent : UnityEvent<Tuio2DCursorInfo> { }

        // Publicly accessible properties for cursor events, which other scrips can subscribe to.
        public Tuio2DCursorEvent OnCursorAdded { get => onCursorAdded; }
        public Tuio2DCursorEvent OnCursorUpdated { get => onCursorUpdated; }
        public Tuio2DCursorEvent OnCursorRemoved { get => onCursorRemoved; }

        // Represents the cursor information received from TUIO, and an "alive" flag to help remove old cursors.
        public class Tuio2DCursorInfo
        {
            public float x;
            public float y;
            public int id;
            public bool alive;
        }

        // Dictionary of cursors, indexed by cursor id (as per TUIO)
        Dictionary<int, Tuio2DCursorInfo> cursors = new Dictionary<int, Tuio2DCursorInfo>();

        // This list will be used to index cursors that are not alive and should be removed
        List<Tuio2DCursorInfo> deadCursors = new List<Tuio2DCursorInfo>();


        void Start()
        {
            // Assign the handler function for OSC messages containing TUIO cursor data
            SetAddressHandler("/tuio/2Dcur", HandleTuio2DCursor);
        }


        private void HandleTuio2DCursor(OSCMessage oscMessage)
        {
            // Handle TUIO cursor messages differently, depending on their content:
            //  "alive" messages are a list of ids for cursors that are still present;
            //  "set" messages are the current position and other data for a single cursor.

            if ((string)oscMessage.values[0] == "alive")
            {
                HandleTuioCursorsAlive(oscMessage);
            }
            else if ((string)oscMessage.values[0] == "set")
            {
                HandleTuioCursorSet(oscMessage);
            }
        }


        private void HandleTuioCursorsAlive(OSCMessage oscMessage)
        {
            // Reset the "alive" state of all known cursors. 
            // This state will thene be updated below, based on TUIO data.
            foreach (Tuio2DCursorInfo cursor in cursors.Values)
            {
                cursor.alive = false;
            }

            // TUIO "alive" messages are a list of ids for cursors that are still present.
            // We go through this list of ids one by one.
            for (int i = 1; i < oscMessage.values.Count; i++)
            {
                // Get a cursor id from the message.
                int cursorId = (int)oscMessage.values[i];

                // If the cursor is known (i.e. is indexed in the cursors Dictionary),
                // we mark that cursor as being "alive".
                if (cursors.TryGetValue(cursorId, out Tuio2DCursorInfo cursor))
                {
                    cursor.alive = true;
                }
                // Otherwise we have a new cursor, but we don't know it's exact position yet.
                // If we chose not to wait for a valid position, we can now create a new cursor,
                // add it to our Dictionary and invoke the respective event.
                else if (!waitForCursorPosition)
                {
                    if (debugMessages) Debug.Log($"{GetType().Name} adding cursor with id {cursorId}");
                    Tuio2DCursorInfo newCursor = new Tuio2DCursorInfo();
                    newCursor.id = cursorId;
                    newCursor.alive = true;
                    cursors.Add(cursorId, newCursor);
                    onCursorAdded.Invoke(newCursor);
                }
            }

            // Now that we know which cursors are "alive", we can remove the other ones.
            RemoveDeadCursors();
        }


        private void HandleTuioCursorSet(OSCMessage oscMessage)
        {
            // TUIO "set" messages are the current position and other data for a single cursor.
            // First, we get the unique id of the cursor, so we can look it up in our Dictionary of known cursors.
            int cursorId = (int)oscMessage.values[1];

            // If the cursor is known (i.e. indexed in the cursors Dictionary),
            // we mark it as being "alive", update its state (position) based on the message and invoke the respective event.
            if (cursors.TryGetValue(cursorId, out Tuio2DCursorInfo cursor))
            {
                cursor.alive = true;
                cursor.x = (float)oscMessage.values[2];
                cursor.y = (float)oscMessage.values[3];
                if (debugMessages) Debug.Log($"{GetType().Name} updated cursor {cursorId} at ({cursor.x:0.00},{cursor.y:0.00})");
                onCursorUpdated.Invoke(cursor);
            }
            // Otherwise we have a new cursor.
            // This cursor may have already shown up in the "alive" message, without an actual position.
            // If we chose to wait for a valid position, we can now create a new cursor, fill in the cursor data,
            // add it to our Dictionary and invoke the respective event.
            else if (waitForCursorPosition)
            {
                if (debugMessages) Debug.Log($"{GetType().Name} adding cursor from \"set\" OSC message, with id {cursorId}");
                Tuio2DCursorInfo newCursor = new Tuio2DCursorInfo();
                newCursor.id = cursorId;
                newCursor.alive = true;
                newCursor.x = (float)oscMessage.values[2];
                newCursor.y = (float)oscMessage.values[3];
                cursors.Add(cursorId, newCursor);
                onCursorAdded.Invoke(newCursor);
            }
        }


        private void RemoveDeadCursors()
        {
            // Remove all known cursors that aren't marked as being "alive".
            // If we were iterating through the Dictionary and removing at the same time, we would get an error
            // (because we are modifying a a collection while going through it, and that leads to inconsistencies).
            // Instead, what we do is gather all "dead" cursors in a separate list.
            // We then go through that list (which is not being modified) and remove each cursor from the Dictionary.
            
            // Gather a list of "dead" cursors.
            foreach (var cursor in cursors.Values)
            {
                if (!cursor.alive) deadCursors.Add(cursor);
            }

            // Remove all "dead" cursors from the Dictionary.
            foreach (Tuio2DCursorInfo cursor in deadCursors)
            {
                if (debugMessages) Debug.Log($"{GetType().Name} removing cursor with id {cursor.id}");
                cursors.Remove(cursor.id);
                onCursorRemoved.Invoke(cursor);
            }

            // Clear the dead cursors list for future use.
            deadCursors.Clear();
        }
    }
}

