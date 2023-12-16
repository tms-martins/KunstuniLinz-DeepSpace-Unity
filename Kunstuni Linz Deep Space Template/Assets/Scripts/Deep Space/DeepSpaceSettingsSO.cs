/*
 * Tiago Martins 2023
 * For the Deep Space at the University of Arts in Linz.
 */

using UnityEngine;

namespace KunstuniLinz.DeepSpace
{
    [CreateAssetMenu(fileName = "Deep Space Settings", menuName = "Deep Space/Deep Space Settings")]
    public class DeepSpaceSettingsSO : ScriptableObject
    {
        [SerializeField]
        public int showDebugUi = 1;

        [SerializeField]
        public int wallCameraDisplayIndex = 0;

        [SerializeField]
        public int floorCameraDisplayIndex = 1;

        [SerializeField]
        public int tuioPort = 3333;
    }
}

