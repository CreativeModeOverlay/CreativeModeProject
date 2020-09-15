﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using CreativeMode;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

public class MusicPlaylistProvider : MonoBehaviour, IMusicPlaylistProvider
{
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
    
    private IMusicPlayerStorage Storage => Instance<IMusicPlayerStorage>.Get();
    private IMusicPlayer Player => Instance<IMusicPlayer>.Get();
    
    private AudioMetadata currentMusicInfo;
    private String lastMarkedMusicUrl;

    private void Awake()
    {
        Player.CurrentMusic
            .Subscribe(m => currentMusicInfo = m)
            .AddTo(this);
    }

    private void Update()
    {
        // Пометить текущую песню как проигранную если играет хотя бы 5 секунд
        if (currentMusicInfo.url != lastMarkedMusicUrl && currentMusicInfo.url != null
            && Player.Position > 5)
        {
            lastMarkedMusicUrl = currentMusicInfo.url;
            Storage.AddPlayedSong(currentMusicInfo.url);
        }
    }

    public void ClearPlaylist()
    {
        Storage.ClearPlaylist();
    }

    public void AddToPlaylist(string url)
    {
        AddToPlaylist(new [] { url });
    }

    public void AddToPlaylist(ICollection<string> urls)
    { 
        Storage.AddToPlaylist(urls);
    }

    public void ResetQueueToPlaylist()
    {
        Storage.ClearQueue();

        var playlist = Storage.GetPlaylist();
            
        if(playlist.Count == 0)
            return;
            
        if (SkipRepeats)
        {
            var filteredPlaylist = new List<string>(playlist.Count);
            var playedSongs = Storage.GetPlayedSongs(TimeSpan.FromDays(28));
            var playedSongsSet = new HashSet<string>(playedSongs);

            filteredPlaylist.AddRange(playlist.Where(song => !playedSongsSet.Contains(song)));
                
            if (filteredPlaylist.Count == 0)
            {
                Storage.RemoveFromPlayedSongs(playlist);
                filteredPlaylist.AddRange(playlist);
                Debug.Log($"Playlist has been played, removing {playlist.Count} songs from repeat protection");
            }

            playlist = filteredPlaylist;
        }

        AddToQueue(Shuffle ? playlist.Shuffle() : playlist);
    }

    public void AddToQueue(string url)
    {
        AddToQueue(new[] { url });
    }
        
    public void AddToQueue(ICollection<string> urls)
    {
        Storage.AddToQueue(urls);
    }
    
    [CanBeNull]
    public string AdvanceToNext()
    {
        if (Storage.HistoryPosition >= Storage.HistorySize - 1 || Storage.HistorySize == 0)
        {
            if (Storage.IsQueueEmpty)
                OnQueueFinished();

            if (Storage.IsQueueEmpty)
            {
                return null;
            }
            
            Storage.AddHistory(Storage.PopQueue());
        }

        return GetNextFromHistory();
    }

    public string ReturnToPrevious()
    {
        if (Storage.HistoryPosition <= 0 || Storage.HistorySize == 0)
        {
            return null;
        }

        return GetPreviousFromHistory();
    }
    
    private string GetPreviousFromHistory()
    {
        if (Storage.HistoryPosition > 0)
            Storage.HistoryPosition--;

        if (Storage.HistoryPosition < 0)
            Storage.HistoryPosition = 0;

        return GetCurrentFromHistory();
    }

    public string GetNextFromHistory()
    {
        if (Storage.HistoryPosition <= Storage.HistorySize - 1)
            Storage.HistoryPosition++;

        return GetCurrentFromHistory();
    }

    private string GetCurrentFromHistory()
    {
        return Storage.GetHistory(Storage.HistoryPosition);
    }

    private void OnQueueFinished()
    {
        Debug.Log("Music Queue Finished");
        ResetQueueToPlaylist();
    }
}
