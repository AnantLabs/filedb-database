using System.Runtime.InteropServices;

namespace FileDbExplorer
{
    class Win32
    {
        [DllImport( "User32.dll" )]
        public static extern short GetKeyState( int vKey );
    }
}
