using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyCap.LowLevel;

namespace KeyCap.Keyboard {

    /// <summary>
    /// Represents a momentary state of the keyboard, storing all the keys that were pressed simultanously.
    /// </summary>
    public class KeyboardState : IEnumerable<Keys> {

        Keys[] _pressedKeys;

        /// <summary>
        /// The keys held down in this keyboard state.
        /// </summary>
        public Keys[] PressedKeys {
            get => _pressedKeys.ToArray();
        }

        /// <summary>
        /// Creates a new keyboardstate from an <see cref="IEnumerable{Keys}"/> representing the keys held.
        /// </summary>
        public KeyboardState(IEnumerable<Keys> keys) {
            _pressedKeys = keys.ToArray();
        }

        /// <summary>
        /// Whether a Control key is held down.
        /// </summary>
        public bool Ctrl {
            get => _pressedKeys.ContainsAny(new[] { Keys.CONTROL, Keys.LCONTROL, Keys.RCONTROL });
        }

        /// <summary>
        /// Whether a Shift key is held down
        /// </summary>
        public bool Shift {
            get => _pressedKeys.ContainsAny(new[] { Keys.SHIFT, Keys.LSHIFT, Keys.RSHIFT });
        }

        /// <summary>
        /// Whether an Alt key is held down.
        /// </summary>
        public bool Alt {
            get => _pressedKeys.ContainsAny(new[] { Keys.MENU, Keys.LMENU, Keys.RMENU });
        }

        /// <summary>
        /// Whether a Windows key is held down.
        /// </summary>
        public bool Win {
            get => _pressedKeys.ContainsAny(new[] { Keys.LWIN, Keys.RWIN });
        }

        /// <summary>
        /// Gets an enumerator for all the keys currently held down.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Keys> GetEnumerator() {
            return _pressedKeys.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
