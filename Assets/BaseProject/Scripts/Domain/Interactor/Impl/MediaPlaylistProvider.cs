using System;
using System.Collections.Generic;
using System.Linq;
using CreativeMode;
using CreativeMode.Impl;
using UniRx;
using UnityEngine;

public class MediaPlaylistProvider : MonoBehaviour, IMediaPlaylistProvider
{
    private IMusicPlayerStorage Storage => Instance<IMusicPlayerStorage>.Get();
    private IMusicPlayer Player => Instance<IMusicPlayer>.Get();
    
    public bool Shuffle
    {
        get => Storage.Shuffle;
        set => Storage.Shuffle = value;
    }
    
    public bool SkipRepeats
    {
        get => Storage.SkipRepeats;
        set => Storage.SkipRepeats = value;
    }

    public int CurrentSet
    {
        get => Storage.CurrentSetId;
        set => Storage.CurrentSetId = value;
    }

    private MediaMetadata currentMusicInfo;
    private String lastMarkedMusicUrl;

    private void Awake()
    {
        Player.CurrentMedia
            .Subscribe(m => currentMusicInfo = m)
            .AddTo(this);
    }

    private void Update()
    {
        // Пометить текущую песню как проигранную если играет хотя бы 5 секунд
        if (currentMusicInfo != null && currentMusicInfo.url != lastMarkedMusicUrl && Player.Position > 5)
        {
            lastMarkedMusicUrl = currentMusicInfo.url;
            Storage.AddPlayedMedia(currentMusicInfo.url);
        }
    }

    public void ClearPlaylist(int setId)
    {
        Storage.ClearPlaylist(setId);
    }

    public void AddToPlaylist(int setId, MediaMetadata url)
    {
        AddToPlaylist(setId, new [] { url });
    }

    public void AddToPlaylist(int setId, IEnumerable<MediaMetadata> urls)
    { 
        Storage.AddToPlaylist(urls
            .Select(u =>
            {
                var entry = ToDb(new PlaylistEntryDB(), u);
                entry.SetId = setId;
                return entry;
            })
            .ToList());
    }

    public void ResetQueueToPlaylist(int setId)
    {
        Storage.ClearQueue(setId);

        var playlist = Storage.GetPlaylist(setId);
            
        if(playlist.Count == 0)
            return;
            
        if (SkipRepeats)
        {
            var filteredPlaylist = new List<PlaylistEntryDB>(playlist.Count);
            var playedSongs = Storage.GetPlayedMedia(TimeSpan.FromDays(28));
            var playedSongsSet = new HashSet<string>(playedSongs);

            filteredPlaylist.AddRange(playlist.Where(song => !playedSongsSet.Contains(song.Url)));
                
            if (filteredPlaylist.Count == 0)
            {
                Storage.RemoveFromPlayedMedia(playlist.Select(e => e.Url));
                filteredPlaylist.AddRange(playlist);
                Debug.Log($"Playlist has been played, removing {playlist.Count} songs from repeat protection");
            }

            playlist = filteredPlaylist;
        }

        var mappedPlaylist = playlist.Select(FromDb);
        AddToQueue(setId, Shuffle ? mappedPlaylist.Shuffle() : mappedPlaylist);
    }

    public void AddToQueue(int setId, MediaMetadata url, int priority = 0)
    {
        AddToQueue(setId, new[] { url }, priority);
    }
        
    public void AddToQueue(int setId, IEnumerable<MediaMetadata> urls, int priority = 0)
    {
        Storage.AddToQueue(urls.Select(u =>
        {
            var entry = ToDb(new QueueEntryDB(), u);
            entry.SetId = setId;
            entry.Priority = priority;
            return entry;
        }).ToList());
    }

    public PlaylistSlice GetSlice(int setId, int maxPreviousCount, int maxNextCount)
    {
        var slice = new PlaylistSlice();

        if (maxPreviousCount > 0)
        {
            var historyPosition = Storage.HistoryPosition - 1;
            slice.previous = new List<MediaMetadata>();
            
            for (var i = 0; i < maxPreviousCount; i++)
            {
                var value = GetFromHistory(historyPosition--);
                
                if(value == null)
                    break;
                
                slice.previous.Add(value);
            }
        }

        if (maxNextCount > 0)
        {
            var historyCount = Mathf.Min(Storage.HistorySize - Storage.HistoryPosition, maxNextCount);
            var queueCount = maxNextCount - historyCount;
            
            slice.next = new List<MediaMetadata>();

            if (historyCount > 0)
            {
                var historyPosition = Storage.HistoryPosition + 1;
                
                for (var i = 0; i < historyCount; i++)
                {
                    slice.next.Add(GetFromHistory(historyPosition++));
                }
            }

            if (queueCount > 0)
            {
                slice.next.AddRange(Storage.GetQueue(setId, 0, queueCount).Select(FromDb));
            }
        }
        
        slice.current = GetCurrent();

        return slice;
    }

    public MediaMetadata AdvanceToNext(int setId)
    {
        if (Storage.HistoryPosition >= Storage.HistorySize - 1 || Storage.HistorySize == 0)
        {
            if (Storage.GetQueueSize(setId) == 0)
                OnQueueFinished(setId);

            // In case "OnQueueFinished" does not return new items
            if (Storage.GetQueueSize(setId) == 0)
                return null;

            var queueEntry = FromDb(Storage.PopQueue(setId));
            var historyEntry = ToDb(new HistoryEntryDB(), queueEntry);
            historyEntry.Date = DateTime.Now;

            Storage.AddHistory(historyEntry);
        }

        return AdvanceNextFromHistory();
    }

    public MediaMetadata ReturnToPrevious()
    {
        if (Storage.HistoryPosition <= 0 || Storage.HistorySize == 0)
            return null;

        return ReturnToPreviousFromHistory();
    }
    
    private MediaMetadata ReturnToPreviousFromHistory()
    {
        if (Storage.HistoryPosition > 0)
            Storage.HistoryPosition--;

        if (Storage.HistoryPosition < 0)
            Storage.HistoryPosition = 0;

        return GetCurrent();
    }

    public MediaMetadata AdvanceNextFromHistory()
    {
        if (Storage.HistoryPosition <= Storage.HistorySize - 1)
            Storage.HistoryPosition++;

        return GetCurrent();
    }

    private MediaMetadata GetCurrent()
    {
        return GetFromHistory(Storage.HistoryPosition);
    }

    private MediaMetadata GetFromHistory(int position)
    {
        return FromDb(Storage.GetHistoryAt(position));
    }

    private void OnQueueFinished(int setId)
    {
        Debug.Log("Music Queue Finished");
        ResetQueueToPlaylist(setId);
    }

    private static MediaMetadata FromDb(MediaEntryDB e)
    {
        return new MediaMetadata
        {
            url = e.Url,
            thumbnailUrl = e.ThumbnailUrl,
            source = e.Source,
            duration = e.Duration,
            
            title = e.Title,
            album = e.Album,
            artist = e.Artist,
            year = e.Year,
        };
    }
    
    private static T ToDb<T>(T instance, MediaMetadata meta) 
        where T : MediaEntryDB
    {
        instance.Url = meta.url;
        instance.ThumbnailUrl = meta.thumbnailUrl;
        instance.Source = meta.source;
        instance.Duration = meta.duration;
        
        instance.Title = meta.title;
        instance.Album = meta.album;
        instance.Artist = meta.artist;
        instance.Year = meta.year;
        return instance;
    }
}
