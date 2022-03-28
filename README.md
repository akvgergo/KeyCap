# KeyCap
Mouse and keyboard hook library. Currently for Windows.

![Nuget](https://img.shields.io/nuget/v/KeyCap)

# What it does?

KeyCap is a global mouse and keyboard capture library, written in C#. It currently uses the Windows API
to read mouse and keyboard events, meaning it is currently available for Windows.

# Why?

Fair question considering there are quite a few libraries achieving the same task that
are already available and tested. There are a few reasons:

* This *may* become multiplatform.
  
  I'm currently working on other projects that utilize this library,
  and if there is a demand (despite it being notoriously hard), it's nice to have this piece under
  my control for this possibility.
  
* Other similar projects don't quite fit my needs.

  Similar projects that I found didn't support eg. cancelling keypresses. On top of that, I wanted to be
  completely independent of not just other packages, but WPF and Winforms too.
  
As a side note, I highly recommend using other libraries if you're just looking for just global event capture.
They're likely better documented and tested. But if the above also describes what you're looking for,
give it a try, hopefully I can surprise you.
  
# How do I use it?

You'll need .net 5 (standard), and that's it!

```C#
        //For console apps, you can use KeyListenerApp or MouseListenerApp
        static void Main(string[] args) {
            KeyListenerApp app = new KeyListenerApp(null);
            
            app.KeyEvent += (o, e) => {
                //Ctrl + Esc will exit.
                if (e.KeyboardState.Ctrl == true && e.EventKeyCode == KeyCap.Keys.ESCAPE) {
                    app.Exit();
                }

                //All the relevant info of the event

                //Key press/hold/release, and the key itself
                Console.WriteLine(e.EventType);
                Console.WriteLine(e.EventKeyCode);
                Console.WriteLine(e.ScanCode);

                //If this event generates character input, you can also do this
                if (e.IsCharInput) {
                    Console.WriteLine(e.Characters);
                }

                //All the modifier keys
                Console.WriteLine("Shift: {0}", e.KeyboardState.Shift);
                Console.WriteLine("Ctrl: {0}", e.KeyboardState.Ctrl);
                Console.WriteLine("Alt: {0}", e.KeyboardState.Alt);
                Console.WriteLine("Win: {0}", e.KeyboardState.Win);
            };

            //All the "App" listeners implement a message a loop
            app.Start();
        }
```

```C#
            //If your application already implements a message loop,
            //or you're already using an "App" somewhere, you can use the vanilla listeners.
            MouseListener listener = new MouseListener();
            listener.MouseEvent += (o, e) => {
                //You'll get every single move event, so unless you're tracking the
                //mouse, you should return quickly.
                if (e.EventType == MouseEventType.MouseMove) {
                    return;
                }

                //The type of event (eg. KeyDown, Scroll), and the affected mouse key
                Console.WriteLine(e.EventType);
                Console.WriteLine(e.MouseKey);

                //The location of the pointer
                Console.WriteLine(e.Location);
            };

```

The App listeners internally implement a message loop to capture events. This means that they can be used as the root of
your program. If you want to do a bit more than just processing input globally, you can define a Window Procedure.

```C#
    class Program {

        static void Main(string[] args) {
            //Set up the app, and specify a Window procedure
            KeyListenerApp app = new KeyListenerApp(WindowProc);
            app.KeyEvent += ...

            app.Start();
        }

        static void WindowProc(MSG message) {
            //Handle specific messages here
            //For example, you can also process Clipboard events this way
        }
    }

```

# License

(WTFPL)[https://choosealicense.com/licenses/wtfpl/]
  
