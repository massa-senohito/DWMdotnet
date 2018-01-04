using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Handles
{
  /// <summary>
  /// ウィンドウハンドルを列挙するクラスの共通機能をまとめたクラスです。
  /// </summary>
  public abstract class WindowHandles : IEnumerable<IntPtr>
  {
    [UnmanagedFunctionPointer( CallingConvention.StdCall )]
    [return: MarshalAs( UnmanagedType.Bool )]
    public delegate bool EnumWindowsProcDelegate( IntPtr windowHandle , IntPtr lParam );

    internal List<IntPtr> handles;

    public WindowHandles( )
    {
      handles = new List<IntPtr>( );
    }

    public IEnumerator<IntPtr> GetEnumerator( )
    {
      return handles.GetEnumerator( );
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
    {
      return handles.GetEnumerator( );
    }

    internal bool EnumWindowProc( IntPtr handle , IntPtr lParam )
    {
      handles.Add( handle );
      return true;
    }
  }

  /// <summary>
  /// トップレベルウィンドウのウィンドウハンドルを列挙する機能を提供します。
  /// </summary>
  [SecurityPermission( SecurityAction.Demand , UnmanagedCode = true )]
  public sealed class TopLevelWindowHandles : WindowHandles
  {
    [SuppressUnmanagedCodeSecurity]
    private static class NativeMethods
    {
      [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall , SetLastError = true )]
      [return: MarshalAs( UnmanagedType.Bool )]
      public static extern bool EnumWindows(
          [MarshalAs( UnmanagedType.FunctionPtr )] EnumWindowsProcDelegate enumProc ,
          IntPtr lParam );
    }

    /// <summary>
    /// トップレベルウィンドウのウィンドウハンドルを列挙します。
    /// </summary>
    public TopLevelWindowHandles( )
        : base( )
    {
      handles = new List<IntPtr>( );
      NativeMethods.EnumWindows( EnumWindowProc , default( IntPtr ) );
    }
  }

  /// <summary>
  /// 親ウィンドウの子ウィンドウのウィンドウハンドルを列挙する機能を提供します。
  /// </summary>
  [SecurityPermission( SecurityAction.Demand , UnmanagedCode = true )]
  public sealed class ChildWindowHandles : WindowHandles
  {
    [SuppressUnmanagedCodeSecurity]
    private static class NativeMethods
    {
      [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall , SetLastError = true )]
      [return: MarshalAs( UnmanagedType.Bool )]
      public static extern bool EnumChildWindows(
          IntPtr handle ,
          [MarshalAs( UnmanagedType.FunctionPtr )] EnumWindowsProcDelegate enumProc ,
          IntPtr lParam );
    }

    IntPtr windowHandle;

    /// <summary>
    /// 親ウィンドウの子ウィンドウのウィンドウハンドルを取得します。
    /// </summary>
    /// <param name="windowHandle">親ウィンドウのウィンドウハンドルです。</param>
    public ChildWindowHandles( IntPtr windowHandle )
        : base( )
    {
      this.windowHandle = windowHandle;
      NativeMethods.EnumChildWindows( windowHandle , EnumWindowProc , default( IntPtr ) );
    }

    /// <summary>
    /// 親ウィンドウのウィンドウハンドルです。
    /// </summary>
    public IntPtr WindowHandle { get { return windowHandle; } }
  }

  /// <summary>
  /// スレッドに所属するトップレベルウィンドウのウィンドウハンドルを列挙する機能を提供します。
  /// </summary>
  [SecurityPermission( SecurityAction.Demand , UnmanagedCode = true )]
  public class ThreadWindowHandles : WindowHandles
  {
    [SuppressUnmanagedCodeSecurity]
    public static class NativeMethods
    {
      [DllImport( "user32.dll" , CallingConvention = CallingConvention.StdCall , SetLastError = true )]
      [return: MarshalAs( UnmanagedType.Bool )]
      public static extern bool EnumThreadWindows(
          uint threadId ,
          [MarshalAs( UnmanagedType.FunctionPtr )] EnumWindowsProcDelegate enumProc ,
          IntPtr lParam );
      [DllImport( "user32.dll" , SetLastError = true )]
      public static extern int GetWindowThreadProcessId(
          IntPtr hWnd , out int lpdwProcessId );
      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern int GetWindowText(IntPtr hWnd,
        StringBuilder lpString, int nMaxCount);
      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern int GetWindowTextLength(IntPtr hWnd);
      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern int GetClassName(IntPtr hWnd,
        StringBuilder lpClassName, int nMaxCount);
    }

    uint threadId;

    /// <summary>
    /// スレッドに所属するトップレベルウィンドウのウィンドウハンドルを取得します。
    /// </summary>
    /// <param name="threadId">スレッドIDです。</param>
    public ThreadWindowHandles( uint threadId )
        : base( )
    {
      this.threadId = threadId;
      NativeMethods.EnumThreadWindows( threadId , EnumWindowProc , default( IntPtr ) );
    }

    public static int GetWindowThreadProcessId(IntPtr hWnd)
    {
      int ret = 0;
      return NativeMethods.GetWindowThreadProcessId( hWnd , out ret );
    }

    public static StringBuilder GetWindowText(IntPtr hWnd)
    {
      //ウィンドウのタイトルの長さを取得する
      int textLen = NativeMethods.GetWindowTextLength(hWnd);
      if ( 0 < textLen )
      {
        //ウィンドウのタイトルを取得する
        StringBuilder tsb = new StringBuilder( textLen + 1 );
        NativeMethods.GetWindowText( hWnd , tsb , tsb.Capacity );

        return tsb;
      }
      return null;
    }
    public static StringBuilder GetClassText( IntPtr hWnd )
    {
      //ウィンドウのクラス名を取得する
      StringBuilder csb = new StringBuilder( 256 );
      NativeMethods.GetClassName( hWnd , csb , csb.Capacity );
      return csb;
    }
    /// <summary>
    /// スレッドIDです。
    /// </summary>
    public uint ThreadID { get { return threadId; } }
  }
}
