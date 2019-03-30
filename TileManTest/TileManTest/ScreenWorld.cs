using DWMDotnet;
using Handles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Whitebell.Library.Collections.Generic;
using WinApi.User32;
using static Types;
using TagType = System.String;

namespace TileManTest
{
    class ScreenWorld
    {
        Screen _Screen;
        int AdjacentScrenOffset;
        Rectangle ScreenGeom;
        // コピーならスキャン後やムーブ後
        OrderedDictionary<TagType , TagManager> TagClientDic = new OrderedDictionary<TagType, TagManager>( );

        public ScreenWorld( Screen screen )
        {
            _Screen = screen;
            AdjacentScrenOffset = screen.Bounds.Left;
            ScreenGeom = screen.Bounds;
        }

        public bool IsContainScreen( IntPtr hwnd )
        {
            return _Screen == Screen.FromHandle( hwnd );
        }

        public bool IsSameScreen( Screen screen )
        {
            return screen == _Screen;
        }

        public List<Client> ClientList(string selectedTag)
        {
            return TagClientDic[ selectedTag ].ClientList;
        }

        public void AddTag( int id )
        {
            var temp = new TagManager( new List<Client>() , id );
            TagClientDic.Add( id.ToString( ) , temp );
        }

        public void PaintIcon( List<ListBox> clientTitleList, PaintEventArgs e )
        {
            for ( int i = 0 ; i < TagClientDic.Count ; i++ )
            {
                var tagMan = TagClientDic.ElementAt( i ).Value;
                var listBox = clientTitleList[ i ];
                //foreach ( var icon in tagMan.IconList )
                List<Icon> iconList = tagMan.IconList;
                for ( int j = 0 ; j < iconList.Count ; j++ )
                {
                    var icon = iconList[ j ];
                    var size = 12;
                    var rect = new Rectangle( listBox.Left - 16 , listBox.Top + j * size + 2 , size , size );
                    if ( icon != null )
                    {
                        // なんかnullでくることがあった
                        e.Graphics.DrawIcon( icon , rect );
                    }
                    else
                    {
                        tagMan.RemoveIcon( j );
                    }
                }
            }
        }

        public TagManager Tag( string id )
        {
            return TagClientDic[ id ];
        }

        public void ChangeTag( TagManager currentTag , string itemID )
        {
            //if ( itemID == SelectedTag )
            //{
            //    return false;
            //}
            //ActiveClient = null;
            currentTag.Visible( false );
            TagClientDic[ itemID ].Visible( true );
            //SelectedTag = itemID;
            //Tile( );
        }

        public void ChangeTag( string currentTag , string itemID )
        {
            ChangeTag( Tag( currentTag ) , itemID );
        }

        public void CleanUp()
        {
            int y = 0;
            foreach ( var tagMan in TagClientDic.Values )
            {
                foreach ( var client in tagMan.ClientList )
                {
                    DWM.setClientVisibility( client , true );
                    //User32Methods.MoveWindow( client.Hwnd , client.X , client.Y , client.W , client.H , true );
                }
            }
        }

        public void ForeachClient( Predicate<Client> action )
        {
            foreach ( var tagMan in TagClientDic.Values )
            {
                foreach ( var client in tagMan.ClientList )
                {
                    if ( action( client ) )
                    {
                        return;
                    }
                }
            }

        }

        public void SetAllWindowFore()
        {
            ForeachClient( c =>
             {
                 ThreadWindowHandles.SetForegroundWindow( c.Hwnd );
                 return false;
             } );
        }

        public void ForeachTagManager(Action<TagManager> action)
        {
            foreach ( var tagMan in TagClientDic.Values )
            {
                action( tagMan );
            }
        }

        public IEnumerable<TagManager> TagList
        {
            get
            {
                return TagClientDic.Values;
            }
        }

        public void Attach( Client client , string dest )
        {
            TagClientDic[ dest ].AddClient( client );
        }

        void ResizeClient( Client c , int x , int y , int w , int h )
        {
            Trace.WriteLine( $"{c.Title} rect : {c.Rect}" );
            Rectangle rect = new Rectangle( x , y , w , h );
            DWM.resize( c , rect.Left , rect.Top , rect.Width , rect.Height , ScreenGeom );
            Trace.WriteLine( $"after {c.Title} rect : {c.Rect}" );
        }

        public void Tile(string selectedTag , int UIHeight)
        {
            List<Client> tiledClient = ClientList( selectedTag ).Where( c => c.TileMode == TileMode.Tile ).ToList( );
            if ( tiledClient.Count == 0 )
            {
                return;
            }
            var master = tiledClient.First( );
            var masterWidth = Tag( selectedTag ).MasterWidth;
            bool onlyOne = tiledClient.Count == 1;
            masterWidth = onlyOne ? ScreenGeom.Width : masterWidth;
            ResizeClient( master , ScreenGeom.Left , ScreenGeom.Top + UIHeight ,
                masterWidth , ScreenGeom.Height - UIHeight );
            Trace.WriteLine( $"master {master.Title} : {master.Rect}" );
            if ( onlyOne )
            {
                return;
            }
            var winGeom = ScreenGeom;
            var x = master.Rect.X + master.Rect.Width; 
            int y = winGeom.Top + UIHeight;
            var w = winGeom.Width - masterWidth;
            var h = (ScreenGeom.Height - UIHeight) / tiledClient.Count;

            var slaveList = tiledClient.Skip( 1 ).ToList( );
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
                Trace.WriteLine( $"slave {item.Title} : {item.Rect}" );
                User32Methods.GetWindowRect( item.Hwnd , out var rect );
                if ( !isLastOne )
                {
                    y = item.Rect.Top + rect.Height;
                }
            }
            
        }

        public override string ToString()
        {
            return _Screen.DeviceName;
        }
    }
}
