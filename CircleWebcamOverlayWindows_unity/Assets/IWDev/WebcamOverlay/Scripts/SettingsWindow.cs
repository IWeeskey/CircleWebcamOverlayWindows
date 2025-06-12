using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IWDev.WebcamOverlay
{
    public class SettingsWindow : MonoBehaviour
    {
        [SerializeField] private Transform _scalerParent;
        [SerializeField] private TMP_Dropdown _dropDownTMPro;
        private float _scaleStep = 0.1f;
        
        
        public void SwitchEnable()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void ExitApp()
        {
            Application.Quit();
        }

        public void ScaleUP()
        {
            ChangeScale(true);
        }

        public void ScaleDown()
        {
            ChangeScale(false);
        }

        private void ChangeScale(bool up)
        {
            float currentScale = _scalerParent.localScale.x;

            currentScale += up ? _scaleStep : _scaleStep * -1f;

            if (currentScale <= 0.25f) currentScale = 0.25f;
            if (currentScale > 5) currentScale = 5f;

            _scalerParent.localScale = Vector3.one * currentScale;
        }

        public void SetDropDown(List<WebCamDevice> devices)
        {
            _dropDownTMPro.ClearOptions();
            List<TMP_Dropdown.OptionData> optionsList = new List<TMP_Dropdown.OptionData>();

            foreach (WebCamDevice device in devices)
            {
                TMP_Dropdown.OptionData newEntry = new TMP_Dropdown.OptionData();
                newEntry.text = device.name;
                optionsList.Add(newEntry);
            }
            _dropDownTMPro.AddOptions(optionsList);



        }

        public void OnDropDownChanged()
        {
            CamerasManager.Instance.TrySwitchToCamera(_dropDownTMPro.value);
        }
        
    }
}
