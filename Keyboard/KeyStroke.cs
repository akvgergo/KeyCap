using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyCap.Keyboard;

namespace KeyCap {
    
    /// <summary>
    /// Stores a keystroke, and the state of modifier keys at the time of the key press. 
    /// </summary>
    public class KeyStroke : IKeyStroke {

        /// <summary>
        /// Whether the Shift modifier key was held down. Null means
        /// whether this key was pressed should be ignored when comparing.
        /// </summary>
        public bool? Shift { get; set; }
        /// <summary>
        /// Whether the Ctrl modifier key was held down. Null means
        /// whether this key was pressed should be ignored when comparing.
        /// </summary>
        public bool? Ctrl { get; set; }
        /// <summary>
        /// Whether the Alt modifier key was held down. Null means
        /// whether this key was pressed should be ignored when comparing.
        /// </summary>
        public bool? Alt { get; set; }
        /// <summary>
        /// Whether the Win modifier key was held down. Null means
        /// whether this key was pressed should be ignored when comparing.
        /// </summary>
        public bool? Win { get; set; }
        /// <summary>
        /// The main key that defines the key combination.
        /// </summary>
        public Keys MainKey { get; set; }

        /// <summary>
        /// Creates a new <see cref="KeyStroke"/> using the spacified <paramref name="mainKey"/> and modifier keys.
        /// </summary>
        public KeyStroke(Keys mainKey, bool? ctrl = null, bool? shift = null, bool? alt = null, bool? win = null) {
            MainKey = mainKey;
            Shift = shift;
            Ctrl = ctrl;
            Alt = alt;
            Win = win;
        }

        /// <summary>
        /// Returns the collection of keys in the current instance as an <see cref="IEnumerable"/>.
        /// </summary>
        public IEnumerable<Keys> GetKeyList() {
            if (Shift == true) 
                yield return Keys.SHIFT;

            if (Ctrl == true)
                yield return Keys.CONTROL;

            if (Alt == true)
                yield return Keys.MENU;

            if (Win == true)
                yield return Keys.LWIN;

            yield return MainKey;
        }

        /// <summary>
        /// Matches this filter against a <see cref="KeyboardState"/> instance, returning true if
        /// the instance contains all the keys in this filter.
        /// </summary>
        public bool Match(KeyboardState kbState) {
            if (!kbState.Contains(MainKey))
                return false;
            
            if (Shift.HasValue && Shift != kbState.Shift)
                return false;

            if (Ctrl.HasValue && Ctrl != kbState.Ctrl)
                return false;

            if (Alt.HasValue && Alt != kbState.Alt)
                return false;

            if (Win.HasValue && Win != kbState.Win)
                return false;

            return true;
        }

        /// <summary>
        /// Returns the enumerator for this instance.
        /// </summary>
        public IEnumerator<Keys> GetEnumerator() {
            return GetKeyList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetKeyList().GetEnumerator();
        }
    }

    /// <summary>
    /// Interface for classes that store a specific key combination.
    /// </summary>
    public interface IKeyStroke : IEnumerable<Keys> {

        /// <summary>
        /// Lists all the keys that should be pressed to match this <see cref="IKeyStroke"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<Keys> GetKeyList();

        /// <summary>
        /// Matches this <see cref="IKeyStroke"/> against a <see cref="KeyboardState"/>.
        /// </summary>
        /// <returns>
        /// True> if the <paramref name="kbState"/> has all the necessary keys pressed concurrently.
        /// Otherwise, false.
        /// </returns>
        bool Match(KeyboardState kbState);
    }
}
