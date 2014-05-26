using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace FileDbExplorer
{
    class Win32
    {
        [DllImport( "User32.dll" )]
        public static extern short GetKeyState( int vKey );
    }
}
