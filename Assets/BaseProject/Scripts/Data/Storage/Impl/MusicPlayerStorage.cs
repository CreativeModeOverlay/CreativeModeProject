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
        public bool IsQueueEmpty => connection.Table<QueueEntryDB>().Count() == 0;
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

        public MusicPlayerStorage(SQLiteConnection c)
        {
            connection = c;
            connection.CreateTable<HistoryEntryDB>();
            connection.CreateTable<PlaylistEntryDB>();
            connection.CreateTable<QueueEntryDB>();
            connection.CreateTable<StorageStateDB>();
            connection.CreateTable<PlayedMusicEntryDB>();

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

        public void AddHistory(string url)
        {
            connection.Insert(new HistoryEntryDB { Url = url, Date = DateTime.Now});
        }

        public List<string> GetHistory(int offset, int maxCount)
        {
            return connection.Table<HistoryEntryDB>()
                .OrderByDescending(h => h.Id)
                .Skip(offset).Take(maxCount)
                .Select(i => i.Url).ToList();
        }

        public string GetHistory(int offset)
        {
            return connection.Table<HistoryEntryDB>()
                .Skip(offset).Take(1).FirstOrDefault()?.Url;
        }

        public void AddPlayedSong(string url)
        {
            connection.InsertOrReplace(new PlayedMusicEntryDB { Url = url , Date = DateTime.Now });
        }

        public List<string> GetPlayedSongs(TimeSpan duration)
        {
            var expirationDate = DateTime.Now - duration;
            
            return connection.Table<PlayedMusicEntryDB>()
                .Where(d => d.Date > expirationDate)
                .Select(i => i.Url).ToList();
        }

        public void RemoveFromPlayedSongs(List<string> songs)
        {
            var songSet = new HashSet<string>(songs);
            connection.Table<PlayedMusicEntryDB>()
                .Where(i => songSet.Contains(i.Url))
                .Delete();
        }

        public void ClearPlaylist()
        {
            connection.DeleteAll<PlaylistEntryDB>();
        }

        public void AddToPlaylist(ICollection<string> items)
        {
            connection.InsertAll(items.Select(url => new PlaylistEntryDB { Url = url }));
        }

        public List<string> GetPlaylist()
        {
            return connection.Table<PlaylistEntryDB>()
                .Select(e => e.Url)
                .ToList();
        }

        public void ClearQueue()
        {
            connection.DeleteAll<QueueEntryDB>();
        }

        public void AddToQueue(ICollection<string> items)
        {
            connection.InsertAll(items.Select(i => new QueueEntryDB {Url = i}));
        }

        public List<string> GetQueue(int offset = 0, int size = 25)
        {
            return connection.Table<QueueEntryDB>()
                .Skip(offset)
                .Take(size)
                .Select(q => q.Url)
                .ToList();
        }

        public string PopQueue()
        {
            try
            {
                connection.BeginTransaction();
                
                var result = connection
                    .Table<QueueEntryDB>()
                    .Take(1).FirstOrDefault();

                if (result == null) 
                    return null;

                connection.Delete(result);
                return result.Url;
            }
            finally
            {
                connection.Commit();
            }
        }
    }
}