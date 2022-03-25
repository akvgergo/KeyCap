using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KeyCap.LowLevel.Windows;


namespace KeyCap.Mouse {

    /// <summary>
    /// Installs a mouse hook on the calling thread, tracking mouse events globally.
    /// This class only works if the owner thread is running a message loop.
    /// </summary>
    public sealed class MouseListener : IMouseEventSource, IDisposable {

        bool _active;
        Hook _hook;
        bool _disposedValue;

        /// <summary>
        /// Global event for all mouse events on the current system.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseEvent;

        /// <summary>
        /// Whether this instance is currently firing mouse events.
        /// </summary>
        public bool Active { get => _hook != null && _hook.Active && _active; }

        /// <summary>
        /// Creates a new instance and starts listening to mouse events.
        /// </summary>
        public MouseListener() {
            _hook = Hook.Create(HookType.WH_MOUSE_LL, HookCallback);
        }

        /// <summary>
        /// Starts the firing of key events.
        /// </summary>
        public void Start() {
            if (_disposedValue) throw new InvalidOperationException();
            _active = true;
        }

        /// <summary>
        /// Pauses the firing of key events.
        /// </summary>
        public void Pause() {
            if (_disposedValue) throw new InvalidOperationException();
            _active = false;
        }

        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0 || !_active)
                return WinApi.CallNextHookEx(_hook.Handle, nCode, wParam, lParam);

            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            var action = (Win32MouseEvent)wParam;

            var loc = hookStruct.pt;
            int delta = 0;
            MouseEventType eventType;
            MouseKey key;

            switch (action) {
                case Win32MouseEvent.WM_MOUSEMOVE:
                    eventType = MouseEventType.MouseMove;
                    key = MouseKey.None;
                    break;
                case Win32MouseEvent.WM_LBUTTONDOWN:
                    eventType = MouseEventType.MouseKeyDown;
                    key = MouseKey.LeftButton;
                    break;
                case Win32MouseEvent.WM_LBUTTONUP:
                    eventType = MouseEventType.MouseKeyUp;
                    key = MouseKey.LeftButton;
                    break;
                case Win32MouseEvent.WM_RBUTTONDOWN:
                    eventType = MouseEventType.MouseKeyDown;
                    key = MouseKey.RightButton;
                    break;
                case Win32MouseEvent.WM_RBUTTONUP:
                    eventType = MouseEventType.MouseKeyUp;
                    key = MouseKey.RightButton;
                    break;
                case Win32MouseEvent.WM_MBUTTONDOWN:
                    eventType = MouseEventType.MouseKeyDown;
                    key = MouseKey.MiddleButton;
                    break;
                case Win32MouseEvent.WM_MBUTTONUP:
                    eventType = MouseEventType.MouseKeyUp;
                    key = MouseKey.MiddleButton;
                    break;
                case Win32MouseEvent.WM_MOUSEWHEEL:
                    eventType = MouseEventType.MouseScroll;
                    key = MouseKey.MiddleButton;
                    delta = hookStruct.mouseData >> 16;
                    break;
                case Win32MouseEvent.WM_XBUTTONDOWN:
                    eventType = MouseEventType.MouseKeyDown;
                    key = (MouseKey)(hookStruct.mouseData >> 16) + 3;
                    break;
                case Win32MouseEvent.WM_XBUTTONUP:
                    eventType = MouseEventType.MouseKeyUp;
                    key = (MouseKey)(hookStruct.mouseData >> 16) + 3;
                    break;
                case Win32MouseEvent.WM_MOUSEHWHEEL:
                    eventType = MouseEventType.MouseScrollHorizontal;
                    key = MouseKey.MiddleButton;
                    delta = hookStruct.mouseData >> 16;
                    break;
                default:
                    eventType = MouseEventType.Special;
                    key = MouseKey.None;
                    break;
            }

            var eventArgs = new MouseEventArgs(loc, eventType, key, (int)action, delta);

            MouseEvent?.Invoke(this, eventArgs);

            if (eventArgs.Handled) return new IntPtr(1);

            return WinApi.CallNextHookEx(_hook.Handle, nCode, wParam, lParam);
        }

        private void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    MouseEvent = null;
                }

                _hook.Dispose();
                _disposedValue = true;
            }
        }
        
        /// <summary>
        /// GC
        /// </summary>
        ~MouseListener() {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Stops firing events and uninstalls the hook.
        /// </summary>
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}