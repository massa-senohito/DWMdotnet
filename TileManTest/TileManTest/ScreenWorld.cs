﻿using DWMDotnet;
using Handles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        OrderedDictionary<TagType , TagManager> UnhandledTag = new OrderedDictionary<TagType, TagManager>();
        // コピーならスキャン後やムーブ後
        OrderedDictionary<TagType , TagManager> TagClientDic = new OrderedDictionary<TagType, TagManager>( );

        public ScreenWorld( Screen screen )
        {
            _Screen = screen;
            AdjacentScrenOffset = screen.Bounds.Left;
            //UnhandledTag = new TagManager( new List<Types.Client>( ) , 20 );
        }

        public bool IsContainScreen( IntPtr hwnd )
        {
            return _Screen == Screen.FromHandle( hwnd );
        }

        public void AddUnhandledClient( Client client , TagType tag )
        {
            UnhandledTag[ tag ].AddClient( client );
        }

        public List<Client> ClientList(string selectedTag)
        {
            return TagClientDic[ selectedTag ].ClientList;
        }

        public void AddTag( int id )
        {
            var temp = new TagManager( new List<Client>() , id );
            var temp2 = new TagManager( new List<Client>() , id );
            TagClientDic.Add( id.ToString( ) , temp );
            // こちらに足すよりclientにフラグ足す
            UnhandledTag.Add( id.ToString( ) , temp2 );
        }



        public void PaintIcon(List<ListBox> clientTitleList, PaintEventArgs e )
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
                    User32Methods.MoveWindow( client.Hwnd , 0 , y , 640 , 480 , true );
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

    }
}
