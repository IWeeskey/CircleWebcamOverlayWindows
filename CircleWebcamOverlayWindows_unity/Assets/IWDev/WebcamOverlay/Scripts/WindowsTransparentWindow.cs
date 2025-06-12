using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace IWDev.WebcamOverlay
{
    public class WindowsTransparentWindow : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint WS_EX_LAYERED = 0x00080000;
        private const uint WS_EX_TRANSPARENT = 0x00000020;
        private const uint LWA_COLORKEY = 0x00000001;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MOVE = 0xF010;
        private const int HTCAPTION = 0x2;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW;

        [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")] static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll")] static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_NCRENDERING_POLICY = 2;
        private const int DWMNCRP_DISABLED = 2;
#endif

        public Color32 transparentKey = new Color32(0, 0, 0, 0);

#if UNITY_STANDALONE_WIN
        private IntPtr _hwnd;
        private uint _cachedExStyle;
#endif
        private bool _clickRestoreNextFrame;
        private PointerEventData _replayEvent;

        void Awake()
        {
            SetWindowSizeAndPosition(100, 100, 512, 512);
#if UNITY_EDITOR
            return;
#endif

#if UNITY_STANDALONE_WIN
            _hwnd = GetActiveWindow();

            uint style = WS_POPUP | WS_VISIBLE;
            SetWindowLong(_hwnd, GWL_STYLE, style);

            int disable = DWMNCRP_DISABLED;
            DwmSetWindowAttribute(_hwnd, DWMWA_NCRENDERING_POLICY, ref disable, sizeof(int));

            _cachedExStyle = GetWindowLong(_hwnd, GWL_EXSTYLE) | WS_EX_LAYERED;
            SetWindowLong(_hwnd, GWL_EXSTYLE, _cachedExStyle);

            uint key = (uint)transparentKey.r | ((uint)transparentKey.g << 8) | ((uint)transparentKey.b << 16);
            SetLayeredWindowAttributes(_hwnd, key, 0, LWA_COLORKEY);

            KeepWindowTopmost();
#endif
        }

        private void SetWindowSizeAndPosition(int x, int y, int width, int height)
        {
            SetWindowPos(_hwnd, HWND_TOPMOST, x, y, width, height,
                SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
        void Update()
        {
#if UNITY_EDITOR
            return;
#endif

#if UNITY_STANDALONE_WIN

            ProcessPreviousClick();

            bool pointerOverUnity = IsPointerOverUnityContent();
            _clickRestoreNextFrame = false;
            if (!pointerOverUnity && Input.GetMouseButtonDown(0))
            {
                //SetForegroundWindow(_hwnd);
            }

            ToggleClickThrough(!pointerOverUnity);

            if (pointerOverUnity && Input.GetMouseButtonDown(0))
            {
                SetForegroundWindow(_hwnd);
                _clickRestoreNextFrame = true;
                _replayEvent = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                ReleaseCapture();
                SendMessage(_hwnd, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
            }
#endif
        }

        void LateUpdate()
        {
#if UNITY_EDITOR
            return;
#endif
            ProcessPreviousClick();
        }

        private void ProcessPreviousClick()
        {
            if (!_clickRestoreNextFrame) return;

            _clickRestoreNextFrame = false;

            List<RaycastResult> hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_replayEvent, hits);

            foreach (var hit in hits)
            {
                ExecuteEvents.Execute(hit.gameObject, _replayEvent, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(hit.gameObject, _replayEvent, ExecuteEvents.pointerClickHandler);
            }
        }


#if UNITY_STANDALONE_WIN
        private static bool IsPointerOverUnityContent()
        {
#if UNITY_EDITOR
            return false;
#endif

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return true;

            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, float.MaxValue))
                    return true;
            }
            return false;
        }

        private void ToggleClickThrough(bool enable)
        {
#if UNITY_EDITOR
            return;
#endif

            uint newEx = enable ? (_cachedExStyle | WS_EX_TRANSPARENT)
                                : (_cachedExStyle & ~WS_EX_TRANSPARENT);

            if (GetWindowLong(_hwnd, GWL_EXSTYLE) != newEx)
                SetWindowLong(_hwnd, GWL_EXSTYLE, newEx);

            KeepWindowTopmost();
        }

        private void KeepWindowTopmost()
        {

#if UNITY_EDITOR
            return;
#endif
            SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }
#endif
    }
}
