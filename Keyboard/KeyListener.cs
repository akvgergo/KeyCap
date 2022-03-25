using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KeyCap;
using KeyCap.LowLevel.Win32;

namespace KeyCap.Keyboard {
    
    /// <summary>
    /// Installs a keyboard hook on the calling thread, tracking key events globally.
    /// This class only works if the owner thread is running a message loop.
    /// </summary>
    /// <remarks>
    /// This class is meant to handle events on a UI thread that is listening for windows messages.
    /// If your app doesn't implement a message loop by itself, <see cref="KeyListenerApp"/>
    /// </remarks>
    public sealed class KeyListener : IKeyEventSource, IDisposable {

        bool _active;
        Hook _hook;
        HashSet<Keys> _keyboardState = new HashSet<Keys>();
        private bool _disposedValue;

        /// <summary>
        /// Whether this instance is currently listening to key events.
        /// </summary>
        public bool Active { get => _hook != null && _hook.Active && _active; }

        /// <summary>
        /// Global event for all key events on the current system.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyEvent;

        /// <summary>
        /// Creates a keyboard hook, listening for key events on the calling thread.
        /// </summary>
        public KeyListener() {
            _hook = Hook.Create(HookType.WH_KEYBOARD_LL, HookCallback);
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

        void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    KeyEvent = null;
                }

                _hook.Dispose();
                _disposedValue = true;
            }
        }

        /// <summary>
        /// GC
        /// </summary>
        ~KeyListener() {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Finalizes this instance, removing all listeners and destroying the installed hook.
        /// </summary>
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// For more in-depth docs, see <see cref="KeyListenerApp"/>
        /// </summary>
        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0 || !_active)
                return WinApi.CallNextHookEx(_hook.Handle, nCode, wParam, lParam);

            Win32KeyEvent wke = (Win32KeyEvent)wParam.ToInt32();
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            KeyEventType ket;

            if (wke == Win32KeyEvent.WM_KEYDOWN || wke == Win32KeyEvent.WM_SYSKEYDOWN) {
                if (!_keyboardState.Contains((Keys)hookStruct.vkCode)) {
                    _keyboardState.Add((Keys)hookStruct.vkCode);
                    ket = KeyEventType.KeyDown;
                } else {
                    ket = KeyEventType.KeyHold;
                }
            } else {
                _keyboardState.Remove((Keys)hookStruct.vkCode);
                ket = KeyEventType.KeyUp;
            }

            var eventArgs = new KeyboardEventArgs(
                ket,
                (Keys)hookStruct.vkCode,
                hookStruct.scanCode,
                _keyboardState.ToArray()
            );


            KeyEvent?.Invoke(this, eventArgs);

            if (eventArgs.Handled) return new IntPtr(1);

            return WinApi.CallNextHookEx(_hook.Handle, eventArgs.Handled ? -1 : nCode, wParam, lParam);
        }
    }
}
