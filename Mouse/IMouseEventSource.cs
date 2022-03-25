using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyCap.Mouse {
    /// <summary>
    /// Interface for all classes that are firing events based on mouse input.
    /// </summary>
    interface IMouseEventSource {

        /// <summary>
        /// Raised for a mouse event.
        /// </summary>
        event EventHandler<MouseEventArgs> MouseEvent;

        /// <summary>
        /// Whether the current instance is firing events.
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Pauses the firing of mouse events.
        /// </summary>
        void Pause();

        /// <summary>
        /// Starts the firing of mouse events
        /// </summary>
        void Start();
    }
}
