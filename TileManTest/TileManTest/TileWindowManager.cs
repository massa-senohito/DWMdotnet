using DWMDotnet;
using Handles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Types;

namespace TileManTest
{
    class TileWindowManager
    {
        // Variable
        public event EventHandler OnTagChange;
        public Client ActiveClient
        {
            get;
            set;
        }
        public List<IScreenWorld> ScreenList = new List<IScreenWorld>( );
        public string SelectedTag = "1";
        public bool IsDirty = false;

        public int UIHeight ;
        public RECT ScreenGeom;
        public Screen BelongScreen
        {
            get;
            set;
        }

        // Methods

        public TileWindowManager( int uiHeight )
        {
            var scrList = Screen.AllScreens;
            foreach ( var screen in scrList )
            {
                Trace.WriteLine( screen.DeviceName + " " + screen.Bounds );
                ScreenList.Add( ScreenWorld.CreateScreenWorld( screen ) );
                // https://stackoverflow.com/questions/53012896/using-setwindowpos-with-multiple-monitors
            }
            UIHeight = uiHeight;
        }

        public List<Client> SelectedClientList()
        {
            List<Client> tmp = new List<Client>( );
            foreach ( var screen in ScreenList )
            {

                IEnumerable<Client> collection = screen.ClientList( SelectedTag );
                tmp.AddRange( collection );
            }
            return tmp;
        }

        public List<Client> SelectedTiledClient()
        {
            return SelectedClientList( ).Where( c => c.TileMode == TileMode.Tile ).ToList();
        }

        public TagManager CurrentTag
        {
            get
            {
                return ScreenList[ 0 ].Tag( SelectedTag );
            }
        }

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

        public void SortMaster()
        {
            if ( ActiveClient == null )
            {
                return;
            }
            foreach ( var screen in ScreenList )
            {
                screen.SortMaster( ActiveClient , SelectedTag );
            }
            Tile( );
            IsDirty = true;
        }

        public void TagSignal( HotKey item )
        {
            bool send = ScreenList[0].TagCount < item.ID;
            string itemID = item.ID.ToString( );
            int sentDest = item.ID - ScreenList[0].TagCount;
            // 同じタグなら変更入らないように
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
                    CurrentTag.RemoveClient( ActiveClient );
                    Attach( ActiveClient , sentDest.ToString( ) );
                    Tile( );
                    ActiveClient = TryGetMaster( );
                }
            }
            else
            {
                ChangeTag( CurrentTag , itemID );
            }
        }

        public void ChangeTag( TagManager currentTag , string nextTag )
        {
            if ( nextTag == SelectedTag )
            {
                return;
            }
            ActiveClient = null;
            // 移動前にマスターの長さを保存
            CurrentTag.MasterWidth = TryGetMaster( )?.W ?? TagManager.DefaultMasterWidth;
            foreach ( var screen in ScreenList )
            {
                screen.ChangeTag( currentTag , nextTag );
            }
            // master1つなら大きさはスクリーンサイズになる、するとスレイブの大きさが0になってしまうので
            if ( CurrentTag.MasterWidth == ScreenGeom.Width )
            {
                CurrentTag.MasterWidth = TagManager.DefaultMasterWidth;
            }

            SelectedTag = nextTag;
            CurrentTag.ResetIcon( );
            Tile( );
            CurrentTag.RemoveEmptyTag( );

            // なんかきいてない気がする
            //foreach ( var screen in ScreenList )
            //{
            //    screen.SetAllWindowFore( );
            //}

            OnTagChange( this , new EventArgs( ) );
        }

        public void Tile()
        {
            foreach ( var screen in ScreenList )
            {
                Trace.WriteLine( $"Tile() {screen}" );
                Trace.Indent( );
                int heightOffset = 0;
                if ( screen.IsSameScreen( BelongScreen ) )
                {
                    heightOffset = UIHeight;
                }
                screen.Tile( SelectedTag , heightOffset );
                Trace.Unindent( );
            }
        }

        public Client TryGetMaster( )
        {
            var master = SelectedTiledClient( ).FirstOrDefault( );
            //Trace.WriteLine( master?.Title );
            return master;
        }

        public void Attach( Client client , string dest )
        {
#if MultiScreen
            foreach ( var screen in ScreenList )
            {
                if ( screen.IsSameScreen( client.Screen ) )
                {
                    screen.Attach( client , dest );
                }
            }
#else
            ScreenList[ 0 ].Attach( client , dest );
#endif
            IsDirty = true;
        }
    }
}
