using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace CreativeMode.Impl
{
    public class MusicPlayerStorage : IMusicPlayerStorage
    {
        private readonly SQLiteConnection connection;
        private StorageStateDB stateDB;
        
        // ReSharper disable once UseMethodAny.2
        public int HistorySize => connection.Table<HistoryEntryDB>().Count();

        public int HistoryPosition
        {
            get => stateDB.HistoryPosition;
            set
            {
                stateDB.HistoryPosition = value;
                SaveState();
            }
        }

        public bool Shuffle
        {
            get => stateDB.Shuffle;
            set
            {
                stateDB.Shuffle = value;
                SaveState();
            }
        }

        public bool SkipRepeats
        {
            get => stateDB.SkipRepeats;
            set
            {
                stateDB.SkipRepeats = value;
                SaveState();
            }
        }

        public int CurrentSetId
        {
            get => stateDB.CurrentSetId;
            set
            {
                stateDB.CurrentSetId = value;
                SaveState();
            }
        }

        public MusicPlayerStorage(SQLiteConnection c)
        {
            connection = c;
            connection.CreateTable<HistoryEntryDB>();
            connection.CreateTable<PlaylistEntryDB>();
            connection.CreateTable<QueueEntryDB>();
            connection.CreateTable<StorageStateDB>();
            connection.CreateTable<PlayedMediaEntryDB>();

            stateDB = connection.Table<StorageStateDB>().FirstOrDefault() ?? new StorageStateDB
            {
                HistoryPosition = -1,
                Shuffle = false,
                SkipRepeats = true
            };
        }

        private void SaveState()
        {
            connection.InsertOrReplace(stateDB);
        }

        public void AddHistory(HistoryEntryDB entry)
        {
            connection.Insert(entry);
        }

        public List<HistoryEntryDB> GetHistory(int offset, int maxCount)
        {
            return connection.Table<HistoryEntryDB>()
                .OrderByDescending(h => h.Id)
                .Skip(offset).Take(maxCount)
                .ToList();
        }

        public HistoryEntryDB GetHistoryAt(int offset)
        {
            return connection.Table<HistoryEntryDB>()
                .Skip(offset).Take(1).FirstOrDefault();
        }

        public void AddPlayedMedia(string url)
        {
            connection.InsertOrReplace(new PlayedMediaEntryDB { Url = url , Date = DateTime.Now });
        }

        public List<string> GetPlayedMedia(TimeSpan duration)
        {
            var expirationDate = DateTime.Now - duration;
            
            return connection.Table<PlayedMediaEntryDB>()
                .Where(d => d.Date > expirationDate)
                .Select(i => i.Url).ToList();
        }

        public void RemoveFromPlayedMedia(IEnumerable<string> songs)
        {
            var songSet = new HashSet<string>(songs);
            connection.Table<PlayedMediaEntryDB>()
                .Where(i => songSet.Contains(i.Url))
                .Delete();
        }

        public void ClearPlaylist(int setId)
        {
            connection.Table<PlaylistEntryDB>()
                .Delete(e => e.SetId == setId);
        }

        public void AddToPlaylist(IEnumerable<PlaylistEntryDB> items)
        {
            connection.InsertAll(items);
        }

        public List<PlaylistEntryDB> GetPlaylist(int setId)
        {
            return connection.Table<PlaylistEntryDB>()
                .Where(e => e.SetId == setId)
                .ToList();
        }

        public void ClearQueue(int setId)
        {
            connection.Table<QueueEntryDB>()
                .Delete(e => e.SetId == setId);
        }

        public void AddToQueue(IEnumerable<QueueEntryDB> items)
        {
            connection.InsertAll(items);
        }

        public List<QueueEntryDB> GetQueue(int setId, int offset = 0, int size = 25)
        {
            return connection.Table<QueueEntryDB>()
                .Skip(offset)
                .Take(size)
                .OrderBy(e => e.Priority)
                .ToList();
        }

        public QueueEntryDB PopQueue(int setId)
        {
            try
            {
                connection.BeginTransaction();
                
                var result = PeekQueue(setId);

                if (result == null) 
                    return null;

                connection.Delete(result);
                return result;
            }
            finally
            {
                connection.Commit();
            }
        }

        public QueueEntryDB PeekQueue(int setId)
        {
            return connection
                .Table<QueueEntryDB>()
                .Take(1)
                .OrderBy(e => e.Priority)
                .FirstOrDefault(e => e.SetId == setId);
        }

        public int GetQueueSize(int setId)
        {
            return connection.Table<QueueEntryDB>().Count(e => e.SetId == setId);
        }
    }
}