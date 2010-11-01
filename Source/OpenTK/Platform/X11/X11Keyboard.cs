 #region License
 //
 // The Open Toolkit Library License
 //
 // Copyright (c) 2006 - 2010 the Open Toolkit library.
 //
 // Permission is hereby granted, free of charge, to any person obtaining a copy
 // of this software and associated documentation files (the "Software"), to deal
 // in the Software without restriction, including without limitation the rights to
 // use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 // the Software, and to permit persons to whom the Software is furnished to do
 // so, subject to the following conditions:
 //
 // The above copyright notice and this permission notice shall be included in all
 // copies or substantial portions of the Software.
 //
 // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 // OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 // HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 // WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 // FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 // OTHER DEALINGS IN THE SOFTWARE.
 //
 #endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Input;

namespace OpenTK.Platform.X11
{
    // Standard keyboard driver that relies on xlib input events.
    // Only one keyboard supported.
    sealed class X11Keyboard : IKeyboardDriver2
    {
        readonly static X11KeyMap keymap = new X11KeyMap();
        readonly static string name = "Core X11 keyboard";
        KeyboardState state = new KeyboardState();
        readonly byte[] keys = new byte[32];

        public X11Keyboard()
        {
            Debug.WriteLine("Using X11Keyboard.");
        }

        public KeyboardState GetState()
        {
            ProcessEvents();
            return state;
        }

        public KeyboardState GetState(int index)
        {
            // X11Keyboard supports a single keyboard only
            ProcessEvents();
            if (index == 0)
                return state;
            else
                return new KeyboardState();
        }

        public string GetDeviceName(int index)
        {
            if (index == 0)
                return name;
            else
                return String.Empty;
        }

        void ProcessEvents()
        {
            IntPtr display = API.DefaultDisplay;
            using (new XLock(display))
            {
                Functions.XQueryKeymap(display, keys);
                for (int keycode = 8; keycode < 256; keycode++)
                {
                    IntPtr keysym = Functions.XKeycodeToKeysym(display, (byte)keycode, 0);
                    IntPtr keysym2 = Functions.XKeycodeToKeysym(display, (byte)keycode, 1);
                    bool pressed = (keys[keycode >> 3] >> (keycode & 0x07) & 0x01) != 0;

                    Key key;
                    if (keymap.TryGetValue((XKey)keysym, out key) ||
                        keymap.TryGetValue((XKey)keysym2, out key))
                    {
                        if (pressed)
                            state.EnableBit((int)key);
                        else
                            state.DisableBit((int)key);
                    }
                }
            }
        }
    }
}

