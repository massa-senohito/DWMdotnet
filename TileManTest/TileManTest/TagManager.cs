using DWMDotnet;
using MouseCaptureTest;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Types;

namespace TileManTest
{
    public class TagManager
    {
        public static int DefaultMasterWidth = 1056;
        DebugLogger Logger;

        public TagManager( List<Client> clientlist , int id )
        {
            ClientList = clientlist;
            Id = id;
            ClientsAreVisible = true;
            IconList = new List<Icon>( );
            MasterWidth = DefaultMasterWidth;
            Logger = new DebugLogger( "TagManager " + id.ToString() );
        }

        public List<Client> ClientList
        {
            get;
            private set;
        }

        public List<Icon> IconList
        {
            get;
            private set;
        }

        public int Id
        {
            get;
            private set;
        }

        public int _MasterWidth;

        public int MasterWidth
        {
            get
            {
                return _MasterWidth;
            }
            set
            {
                _MasterWidth = value;
            }
        }



        public bool ClientsAreVisible
        {
            get;
            private set;
        }

        public void Visible(bool visible)
        {
            foreach ( var item in ClientList )
            {
                DWM.setClientVisibility( item , visible);
            }
            ClientsAreVisible = visible;
        }

        public void DumpIcon()
        {
            for ( int i = 0 ; i < IconList.Count ; i++ )
            {
                var icon = IconList[ i ];
                using ( FileStream file = new FileStream( $"{ClientList[ i ].Title}.bmp" , FileMode.OpenOrCreate ) )
                {
                    icon.Save( file );
                }
            }

        }

        public Client TryGetClient( IntPtr hwnd )
        {
            var foundItem = ClientList.FirstOrDefault( c => c.Hwnd == hwnd );
            if ( foundItem != null )
            {
                return foundItem;
            }
            return null;
        }

        public void AddClient( Client c )
        {
            Logger.Info( $"add {c.Title}" );
            ClientList.Add( c );
            Icon item = Win32dll.GetAppIcon( c.Hwnd );
            IconList.Add( item );
            Logger.Warn( $"all Client {ClientTitles.ToJson( ) }" );
        }

        public void ResetIcon()
        {
            IconList.Clear( );
            foreach ( var c in ClientList )
            {
                Icon item = Win32dll.GetAppIcon( c.Hwnd );
                IconList.Add( item );
            }
        }

        public bool HasClient( IntPtr hwnd )
        {
            return ClientList.Any( c => c.Hwnd == hwnd );
        }

        public bool HasClient( Client client)
        {
            return ClientList.Any( c => c.Hwnd == client.Hwnd );
        }

        public bool RemoveClient( Client client )
        {
            var mayInd = ClientList.FindIndex( c => c.Hwnd == client.Hwnd );
            if ( mayInd != -1 )
            {
                // アイコンが生成されないとインデックスが不一致になる
                Logger.Info( $"removed {client.Title}" );
                ClientList.RemoveAt( mayInd );
                var icon = IconList[ mayInd ];
                RemoveIcon( mayInd );
                icon?.Dispose( );
                Logger.Warn( $"all Client {ClientTitles.ToJson( ) }" );
                return true;
            }
            return false;
        }

        public void SortMaster( Client nextMaster )
        {
            if ( !HasClient( nextMaster ) )
            {
                return;
            }
            var index = ClientList.IndexOf( nextMaster );

            ClientList.RemoveAt( index );
            var oldMaster = ClientList[ 0 ];
            ClientList.RemoveAt( 0 );
            ClientList.Insert( 0 , nextMaster );
            ClientList.Insert( index , oldMaster );
        }

        public void RemoveIcon( int ind )
        {
            Logger.Info( $"removedIcon {ind}" );
            IconList.RemoveAt( ind );
        }

        public IEnumerable<string> ClientTitles
        {
            get
            {
                foreach ( var item in ClientList )
                {
                    yield return item.Title;
                }
            }
        }

    } // class


} // namespace
