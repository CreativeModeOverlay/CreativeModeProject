using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace CreativeMode.Impl
{
    public class MusicPlayerStorage : IMusicPlayerStorage
    {
        private readonly SQLiteConnection connection;
        private StorageState state;
        
        // ReSharper disable once UseMethodAny.2
        public bool IsQueueEmpty => connection.Table<QueueEntry>().Count() == 0;
        public int HistorySize => connection.Table<HistoryEntry>().Count();

        public int HistoryPosition
        {
            get => state.HistoryPosition;
            set
            {
                state.HistoryPosition = value;
                SaveState();
            }
        }

        public bool Shuffle
        {
            get => state.Shuffle;
            set
            {
                state.Shuffle = value;
                SaveState();
            }
        }

        public bool SkipRepeats
        {
            get => state.SkipRepeats;
            set
            {
                state.SkipRepeats = value;
                SaveState();
            }
        }

        public MusicPlayerStorage(SQLiteConnection c)
        {
            connection = c;
            connection.CreateTable<HistoryEntry>();
            connection.CreateTable<PlaylistEntry>();
            connection.CreateTable<QueueEntry>();
            connection.CreateTable<StorageState>();
            connection.CreateTable<PlayedMusicEntry>();

            state = connection.Table<StorageState>().FirstOrDefault() ?? new StorageState
            {
                HistoryPosition = -1,
                Shuffle = false,
                SkipRepeats = true
            };
        }

        private void SaveState()
        {
            connection.InsertOrReplace(state);
        }

        public void AddHistory(string url)
        {
            connection.Insert(new HistoryEntry { Url = url, Date = DateTime.Now});
        }

        public List<string> GetHistory(int offset, int maxCount)
        {
            return connection.Table<HistoryEntry>()
                .OrderByDescending(h => h.Id)
                .Skip(offset).Take(maxCount)
                .Select(i => i.Url).ToList();
        }

        public string GetHistory(int offset)
        {
            return connection.Table<HistoryEntry>()
                .Skip(offset).Take(1).FirstOrDefault()?.Url;
        }

        public void AddPlayedSong(string url)
        {
            connection.InsertOrReplace(new PlayedMusicEntry { Url = url , Date = DateTime.Now });
        }

        public List<string> GetPlayedSongs(TimeSpan duration)
        {
            var expirationDate = DateTime.Now - duration;
            
            return connection.Table<PlayedMusicEntry>()
                .Where(d => d.Date > expirationDate)
                .Select(i => i.Url).ToList();
        }

        public void RemoveFromPlayedSongs(List<string> songs)
        {
            var songSet = new HashSet<string>(songs);
            connection.Table<PlayedMusicEntry>()
                .Where(i => songSet.Contains(i.Url))
                .Delete();
        }

        public void ClearPlaylist()
        {
            connection.DeleteAll<PlaylistEntry>();
        }

        public void AddToPlaylist(ICollection<string> items)
        {
            connection.InsertAll(items.Select(url => new PlaylistEntry { Url = url }));
        }

        public List<string> GetPlaylist()
        {
            return connection.Table<PlaylistEntry>()
                .Select(e => e.Url)
                .ToList();
        }

        public void ClearQueue()
        {
            connection.DeleteAll<QueueEntry>();
        }

        public void AddToQueue(ICollection<string> items)
        {
            connection.InsertAll(items.Select(i => new QueueEntry {Url = i}));
        }

        public string PopQueue()
        {
            try
            {
                connection.BeginTransaction();
                
                var result = connection
                    .Table<QueueEntry>()
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

        private class MusicEntry
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Url { get; set; }
        }
        
        private class QueueEntry : MusicEntry { }
        private class PlaylistEntry : MusicEntry { }
        
        private class PlayedMusicEntry
        {
            [PrimaryKey]
            public string Url { get; set; }
            public DateTime Date { get; set; }
        }

        private class HistoryEntry : MusicEntry
        {
            public DateTime Date { get; set; }
        }
        
        private class StorageState
        {
            [PrimaryKey]
            public int Id { get; set; } = 0;
            public int HistoryPosition { get; set; }
            public bool Shuffle { get; set; }
            public bool SkipRepeats { get; set; }
        }
    }
}