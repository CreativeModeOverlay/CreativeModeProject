using System;
using System.Collections.Generic;

namespace CreativeMode
{
    public interface INoteStorage
    {
        void PutNote(NoteDB note);
        void DeleteNote(int id);

        IObservable<KeyValuePair<int, string>[]> GetAllNotes();
        IObservable<NoteDB> GetNote(int id);
    }
}