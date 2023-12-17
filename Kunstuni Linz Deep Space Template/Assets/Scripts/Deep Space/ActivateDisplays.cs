/*
 * Tiago Martins 2023
 * For the Deep Space at the University of Arts in Linz.
 * Based on https://docs.unity3d.com/Manual/MultiDisplay.html
 */

using UnityEngine;

namespace KunstuniLinz.DeepSpace
{
    public class ActivateDisplays : MonoBehaviour
    {
        void Start()
        {
            string message = $"Displays connected ({Display.displays.Length}):";
            for (int i = 0; i < Display.displays.Length; i++)
            {
                var display = Display.displays[i];
                message += $"\n\t[{i}] system WxH {display.systemWidth}x{display.systemHeight}; render WxH {display.renderingWidth}x{display.renderingHeight}";
            }
            Debug.Log(message);

            // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
            // Check if additional displays are available and activate each.
            for (int i = 1; i < Display.displays.Length; i++)
            {
                Display.displays[i].Activate();
            }

            Screen.fullScreen = true;
        }
    }
}
