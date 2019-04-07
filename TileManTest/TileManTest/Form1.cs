//#define USESCREENWORLD
//#define RECOVER
using DWMDotnet;
using Handles;
using MouseCaptureTest;
using NetCoreEx.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Whitebell.Library.Collections.Generic;
using WinApi.Gdi32;
using WinApi.User32;
using static MouseCaptureTest.Win32dll;
using static Types;
using TagType = System.String;
using DrawRectangle = System.Drawing.Rectangle;
using System.IO;

namespace TileManTest
{

    public partial class Form1 : Form
    {
        Timer UpdateTimer;
        uint ShellHookID;
        private List<HotKey> HotkeyList = new List<HotKey>( );
        RECT ScreenGeom;
        RECT WindowGeom;
        int BarHeight;
        int BarY;
        bool TopBar = true;
        bool IsDirty = false;
        int UIHeight = 118;
        int ToggleHotID = 100;
        string SelectedTag = "1";

        List<Client> SelectedClientList()
        {
            List<Client> tmp = new List<Client>( );
            foreach ( var screen in ScreenList )
            {

                IEnumerable<Client> collection = screen.ClientList( SelectedTag );
                tmp.AddRange( collection );
            }
            return tmp;
        }

        List<Client> SelectedTiledClient()
        {
            return SelectedClientList( ).Where( c => c.TileMode == TileMode.Tile ).ToList();
        }

        List<IScreenWorld> ScreenList = new List<IScreenWorld>( );
        List<ListBox> ClientTitleList;

        const string SettingPath = "TileSetting.txt";
        TileSetting _TileSetting ;

        void ChangeMaster( Client client )
        {
            List<Client> list = SelectedClientList( );
            var nextMaster = list.Find( c => c == client );
            List<Client> clientList = list;
            clientList.Remove( client );
            clientList.Reverse( );
            clientList.Add( client );
            clientList.Reverse( );
        }

        public Form1()
        {
            _TileSetting = TileSetting.Load( SettingPath );
            var scrList = Screen.AllScreens;
            int widthSum = 0;
            foreach ( var item in scrList )
            {
                Trace.WriteLine( item.DeviceName + " " + item.Bounds );
                ScreenList.Add( ScreenWorld.CreateScreenWorld( item ) );
                // https://stackoverflow.com/questions/53012896/using-setwindowpos-with-multiple-monitors
            }

            try
            {
                InitializeComponent( );

                ClientTitleList = new List<ListBox>( )
                {
                    listBox1,
                    listBox2,
                    listBox3,
                    listBox4,
                    listBox5,
                    listBox6,
                    listBox7,
                    listBox8,
                    listBox9,
                };

                for ( int i = 1 ; i < 10 ; i++ )
                {
                    Keys d0 = ( Keys )( ( int )Keys.D0 + i );
                    const Keys changeTag = Keys.Control ;
                    const Keys sendTag   = Keys.Control | Keys.Alt;
                    var hotkeyNotify = new HotKey( this.Handle, i   , d0 , changeTag);
                    var hotkeySend   = new HotKey( this.Handle, i + 10, d0 , sendTag);
                    HotkeyList.Add( hotkeyNotify );
                    HotkeyList.Add( hotkeySend );

                    foreach ( var item in ScreenList )
                    {
                        item.AddTag( i );
                    }
                    ClientTitleList[ i - 1 ].Click += Form1_SelectedIndexChanged;
                }
                HotkeyList.Add( new HotKey( Handle , ToggleHotID , Keys.H , Keys.Control ) );

                SetUp( );
                ActiveClient = TryGetMaster( );
                UpdateGeom( );
                ThreadWindowHandles.RegisterShellHookWindow( Handle );
                ShellHookID = ThreadWindowHandles.RegisterWindowMessage( "SHELLHOOK" );
                FormClosing += Form1_FormClosing;
                Tile( );
                Bounds = WindowGeom.Rect;
                Height = UIHeight;
                UpdateTimer = new Timer( );
                UpdateTimer.Interval = 10;
                UpdateTimer.Tick += T_Tick;
                UpdateTimer.Start( );
                Paint += Form1_Paint;
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.ToString( ) );
                CleanUp( );
            }
        }

        private void Form1_Paint( object sender , PaintEventArgs e )
        {
            foreach ( var item in ScreenList )
            {
                item.PaintIcon( ClientTitleList , e );
            }
        }

        private void Form1_SelectedIndexChanged( object sender , EventArgs e )
        {
            var listBox = sender as ListBox;
            var newTag  = listBox.Name.Last( ).ToString();
            //ChangeTag( TagClientDic[ SelectedTag.ToString( ) ] , newTag );
            ChangeTag( CurrentTag , newTag );
            SelectedTag = newTag;
            int selectedInd = listBox.SelectedIndex;
            //label3.Text = listBox.Name + " " + selectedInd.ToString();
            if ( selectedInd != ListBox.NoMatches )
            {
                User32Methods.SetForegroundWindow(
                SelectedClientList( )[ selectedInd ].Hwnd );
            }
        }

        private TagManager CurrentTag
        {
            get
            {
                return ScreenList[ 0 ].Tag( SelectedTag );
            }
        }

        private void CleanUp()
        {
            foreach ( var screen in ScreenList )
            {
                screen.CleanUp( );
            }
            foreach ( var hotkey in HotkeyList )
            {
                hotkey.Unregister( );
            }
        }

        private void Form1_FormClosing( object sender , FormClosingEventArgs e )
        {
            CleanUp( );

        }

        protected override void WndProc( ref System.Windows.Forms.Message m )
        {
            // DrawBar
            base.WndProc( ref m );
            //return;
            var code = ( WM )m.Msg;
            var wParam = m.WParam;
            #region barhandler
            switch ( code )
            {
                case WM.CREATE:

                    UpdateBar( );

                    break;
                case WM.PAINT:

                    //PaintStruct paintStruct;
                    //User32Methods.BeginPaint( m.HWnd , out paintStruct );
                    //DrawBar( );
                    //User32Methods.EndPaint( m.HWnd , ref paintStruct );

                    break;
                case WM.LBUTTONDOWN:
                    break;

                default:
                    break;
            }
            #endregion
            if ( code == WM.CLOSE )
            {
            }
            else if ( code == WM.DESTROY )
            {
            }
            else if ( code == WM.HOTKEY )
            {
                OnHotKey( m );
            }
            else
            {
                if ( m.Msg == ShellHookID )
                {

                    var param = ( WM )( wParam );
                    var client = GetClient( m.LParam );
                    OtherWindow( m , param , client );

                }

            }

                //if(code != 0 && wParam != IntPtr.Zero)
                //Trace.WriteLine( $"code : {code} wParam : {wParam}" );
        }

        private void OnHotKey( System.Windows.Forms.Message m )
        {
            foreach ( var item in HotkeyList )
            {
                if ( m.LParam == item.LParam )
                {
                    Trace.WriteLine( $"hotkey {item.ID}" );
                    if ( item.ID == ToggleHotID )
                    {
                        Visible = !Visible;
                        return;
                    }
                    TagSignal( item );
                }
            }
        }

        Client ActiveClient;

        private void TagSignal( HotKey item )
        {
            bool send = 9 < item.ID;
            TagManager currentTag = CurrentTag;
            string itemID = item.ID.ToString( );
            int sentDest = item.ID - 10;
            // 同じタグなら再度入らないように
            if ( sentDest.ToString() == SelectedTag )
            {
                return;
            }
            if ( send )
            {
                if ( ActiveClient != null )
                {
                    IsDirty = true;
                    DWM.setClientVisibility( ActiveClient , false );
                    currentTag.RemoveClient( ActiveClient );
                    Attach( ActiveClient , sentDest.ToString( ) );
                    Tile( );
                    ActiveClient = TryGetMaster( );
                }
            }
            else
            {
                ChangeTag( currentTag , itemID );
            }
        }

        private void ChangeTag( TagManager currentTag , string itemID )
        {
            if ( itemID == SelectedTag )
            {
                return;
            }
            ActiveClient = null;
            // 移動前にマスターの長さを保存
            CurrentTag.MasterWidth = TryGetMaster( )?.W ?? TagManager.DefaultMasterWidth;
            ScreenList[ 0 ].ChangeTag( currentTag , itemID );
            // master1つなら大きさはスクリーンサイズになる、するとスレイブの大きさが0になってしまうので
            if ( CurrentTag.MasterWidth == ScreenGeom.Width )
            {
                CurrentTag.MasterWidth = TagManager.DefaultMasterWidth;
            }

            ScreenList[ 0 ].SetAllWindowFore( );
            SelectedTag = itemID;
            Tile( );
        }

        private void OtherWindow( System.Windows.Forms.Message m , WM param , Client client )
        {
            //Trace.WriteLine( client?.Title + " " + param );
            switch ( param )
            {
                case WM.HSHELL_WINDOWCREATED:
                    TileMode tileMode = IsManageable( m.LParam );
                    if ( tileMode == TileMode.Tile )
                    {
                        var newClient = Manage( m.LParam , tileMode );
                        // とりあえず子ウィンドウはスルー
                        Tile( );
                    }
                    Trace.WriteLine( "created :" + ThreadWindowHandles.GetWindowText( m.LParam ) );

                    break;
                case WM.HSHELL_WINDOWDESTROYED:
                    if ( client != null )
                    {

                        Trace.WriteLine( "destroyed : " + ThreadWindowHandles.GetWindowText( m.LParam ) );
                        // タグ移動で隠された時にも来るので、その場合無視
                        if ( !client.Ignore )
                        {

                            Unmanage( client );
                            Tile( );
                        }
                        else
                        {
                            client.Ignore = false;
                        }
                    }
                    break;
                case WM.HSHELL_WINDOWACTIVATED:
                case WM.HSHELL_RUDEAPPACTIVATED:
                    if ( client != null )
                    {
                        ActiveClient = client;
                        Trace.WriteLine( "active : " + ThreadWindowHandles.GetWindowText( m.LParam ) );
                    }
                    break;

                default:
                    break;
            }

        }

        void UpdateGeom()
        {
            var hwnd = ThreadWindowHandles.FindWindow( "Shell_TrayWnd" , null );
            RECT rect = new RECT();
            if ( ThreadWindowHandles.IsWindowVisible( hwnd ) > 0 )
            {
                ThreadWindowHandles.SystemParametersInfoGet( ( uint )SystemParametersDesktopInfo.SPI_GETWORKAREA , 0 ,
                    ref rect , ( uint )SystemParamtersInfoFlags.None );
                ScreenGeom = rect;
            }
            else
            {
                int sx = User32Methods.GetSystemMetrics( SystemMetrics.SM_XVIRTUALSCREEN );
                int sy = User32Methods.GetSystemMetrics( SystemMetrics.SM_YVIRTUALSCREEN );
                int sw = User32Methods.GetSystemMetrics( SystemMetrics.SM_CXVIRTUALSCREEN );
                int sh = User32Methods.GetSystemMetrics( SystemMetrics.SM_CYVIRTUALSCREEN );
                ScreenGeom = new RECT( sx , sy , sw , sh );
            }
            // 固定値
            BarHeight = 20;
            int wy = ScreenGeom.Top;
            int wh = ScreenGeom.Height;
            BarY = -BarHeight;
            {
                if ( TopBar )
                {
                    wy = ScreenGeom.Top + BarHeight;
                    BarY = wy - BarHeight;
                }
                else
                {
                    BarY = wy + wh;
                }
                wh = ScreenGeom.Height - BarHeight;
            }
            WindowGeom = new RECT( ScreenGeom );
        }

        bool HasUpdateClientTitle( Client client )
        {
            if ( client.HasTitleUpdate( ) )
            {
                IsDirty = true;
                return true;
            }
            return false;
        }

        private void UpdateClient()
        {
            var bui = new StringBuilder( );
            //foreach ( var tagMan in TagClientDic.Values )
            foreach ( var tagMan in ScreenList[0].TagList )
            {
                foreach ( var client in tagMan.ClientList )
                {
                    bui.Append( $"{client.Title} y = {client.Rect.Y} h = {client.Rect.Height}\n" );
                    if ( client.HasTitleUpdate( ) )
                    {
                        IsDirty = true;
                        return;
                    }
                }
            }
            // ScreenList[ 0 ].ForeachClient( HasUpdateClientTitle );
            //label3.Text = bui.ToString( );
        }

        void UpdateTitle( TagManager tagMan )
        {
            ClientTitleList[ tagMan.Id - 1 ].Items.Clear( );
            ClientTitleList[ tagMan.Id - 1 ].Items.AddRange( tagMan.ClientTitles.ToArray( ) );
        }

        private void T_Tick( object sender , EventArgs e )
        {
            DebugLogger.GlobalLogger.Update( );
            var wnd = Win32dll.WindowFromPoint( MousePosition.X , MousePosition.Y );
            uint procID = 0;
            label2.Text = wnd.ToString( );
            //label1.Text = SelectedClientList( ).Aggregate( "" , ( acc , c ) => acc + "\n" + c.Title );
            Win32dll.GetWindowThreadProcessId( wnd , out procID );
            //wnd = Win32dll.OpenProcess( Win32dll.ProcessAccessFlags.QueryInformation | Win32dll.ProcessAccessFlags.VMRead | Win32dll.ProcessAccessFlags.Terminate , false , procID );
            //label3.Text = SelectedTag;
            //var size = User32Methods.GetWindowTextLength( wnd );
            //if ( size > 0 )
            //{
            //    var len = size + 1;
            //    var sb = new StringBuilder( len );

            //    label1.Text = Win32dll.QueryFullProcessImageName( wnd , false );
            //}
            UpdateClient( );

            if ( IsDirty )
            {
                //foreach ( var tagMan in TagClientDic.Values )
                foreach ( var tagMan in ScreenList[0].TagList )
                {
                    UpdateTitle( tagMan );
                }
                Invalidate( );
                IsDirty = false;
            }

            CalcSlaveSizeFromMaster( );

            foreach ( var item in SlaveList( ) )
            {
                if ( item.HasSizeUpdate( ) )
                {

                }
            }
        }

        private void CalcSlaveSizeFromMaster()
        {
            Client masterClient = TryGetMaster( );
            if ( masterClient == null )
            {
                return;
            }
            if ( masterClient.HasSizeUpdate( ) )
            {
                foreach ( var slave in SlaveList( ) )
                {
                    int x = masterClient.Rect.Width;
                    slave.SetXW( x , ScreenGeom.Width - x );
                }
            }
        }

        IScreenWorld FindScreen( Screen screen )
        {
            foreach ( var item in ScreenList )
            {
                if ( item.IsSameScreen(screen) )
                {
                    return item;
                }
            }
            Debug.Assert( false );
            return null;
        }

        private void ArrangeIfScreenChanged()
        {
            foreach ( var client in SelectedClientList() )
            {
                if ( client.ScreenChanged )
                {

                }
            }
        }

        Client GetClient( IntPtr hwnd )
        {
            //foreach ( var item in TagClientDic.Values )
            foreach ( var tagMan in ScreenList[0].TagList )
            {
                var foundItem = tagMan.TryGetClient( hwnd );
                if ( foundItem != null )
                {
                    return foundItem;
                }
            }
            return null;
        }

        Client Manage( IntPtr hwnd , TileMode tileMode )
        {
            //StringBuilder value = ThreadWindowHandles.GetWindowText( hwnd );
            //if ( value != null )
            //{
            //    //Trace.WriteLine( value );
            //}

            if ( ContainClient( hwnd ) )
            {
                return GetClient( hwnd );
            }
            WindowInfo windowInfo = new WindowInfo( );
            User32Methods.GetWindowInfo( hwnd , ref windowInfo );
            Client client = CreateClient( hwnd );
            if ( tileMode == TileMode.Tile )
            {

                WindowPlacement windowPlacement = new WindowPlacement( );
                if ( ThreadWindowHandles.IsWindowVisible( client.Hwnd ) > 0 )
                {
                    User32Methods.SetWindowPlacement( hwnd , ref windowPlacement );
                }

                if ( ThreadWindowHandles.IsWindowVisible( hwnd ) > 0 )
                {
                    NetCoreEx.Geometry.Rectangle windowRect = windowInfo.WindowRect;
                    DWM.resize( client , windowRect.Left , windowRect.Top , windowRect.Width , windowRect.Height , WindowGeom.Rect );
                }
            }
            client.TileMode = tileMode;

            Attach( client , SelectedTag );
            return client;
        }

        private static Client CreateClient( IntPtr hwnd )
        {
            var title = ThreadWindowHandles.GetWindowText( hwnd );
            if ( title == string.Empty )
            {
                title = ThreadWindowHandles.GetClassText( hwnd );
            }
            Client client = Types.createClient(
                hwnd , ThreadWindowHandles.GetParent( hwnd ) ,
                ( int )User32Methods.GetWindowThreadProcessId( hwnd , IntPtr.Zero ) ,
                 WindowStyle( hwnd ) , title );
            return client;
        }

        private void Attach( Client client , string dest )
        {
            //TagClientDic[ dest ].AddClient( client );
            ScreenList[ 0 ].Attach( client , dest );
            IsDirty = true;
        }

        bool Scan( IntPtr hwnd , IntPtr lParam )
        {
            TileMode tileMode = IsManageable( hwnd );
            if ( tileMode != TileMode.NoHandle )
            {
                Manage( hwnd , tileMode);
                // chrome、最小化してるウィンドウがマスターだとサイズが0のように処理される
                ThreadWindowHandles.ShowWindow( hwnd , ThreadWindowHandles.SW.SW_RESTORE );
            }
            return true;
        }

        void SetBorder( Client client , bool border )
        {
            if ( border )
            {
                var style = ( WindowStyles )User32Helpers.GetWindowLongPtr( client.Hwnd , WindowLongFlags.GWL_STYLE );
                var titleAndScaleable = WindowStyles.WS_CAPTION | WindowStyles.WS_SIZEBOX;
                User32Helpers.SetWindowLongPtr( client.Hwnd , WindowLongFlags.GWL_STYLE , ( IntPtr )( style | titleAndScaleable ) );
            }
            else
            {
                //User32Helpers.SetWindowLongPtr( client.Hwnd , WindowLongFlags.GWL_STYLE , ( IntPtr )( style | titleAndScaleable ) );

            }
        }

        void SetUp()
        {
            User32Methods.EnumWindows( Scan , IntPtr.Zero );
            IsDirty = true;
        }

        void SetupBar()
        {
            //var font = Gdi32Helpers.GetStockObject( StockObject.SYSTEM_FONT );
            //ThreadWindowHandles.SelectObject( Handle , font );
            //blw直書きなので

        }

        TileMode IsManageable( IntPtr hwnd )
        {
            if ( ContainClient( hwnd ) )
            {
                return TileMode.NoHandle;
            }
            if ( hwnd == Handle )
            {
                return TileMode.NoHandle;
            }
            String value = ThreadWindowHandles.GetWindowText( hwnd );
            String classText = ThreadWindowHandles.GetClassText( hwnd );
            bool isBlackListName = false;
            if ( !_TileSetting.IsTilingTarget( hwnd ) )
            {
                isBlackListName = true;
            }
            Trace.WriteLine( value + " : className = " + classText );
#if RECOVER
            if ( 
                value.Contains( "Chrome" ) ||
                value.Contains( "Avast Secure" ) ||
                classText.Contains( "CabinetWClass" )
                )
            {
                DWM.setVisibility( hwnd , true );
                return TileMode.Tile;
            }
#endif
            var parent = ThreadWindowHandles.GetParent( hwnd );
            var owner = User32Helpers.GetWindow( hwnd , GetWindowFlag.GW_OWNER );
            WindowStyles style = WindowStyle( hwnd );
            //tosafeがほしいけど別ライブラリ内
            var exStyle = ( WindowExStyles )( User32Helpers.GetWindowLongPtr( hwnd , WindowLongFlags.GWL_EXSTYLE ).ToInt32( ) );
            bool isParentOK = parent != IntPtr.Zero && IsManageable( parent ) == TileMode.Tile;
            bool isTool = ( exStyle & WindowExStyles.WS_EX_TOOLWINDOW ) != 0;
            bool isApp = ( exStyle & WindowExStyles.WS_EX_APPWINDOW ) != 0;
            if ( isParentOK && !ContainClient( parent ) )
            {
                // なんか見えないウィンドウをmanageするので
                //Manage( hwnd );
            }

            if ( ( style & WindowStyles.WS_DISABLED ) != 0 )
            {
                return TileMode.NoHandle;
            }
            // WS_POPUPWINDOW も含んでいる
            if ( ( style & WindowStyles.WS_POPUP ) != 0 )
            {
                return TileMode.NoHandle;
            }
            bool hasParent = parent != IntPtr.Zero;
            var winIsVisibleAndNoParent = !hasParent && User32Methods.IsWindowVisible( hwnd );
            if ( winIsVisibleAndNoParent || isParentOK )
            {
                if ( ( !isTool && !hasParent ) || ( isTool && isParentOK ) || ( isApp && hasParent ) )
                {
                    if ( value != null )
                    {

                        Trace.Write( value );
                        Trace.WriteLine( " isTool : " + isTool + " isApp : " + isApp + " style : " + style );
                    }
                    if ( isBlackListName )
                    {
                        return TileMode.Float;
                    }
                    return TileMode.Tile;
                }
            }
            return TileMode.NoHandle;
        }

        private static WindowStyles WindowStyle( IntPtr hwnd )
        {
            return ( WindowStyles )User32Helpers.GetWindowLongPtr( hwnd , WindowLongFlags.GWL_STYLE ).ToInt32( );
        }

        private bool ContainClient( IntPtr hwnd )
        {
            //foreach ( var item in TagClientDic.Values )
            foreach ( var tagMan in ScreenList[0].TagList )
            {
                bool contains = tagMan.HasClient( hwnd );
                if ( contains )
                {
                    return true;
                }
            }
            return false;
        }

        void ResizeClient( Client c , int x , int y , int w , int h )
        {
            Trace.WriteLine( $"{c.Title} rect : {c.Rect}" );
            Rectangle rect = new Rectangle( x , y , x + w , y + h );
            DWM.resize( c , rect.Left , rect.Top , rect.Width , rect.Height , ScreenGeom.Rect );
            Trace.WriteLine( $"after {c.Title} rect : {c.Rect}" );
        }

        void Tile()
        {
            foreach ( var screen in ScreenList )
            {
                Trace.WriteLine( $"Tile() {screen}" );
                Trace.Indent( );
                screen.Tile( SelectedTag , UIHeight );
                Trace.Unindent( );
            }
#if OLDTILE
            List<Client> clientList = SelectedTiledClient( );
            if ( clientList.Count == 0 )
            {
                return;
            }
            var master = clientList.First( );
            RECT winGeom = WindowGeom;
            // MasterWidthとどのスクリーン化だけ覚えておくほうが
            var masterWidth = CurrentTag.MasterWidth;//MasterFact * winGeom.Width;

            bool onlyOne = clientList.Count == 1;
            float width = ( onlyOne ? winGeom.Width : masterWidth ) - ( 2 * master.Bw );
            ResizeClient( master , winGeom.Left , winGeom.Top + UIHeight ,
                ( int )width , winGeom.Height - UIHeight - 2 * master.Bw );
            if ( onlyOne )
            {
                return;
            }
            var x = master.Rect.X + master.Rect.Width;
            int y = winGeom.Top + UIHeight;
            var w = winGeom.Left + winGeom.Width - ( int )width;
            var h = ( WindowGeom.Height - UIHeight ) / clientList.Count;
            if ( h < BarHeight )
            {
                h = winGeom.Height;
            }
            var slaveList = SlaveList( );
            var ver = Environment.OSVersion.Version.Major;
            int slaveCount = slaveList.Count;
            if ( ver > 5 )
            {

            }
            for ( int i = 0 ; i < slaveCount ; i++ )
            {

                var item = slaveList[ i ];
                bool isLastOne = ( i + 1 == slaveCount );
                var height = isLastOne ? winGeom.Top + winGeom.Height - y - 2 * item.Bw : h - 2 * item.Bw;
                ResizeClient( item , x , y , w - 2 * item.Bw , height );
                User32Methods.GetWindowRect( item.Hwnd , out Rectangle rect );
                if ( !isLastOne )
                {
                    y = item.Rect.Top + rect.Height;
                }
            }
#endif
        }

        private Client TryGetMaster( )
        {
            var master = SelectedTiledClient( ).FirstOrDefault( );
            //Trace.WriteLine( master?.Title );
            return master;
        }

        private List<Client> SlaveList( )
        {
            return SelectedTiledClient( ).Skip( 1 ).ToList( );
        }

        void Unmanage( Client client )
        {
            DWM.setClientVisibility( client , true );
            Detach( client );
        }

        private void Detach( Client client )
        {
            //foreach ( var item in TagClientDic.Values )
            foreach ( var tagMan in ScreenList[0].TagList )
            {
                var contains = tagMan.RemoveClient(client);
                if ( contains  )
                {
                    IsDirty = true;
                    return ;
                }
            }
        }

        void UpdateBar()
        {
            User32Helpers.SetWindowPos( Handle , HwndZOrder.HWND_TOPMOST 
                , 0 , BarY , WindowGeom.Width , BarHeight ,
                WindowPositionFlags.SWP_SHOWWINDOW | WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_NOSENDCHANGING );
        }

        private void QuitButton_Click( object sender , EventArgs e )
        {
            CleanUp( );
            Close( );
            File.WriteAllText( SettingPath , _TileSetting.ToJson( ) );
        }
    }
}
