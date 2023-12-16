/*
 * Tiago Martins 2023
 * For the Deep Space at the University of Arts in Linz.
 */

using UnityEngine;

namespace KunstuniLinz.DeepSpace
{
    public class DeepSpaceSettingsLoader : MonoBehaviour
    {
        [SerializeField] string settingsFileName = "Settings/DeepSpaceSettings.txt";
        [SerializeField] DeepSpaceSettingsSO deepSpaceSettings;
        [SerializeField] Camera wallCamera;
        [SerializeField] Camera floorCamera;
        [SerializeField] TuioCursorManager tuioCursorManager;
        [SerializeField] GameObject debugUiParent;
        [SerializeField] bool destroyComponentAfterLoading = false;
        [SerializeField] bool destroyGameObjectAfterLoading = false;

        void Start()
        {
            LoadSettings();
        }

        void LoadSettings()
        {
            bool settingsLoaded = false;

            if (deepSpaceSettings == null)
            {
                Debug.Log($"{GetType().Name}: cannot load settings because there's no settings scriptable object assigned.");
                return;
            }

            if (Application.isEditor)
            {
                Debug.Log($"{GetType().Name}: cannot load settings from a file when playing in the editor.");
                settingsLoaded = true;
            }
            else
            {
                string dataPath = Application.dataPath;
                Debug.Log($"{GetType().Name}: application data path [{dataPath}]");

                try
                {
                    string jsonString = System.IO.File.ReadAllText(dataPath + "/" + settingsFileName);

                    // we aren't allowed to create a new SO at runtime
                    // so we overwrite the existing one
                    JsonUtility.FromJsonOverwrite(jsonString, deepSpaceSettings);
                    settingsLoaded = true;

                }
                catch (System.IO.FileNotFoundException e)
                {
                    Debug.LogWarning($"{GetType().Name}: cannot load settings, file [{dataPath + "/" + settingsFileName}] doesn't exist, exception:\n{e.ToString()}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"{GetType().Name}: could not load settings from [{dataPath + "/" + settingsFileName}] because: \n" + e.Message);
                }
            }

            if (settingsLoaded)
            {
                if (debugUiParent)
                {
                    bool showDebugUi = deepSpaceSettings.showDebugUi != 0 ? true : false;
                    Debug.Log($"{GetType().Name}: setting debug UI visibility to {showDebugUi}");
                    debugUiParent.SetActive(showDebugUi);
                }
                if (wallCamera)
                {
                    Debug.Log($"{GetType().Name}: setting wall camera's display to {deepSpaceSettings.wallCameraDisplayIndex}");
                    wallCamera.targetDisplay = deepSpaceSettings.wallCameraDisplayIndex;
                }
                if (floorCamera)
                {
                    Debug.Log($"{GetType().Name}: setting floor camera's display to {deepSpaceSettings.floorCameraDisplayIndex}");
                    floorCamera.targetDisplay = deepSpaceSettings.floorCameraDisplayIndex;
                }
                if (tuioCursorManager)
                {
                    Debug.Log($"{GetType().Name}: setting Tuio OSC port to {deepSpaceSettings.tuioPort}");
                    tuioCursorManager.SetOSCPort(deepSpaceSettings.tuioPort);
                    tuioCursorManager.Open();
                }
            }

            if (destroyGameObjectAfterLoading)
            {
                Destroy(gameObject);
            }
            else if (destroyComponentAfterLoading)
            {
                Destroy(this);
            }
        }
    }
}

