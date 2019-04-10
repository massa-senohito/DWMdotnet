using Autofac.Extras.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TagType = System.String;

namespace TileManTest
{
    [Intercept( typeof(CallLogger))]
    public interface IScreenWorld
    {
        IEnumerable<TagManager> TagList
        {
            get;
        }

        void AddTag( int id );
        void Attach( Types.Client client , string dest );
        IEnumerable<Types.Client> MovedClients( TagType SelectedTag );
        void ChangeTag( string currentTag , string itemID );
        void ChangeTag( TagManager currentTag , string itemID );
        void CleanUp();
        List<Types.Client> ClientList( string selectedTag );
        void ForeachClient( Predicate<Types.Client> action );
        void ForeachTagManager( Action<TagManager> action );
        bool IsContainScreen( IntPtr hwnd );
        bool IsSameScreen( Screen screen );
        void PaintIcon( List<ListBox> clientTitleList , PaintEventArgs e );
        void SetAllWindowFore();
        TagManager Tag( string id );
        void Tile( string selectedTag , int UIHeight );
        void Dump();
        string ToString();
    }
}