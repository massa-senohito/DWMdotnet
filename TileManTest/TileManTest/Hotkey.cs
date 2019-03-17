﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class HotKey
{
    [DllImport( "user32" , SetLastError = true )]
    private static extern int RegisterHotKey( IntPtr hWnd ,
                                             int id ,
                                             int fsModifier ,
                                             int vk );

    [DllImport( "user32" , SetLastError = true )]
    private static extern int UnregisterHotKey( IntPtr hWnd ,
                                               int id );

    public HotKey( IntPtr hWnd , int id , Keys key )
    {
        this.hWnd = hWnd;
        this.id = id;

        // Keys列挙体の値をWin32仮想キーコードと修飾キーに分離
        int keycode = ( int )( key & Keys.KeyCode );
        int modifiers = ( int )( key & Keys.Modifiers ) >> 16;

        this.lParam = new IntPtr( modifiers | keycode << 16 );

        if ( RegisterHotKey( hWnd , id , modifiers , keycode ) == 0 )
            // ホットキーの登録に失敗
            throw new Win32Exception( Marshal.GetLastWin32Error( ) );
    }

    public void Unregister()
    {
        if ( hWnd == IntPtr.Zero )
            return;

        if ( UnregisterHotKey( hWnd , id ) == 0 )
            // ホットキーの解除に失敗
            throw new Win32Exception( Marshal.GetLastWin32Error( ) );

        hWnd = IntPtr.Zero;
    }

    public IntPtr LParam
    {
        get
        {
            return lParam;
        }
    }
    public int ID
    {
        get
        {
            return id;
        }
    }
    private IntPtr hWnd; // ホットキーの入力メッセージを受信するウィンドウのhWnd
    private readonly int id; // ホットキーのID(0x0000〜0xBFFF)
    private readonly IntPtr lParam; // WndProcメソッドで押下されたホットキーを識別するためのlParam値
}