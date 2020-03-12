using Autofac.Extras.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static Types;
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

        int TagCount
        {
            get;
        }

        void AddTag( int id );
        void Attach( Client client , string dest );
        IEnumerable<Client> MovedClients( TagType SelectedTag );
        void ChangeTag( string currentTag , string itemID );
        void ChangeTag( TagManager currentTag , string itemID );
        void CleanUp();
        List<Client> ClientList( string selectedTag );
        void ForeachClient( Predicate<Client> action );
        void ForeachTagManager( Action<TagManager> action );
        bool IsContainScreen( IntPtr hwnd );
        bool IsSameScreen( Screen screen );
        void PaintIcon( List<ListBox> clientTitleList , PaintEventArgs e , TagType SelectedTag );
        void SetAllWindowFore();
        void SortMaster( Client NextMaster , TagType SelectedTag );
        TagManager Tag( string id );
        void Tile( string selectedTag , int UIHeight );
        void UpdateScreen( TagType SelectedTag );
        void Dump();
        string ToString();
    }
}