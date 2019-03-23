using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Whitebell.Library.Collections.Generic;
using static Types;
using TagType = System.String;

namespace TileManTest
{
    class ScreenWorld
    {
        Screen _Screen;
        int AdjacentScrenOffset;
        TagManager UnhandledTag;
        // コピーならスキャン後やムーブ後
        OrderedDictionary<TagType , TagManager> TagClientDic = new OrderedDictionary<TagType, TagManager>( );

        public ScreenWorld( Screen screen )
        {
            _Screen = screen;
            AdjacentScrenOffset = screen.Bounds.Left;
            UnhandledTag = new TagManager( new List<Types.Client>( ) , 20 );
        }

        public bool IsContainScreen( IntPtr hwnd )
        {
            return _Screen == Screen.FromHandle( hwnd );
        }

        public void AddUnhandledClient( Types.Client client )
        {
            UnhandledTag.AddClient( client );
        }

        public List<Client> ClientList(string selectedTag)
        {
            return TagClientDic[ selectedTag ].ClientList;
        }

        public void AddTag( int id , TagManager tag )
        {
            TagClientDic.Add( id.ToString( ) , tag );
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

        public void ChangeTag( TagManager currentTag , string itemID )
        {
            //if ( itemID == SelectedTag )
            //{
            //    return;
            //}
            //ActiveClient = null;
            //currentTag.Visible( false );
            //TagClientDic[ itemID ].Visible( true );
            //SelectedTag = itemID;
            //Tile( );
        }
    }
}
