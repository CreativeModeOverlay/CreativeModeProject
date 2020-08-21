using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using UniRx;

namespace CreativeMode.Impl
{
    public class NoteStorage : INoteStorage
    {
        private ItemWatcher<int> noteWatcher = new ItemWatcher<int>();
        
        private SQLiteConnection connection;

        public NoteStorage(SQLiteConnection connection)
        {
            this.connection = connection;

            connection.CreateTable<NoteTable>();
            connection.CreateTable<PageTable>();
        }

        public void PutNote(NoteDB note)
        {
            var n = new NoteTable { Id = note.id, Title = note.title};
            connection.InsertOrReplace(n);
            
            var noteId = n.Id;
            connection.Table<PageTable>().Where(p => p.NoteId == noteId).Delete();
            connection.InsertAll(note.pages.Select(p => new PageTable { NoteId = noteId, Text = p}));
            
            noteWatcher.NotifyUpdate(noteId);
        }

        public void DeleteNote(int id)
        {
            connection.Delete<NoteTable>(id);
            connection.Table<PageTable>().Where(p => p.NoteId == id).Delete();
            noteWatcher.NotifyDelete(id);
        }

        public IObservable<KeyValuePair<int, string>[]> GetAllNotes()
        {
            return noteWatcher.EveryUpdate()
                .Select(_ => connection.Table<NoteTable>()
                    .Select(n => new KeyValuePair<int, string>(n.Id, n.Title))
                    .ToArray());
        }

        public IObservable<NoteDB> GetNote(int id)
        {
            return noteWatcher.EveryUpdate(id)
                .Select(_ =>
                { 
                    var note = connection
                        .Table<NoteTable>()
                        .First(n => n.Id == id);

                    var pages = connection
                        .Table<PageTable>()
                        .Where(p => p.NoteId == id)
                        .Select(p => p.Text)
                        .ToArray();
                    
                    return new NoteDB
                    {
                        id = id,
                        title = note.Title,
                        pages = pages
                    };
                });
        }
        
        private class NoteTable
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Title { get; set; }
        }
        
        private class PageTable
        {
            public int NoteId { get; set; }
            public string Text { get; set; }
        }
    }
}