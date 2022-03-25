using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KeyCap.LowLevel.Win32;


namespace KeyCap.Mouse {

    /// <summary>
    /// Globally listens to mouse events through a primitive message loop.
    /// </summary>
    /// <remarks>
    /// This class implements a message loop and a hook, allowing you to be the source of all events for
    /// an entire application, or for a single thread.
    /// 
    /// If your application is using a GUI framework such as WinForms or WPF,
    /// consider using <see cref="MouseListener"/> instead.
    /// </remarks>
    public sealed class MouseListenerApp : IMouseEventSource, IDisposable {

        Hook _hook;
        bool _active;
        bool _inMsgLoop;
        bool _disposedValue;
        WndProc _proc;

        /// <summary>
        /// Global event for all mouse events on the current system.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseEvent;

        /// <summary>
        /// Whether this instance is currently firing mouse events.
        /// </summary>
        public bool Active { get => _hook != null && _hook.Active && _active; }

        /// <summary>
        /// Creates a new instance with the specified message handler, and installs a hook.
        /// </summary>
        public MouseListenerApp(WndProc proc) {
            _hook = Hook.Create(HookType.WH_MOUSE_LL, HookCallback);
            _proc = proc;
        }

        /// <summary>
        /// Pauses the firing of key events.
        /// </summary>
        /// <remarks>
        /// This does not uninstall the hook, it simply stops the main event and processing.
        /// </remarks>
        public void Pause() {
            if (_disposedValue) throw new InvalidOperationException();
            _active = false;
        }

        /// <summary>
        /// Starts the pumping of messages and the firing of events.
        /// </summary>
        public void Start() {
            if (_disposedValue) throw new InvalidOperationException();
            _active = true;
            _inMsgLoop = true;
            Win32Interop.RunMessageLoop(_proc);
            _inMsgLoop = false;
        }

        /// <summary>
        /// Posts a WM_QUIT message to the message loop, causing it to exit.
        /// </summary>
        public void Exit() {
            if (_disposedValue) throw new InvalidOperationException();
            if (_inMsgLoop) WinApi.PostQuitMessage(0);
        }

        /// <summary>
        /// The callback function for the installed hook.
        /// </summary>
        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0 || !_active)
                return WinApi.CallNextHookEx(_hook.Handle, nCode, wParam, lParam);

            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            var action = (Win32MouseEvent)wParam;

            var loc = hookStruct.pt;
            int delta = 0;
            MouseEventType eventType;
            MouseKey key;

            //The two enum system should cover everything in a relatively simple form,
            //and still leave wiggle room
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

                if (_inMsgLoop) {
                    Win32Interop.DoEvents();

                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// GC
        /// </summary>
        ~MouseListenerApp() {
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