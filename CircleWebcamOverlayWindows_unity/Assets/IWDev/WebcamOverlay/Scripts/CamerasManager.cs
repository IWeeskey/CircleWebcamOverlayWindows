using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IWDev.WebcamOverlay
{
    public class CamerasManager : MonoBehaviour
    {
        private static CamerasManager _instance = null;
        public static CamerasManager Instance
        {
            get
            {
                if (_instance == null) _instance = (CamerasManager)FindFirstObjectByType(typeof(CamerasManager));
                return _instance;
            }
        }

        private List<WebCamDevice> _availableCameras;
        [SerializeField]private CameraFeedToUI _cameraShower;
        [SerializeField] private SettingsWindow _settingsWindow;

        private void Start()
        {
            UpdateAvailableCameras();
            TrySwitchToCamera(0);
        }

        public void TrySwitchToCamera(int cameraIndex)
        {
            if (_availableCameras.Count <= 0 || _availableCameras.Count < cameraIndex) return;
            _cameraShower.StartShowCamera(_availableCameras[cameraIndex].name);
        }

        private void UpdateAvailableCameras()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            _availableCameras = new List<WebCamDevice>(devices);
            _settingsWindow.SetDropDown(_availableCameras);
        }
    }
}
