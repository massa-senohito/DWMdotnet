﻿using DWMDotnet;
using Handles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Whitebell.Library.Collections.Generic;
using WinApi.User32;
using Castle.DynamicProxy;
using Autofac.Extras.DynamicProxy;
using Autofac;
using static Types;
using TagType = System.String;

namespace TileManTest
{
    public class CallLogger : IInterceptor
    {
        bool IsBlackMethod( IInvocation invocation )
        {
            List<string> blackList = new List<TagType>
            {
                "ClientList",
                "TagList",
            };
            return blackList.Any( s => invocation.Method.Name.Contains( s ) );
        }
#line hidden
        public void Intercept( IInvocation invocation )
        {
            string args = string.Join( ", " , invocation.Arguments.Select( a => ( a ?? "" ).ToString( ) ).ToArray( ) );
            //Trace.WriteLine( invocation.InvocationTarget );
            var cal = invocation.InvocationTarget as ScreenWorld;
            //cal.Dump( );
            if ( !IsBlackMethod( invocation ) )
            {
                args = "";
                Trace.WriteLine( $"method : {invocation.Method.Name} with parameters {args}... " );
            }
            invocation.Proceed( );
            cal = invocation.InvocationTarget as ScreenWorld;
            //cal.Dump( );
            if ( !IsBlackMethod( invocation ) )
            {
                //Trace.WriteLine( $"Done: result was {invocation.ReturnValue}." );
            }
        }
#line default
    }

    public class ScreenWorld : IScreenWorld
    {
        Screen _Screen;
        int AdjacentScrenOffset;
        Rectangle ScreenGeom;
        // コピーならスキャン後やムーブ後
        OrderedDictionary<TagType , TagManager> TagClientDic = new OrderedDictionary<TagType, TagManager>( );
        DebugLogger Logger;
        Font NumberFont;
        SolidBrush NumberBrush = new SolidBrush( Color.FromArgb( 0 , 172 , 204 ));
        SolidBrush CurrentTagRectBrush = new SolidBrush( Color.FromArgb( 63 , 72 , 204 ) );

        public ScreenWorld( Screen screen )
        {
            Logger = new DebugLogger( "ScreenWorld " + screen.DeviceName );
            _Screen = screen;
            AdjacentScrenOffset = screen.Bounds.Left;
            ScreenGeom = screen.WorkingArea;

            NumberFont = new Font( new FontFamily( "Meiryo" ) , 9 , FontStyle.Regular );
    
        }

        public bool IsContainScreen( IntPtr hwnd )
        {
            return _Screen == Screen.FromHandle( hwnd );
        }

        public bool IsSameScreen( Screen screen )
        {
            return screen.Bounds == _Screen.Bounds;
        }

        public List<Client> ClientList(string selectedTag)
        {
            return TagClientDic[ selectedTag ].ClientList;
        }

        public void AddTag( int id )
        {
            var temp = new TagManager( new List<Client>() , id );
            Logger.Debug( "AddTag" );
            TagClientDic.Add( id.ToString( ) , temp );
        }

        public void PaintIcon( List<ListBox> clientTitleList, PaintEventArgs e , TagType SelectedTag)
        {
            int selectedIntTag = int.Parse( SelectedTag );
            for ( int i = 0 ; i < TagClientDic.Count ; i++ )
            {
                var tagMan = TagClientDic.ElementAt( i ).Value;
                var listBox = clientTitleList[ i ];
                // 現在のタグの上に今のタグを確認するための青い四角を書く
                // todo コンストラクタでリストボックス受け取りたい
                int tagName = i + 1;
                Rectangle bounds = listBox.Bounds;
                if ( tagName == selectedIntTag )
                {
                    int x = bounds.X;
                    int y = 0;
                    int w = bounds.Width;
                    int h = 12;
                    e.Graphics.FillRectangle( CurrentTagRectBrush , x , y , w , h );
                }
                e.Graphics.DrawString( tagName.ToString( ) , NumberFont , NumberBrush , bounds.X + bounds.Width / 4 , bounds.Y -13 );
                List<Icon> iconList = tagMan.IconList;
                for ( int j = 0 ; j < iconList.Count ; j++ )
                {
                    var icon = iconList[ j ];
                    var size = 12;
                    var rect = new Rectangle( listBox.Left - 16 , listBox.Top + j * size + 2 , size , size );
                    if ( icon != null && icon.Width > 0)
                    {
                        // なんかnullや破棄されていることがあった
                        e.Graphics.DrawIcon( icon , rect );
                    }
                    else
                    {
                        tagMan.RemoveIcon( j );
                        Logger.Warn( $"icon {j} removed illegal" );
                    }
                }
            }
        }

        public IEnumerable<Client> MovedClients(TagType SelectedTag)
        {
            var clientList = ClientList( SelectedTag );
            foreach ( var client in clientList )
            {
                if ( client.ScreenChanged )
                {
                    yield return client;
                }
            }
        }

        public void SortMaster( Client NextMaster , TagType SelectedTag )
        {
            var tagMan = Tag( SelectedTag );
            tagMan.SortMaster( NextMaster );
        }

        public void UpdateScreen( TagType SelectedTag )
        {
            var clientList = ClientList( SelectedTag );
            foreach ( var client in clientList )
            {
                client.UpdateScreen( );
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

        IntPtr TopMost = new IntPtr( -1 );
        IntPtr NoTopMost = new IntPtr( -2 );
        // todo すべてのクライアントが対象になっている アクティブタグをこのクラス側で管理したい
        public void SetAllWindowFore()
        {
            ForeachClient( c =>
             {
                 Logger.Trace( c.Title );
                 User32Methods.SetWindowPos( c.Hwnd , TopMost , c.X , c.Y , c.W , c.H ,
                     WindowPositionFlags.SWP_NOMOVE | WindowPositionFlags.SWP_NOSIZE );
                 User32Methods.SetWindowPos( c.Hwnd , NoTopMost , c.X , c.Y , c.W , c.H ,
                     WindowPositionFlags.SWP_SHOWWINDOW | WindowPositionFlags.SWP_NOMOVE | WindowPositionFlags.SWP_NOSIZE );
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
        public int TagCount
        {
            get
            {
                return TagClientDic.Count;
            }
        }

        public void Attach( Client client , string dest )
        {
            TagClientDic[ dest ].AddClient( client );
        }

        void ResizeClient( Client c , int x , int y , int w , int h )
        {
            Rectangle rect = new Rectangle( x , y , w , h );
            DWM.resize( c , rect.Left , rect.Top , rect.Width , rect.Height , ScreenGeom );
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
            Logger.Info( $"master {master.Title} : {master.Rect}" );
            if ( onlyOne )
            {
                return;
            }
            var winGeom = ScreenGeom;
            var slaveList = tiledClient.Skip( 1 ).ToList( );
            int slaveCount = slaveList.Count;

            var x = master.Rect.X + master.Rect.Width; 
            int y = winGeom.Top + UIHeight;
            var w = winGeom.Width - masterWidth;
            var h = (ScreenGeom.Height - UIHeight) / slaveCount;

            for ( int i = 0 ; i < slaveCount ; i++ )
            {

                var item = slaveList[ i ];
                bool isLastOne = ( i + 1 == slaveCount );
                var height = isLastOne ? winGeom.Top + winGeom.Height - y - 2 * item.Bw : h - 2 * item.Bw;
                ResizeClient( item , x , y , w - 2 * item.Bw , height );
                Logger.Info( $"slave {item.Title} : {item.Rect}" );
                User32Methods.GetWindowRect( item.Hwnd , out var rect );
                if ( !isLastOne )
                {
                    y = item.Rect.Top + rect.Height;
                }
            }
            
        }

        public static IScreenWorld CreateScreenWorld( Screen screen )
        {
            if ( false )
            {

                var builder = new ContainerBuilder( );
                builder.RegisterType<ScreenWorld>( ).As<IScreenWorld>( ).EnableInterfaceInterceptors( );
                builder.Register( c => new CallLogger( ) );
                var container = builder.Build( );
                var willBeIntercepted = container.Resolve<IScreenWorld>( TypedParameter.From( screen ) );
                return willBeIntercepted;
            }
            else
            {
                return new ScreenWorld( screen );
            }

        }

        public void Dump()
        {
        }

        public override string ToString()
        {
            return _Screen.DeviceName;
        }


    }
}
