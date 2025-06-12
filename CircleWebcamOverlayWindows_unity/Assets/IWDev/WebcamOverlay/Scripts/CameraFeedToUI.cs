using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace IWDev.WebcamOverlay
{
    public class CameraFeedToUI : MonoBehaviour
    {
        [SerializeField]private RawImage _targetImage;
        private WebCamTexture webcamTexture;
        private IEnumerator _currentFixAspectCoro;
        private float _currentCameraAspect;

        public void StartShowCamera(string cameraName)
        {
            if (webcamTexture != null) webcamTexture.Stop();
            webcamTexture = new WebCamTexture(cameraName);
            webcamTexture.Play();
            _targetImage.texture = webcamTexture;
            FixAspect();
        }

        public void FixAspect()
        {
            if (_currentFixAspectCoro != null) StopCoroutine(_currentFixAspectCoro);
            _currentFixAspectCoro = FixAspectCoro();
            StartCoroutine(_currentFixAspectCoro);
        }

        IEnumerator FixAspectCoro()
        {
            while (webcamTexture.width <= 16)
                yield return null;

            _currentCameraAspect = (float)webcamTexture.width / webcamTexture.height;
            _targetImage.rectTransform.sizeDelta = new Vector2(100 * _currentCameraAspect, 100 );
        }

    }
}
