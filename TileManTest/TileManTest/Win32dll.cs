using Handles;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using WinApi.Gdi32;
using WinApi.User32;

namespace MouseCaptureTest
{
    internal class Win32dll
    {
        public const int STRING_BUFFER_LENGTH = 1024;

        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern IntPtr WindowFromPoint( int x , int y );

        [DllImport( "user32.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        private static extern int GetWindowText( IntPtr hWnd , string lpString , int cch );

        public static string GetWindowText( IntPtr hWnd )
        {
            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = GetWindowText( hWnd , text , text.Length );
            if ( len == 0 )
                return null;
            else
                return text.Substring( 0 , len );
        }

        [DllImport( "user32.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        private static extern int GetClassName( IntPtr hWnd , string lpString , int cch );

        public static string GetClassName( IntPtr hWnd )
        {
            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = GetClassName( hWnd , text , text.Length );
            if ( len == 0 )
                return null;
            else
                return text.Substring( 0 , len );
        }

        public enum GetAncestorFlags : uint
        {
            GA_PARENT = 1,
            GA_ROOT = 2,
            GA_ROOTOWNER = 3
        }
        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern IntPtr GetAncestor( IntPtr hWnd , GetAncestorFlags gaFlags );

        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern uint GetWindowThreadProcessId( IntPtr hWnd , out uint ProcessId );

        public enum ProcessAccessFlags : uint
        {
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x00000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            SuspendResume = 0x00000800,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        [DllImport( "kernel32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern IntPtr OpenProcess( ProcessAccessFlags dwDesiredAccess , bool bInheritHandle , uint dwProcessId );

        [DllImport( "psapi.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern bool EnumProcessModules( IntPtr hProcess , [MarshalAs( UnmanagedType.LPArray )] [In][Out] IntPtr[] lphModule , uint cb , out uint lpcbNeeded );

        [DllImport( "psapi.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        public static extern int GetModuleFileNameEx( IntPtr hProcess , IntPtr hModule , string lpBaseName , int nSize );

        public static string GetModuleFileNameEx( IntPtr hProcess , IntPtr hModule )
        {
            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = GetModuleFileNameEx( hProcess , hModule , text , text.Length );
            if ( len == 0 )
                return null;
            else
                return text.Substring( 0 , len );
        }

        [DllImport( "kernel32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern bool CloseHandle( IntPtr handle );

        [DllImport( "psapi.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        private static extern int GetProcessImageFileName( IntPtr hProcess , string lpString , int cch );

        public static string GetProcessImageFileName( IntPtr hProcess )
        {
            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = GetProcessImageFileName( hProcess , text , text.Length );
            if ( len == 0 )
                return null;
            else
                return text.Substring( 0 , len );
        }

        [DllImport( "kernel32.dll" , CharSet = CharSet.Auto , CallingConvention = CallingConvention.StdCall )]
        public static extern bool QueryFullProcessImageName( IntPtr hProcess , uint dwFlags , string lpExeName , ref int lpdwSize );

        public static string QueryFullProcessImageName( IntPtr hProcess , bool native )
        {
            uint dwFlags = ( native ? ( uint )0x00000001 : ( uint )0 );

            string text = new string( ( char )0 , STRING_BUFFER_LENGTH );
            int len = text.Length;
            if ( QueryFullProcessImageName( hProcess , dwFlags , text , ref len ) )
                return text.Substring( 0 , len );
            else
                return null;
        }

        public const uint WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = -20;
        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern uint GetWindowLong( IntPtr hWnd , int index );

        [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall )]
        public static extern uint SetWindowLong( IntPtr hWnd , int index , uint unValue );

        public class PenBrush : IDisposable
        {
            public IntPtr BrushPtr;
            public static int Color2Int( Color color )
            {

                int code = ( color.R << 16 ) + ( color.G << 8 ) + color.B;
                return code;
            }

            public static int Color2Int( SystemColor systemColor )
            {
                return Color2Int( CreateSolidBrush( systemColor ) );
            }

            public PenBrush(bool pen ,SystemColor sysColor)
            {
                Color color = CreateSolidBrush( sysColor );
                if ( pen )
                {
                    int code = Color2Int( color );
                    BrushPtr = ThreadWindowHandles.CreatePen( PenStyle.PS_SOLID , 2 , ( uint )code );
                }
                else
                {
                    BrushPtr = Gdi32Helpers.CreateSolidBrush( color.R , color.G , color.B );
                }
            }

            public void Dispose()
            {
                ThreadWindowHandles.DeleteObject( BrushPtr );
            }

            static Color CreateSolidBrush( SystemColor sysColor )
            {
                switch ( sysColor )
                {
                    case SystemColor.COLOR_SCROLLBAR:
                        return Color.FromArgb( 100 , 200 , 200 );
                    case SystemColor.COLOR_BACKGROUND:
                        return Color.FromArgb( 0 , 0 , 0 );
                    //case SystemColor.COLOR_ACTIVECAPTION:
                    //    break;
                    //case SystemColor.COLOR_INACTIVECAPTION:
                    //    break;
                    //case SystemColor.COLOR_MENU:
                    //    break;
                    //case SystemColor.COLOR_WINDOW:
                    //    break;
                    //case SystemColor.COLOR_WINDOWFRAME:
                    //    break;
                    //case SystemColor.COLOR_MENUTEXT:
                    //    break;
                    //case SystemColor.COLOR_WINDOWTEXT:
                    //    break;
                    //case SystemColor.COLOR_CAPTIONTEXT:
                    //    break;
                    //case SystemColor.COLOR_ACTIVEBORDER:
                    //    break;
                    //case SystemColor.COLOR_INACTIVEBORDER:
                    //    break;
                    //case SystemColor.COLOR_APPWORKSPACE:
                    //    break;
                    //case SystemColor.COLOR_HIGHLIGHT:
                    //    break;
                    //case SystemColor.COLOR_HIGHLIGHTTEXT:
                    //    break;
                    //case SystemColor.COLOR_BTNFACE:
                    //    break;
                    //case SystemColor.COLOR_BTNSHADOW:
                    //    break;
                    //case SystemColor.COLOR_GRAYTEXT:
                    //    break;
                    //case SystemColor.COLOR_BTNTEXT:
                    //    break;
                    //case SystemColor.COLOR_INACTIVECAPTIONTEXT:
                    //    break;
                    //case SystemColor.COLOR_BTNHIGHLIGHT:
                    //    break;
                    //case SystemColor.COLOR_3DDKSHADOW:
                    //    break;
                    //case SystemColor.COLOR_3DLIGHT:
                    //    break;
                    //case SystemColor.COLOR_INFOTEXT:
                    //    break;
                    //case SystemColor.COLOR_INFOBK:
                    //    break;
                    //case SystemColor.COLOR_HOTLIGHT:
                    //    break;
                    //case SystemColor.COLOR_GRADIENTACTIVECAPTION:
                    //    break;
                    //case SystemColor.COLOR_GRADIENTINACTIVECAPTION:
                    //    break;
                    //case SystemColor.COLOR_MENUHILIGHT:
                    //    break;
                    //case SystemColor.COLOR_MENUBAR:
                    //    break;

                    default:
                        return Color.FromArgb( 0 , 0 , 0 );
                }

            }
        }
    }
}