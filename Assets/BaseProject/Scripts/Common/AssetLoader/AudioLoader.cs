using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using NAudio.Wave;
using TagLib.Riff;
using UniRx;
using UnityEngine;
using Color = UnityEngine.Color;

namespace CreativeMode
{
    public class AudioLoader : AssetLoader<AudioAsset>
    {
        protected override IObservable<SharedAsset<AudioAsset>.IReferenceProvider> CreateAssetProvider(string url)
        {
            return GetAssetStream(url)
                .SelectMany(s =>
                {
                    var position = s.Position;

                    return LoadMetadata(url, s)
                        .Select(meta => {
                            s.Position = position;
                            var waveStream = new Mp3FileReader(s);
                            var sampleProvider = waveStream.ToSampleProvider();
                            var bytesPerSample = waveStream.WaveFormat.BitsPerSample / 8 *
                                                 waveStream.WaveFormat.Channels;
                            var lengthSamples = (int) (waveStream.Length / bytesPerSample);

                            return new LoadedAudioData
                            {
                                waveStream = waveStream,
                                sampleProvider = sampleProvider,
                                lengthSamples = lengthSamples,
                                audioData = meta
                            };
                        });
                })
                .SubscribeOn(Scheduler.ThreadPool)
                .ObserveOnMainThread()
                .Select(data =>
                {
                    var asset = new AudioAsset(
                        CreateAudioClip(url, data.waveStream, data.sampleProvider, data.lengthSamples),
                        data.audioData);

                    return SharedAsset<AudioAsset>.Create(asset, a => !a.Clip,
                        a => {
                            data.waveStream.Dispose();
                            UnityEngine.Object.Destroy(a.Clip);
                        });
                });
        }

        private IObservable<AudioMetadata> LoadMetadata(string url, Stream stream)
        {
            return LoadLyricsForUri(url)
                .Select(lyrics =>
                {
                    var tag = TagLib.File.Create(new TagLibStream(url, stream)).Tag;

                    return new AudioMetadata
                    {
                        url = url,
                        coverUrl = url,
                        album = tag.Album,
                        artist = string.IsNullOrWhiteSpace(tag.JoinedAlbumArtists)
                            ? tag.JoinedPerformers
                            : tag.JoinedAlbumArtists,
                        title = tag.Title,
                        year = tag.Year.ToString(),
                        lyrics = lyrics
                    };
                });

        }
        
        private static AudioClip CreateAudioClip(string uri, WaveStream waveStream, ISampleProvider sampleProvider, int lengthSamples)
        {
            var clip = AudioClip.Create(uri, lengthSamples, 
                sampleProvider.WaveFormat.Channels, 
                sampleProvider.WaveFormat.SampleRate, true,
                data =>
                {
                    sampleProvider.Read(data, 0, data.Length);
                },
                position => waveStream.Position = position);

            return clip;
        }

        private List<LyricsUrl> GetLyricsUrl(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            var isFile = uri.IsFile;
            var result = new List<LyricsUrl>();

            if (isFile)
            {
                var filePath = Uri.UnescapeDataString(uri.AbsolutePath);
                var directory = Directory.GetParent(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var lrcFiles = directory.GetFiles($"{fileName}*.lrc");

                foreach (var file in lrcFiles)
                {
                    var lrcFileName = Path.GetFileNameWithoutExtension(file.FullName);
                    var lrcName = lrcFileName.Substring(fileName.Length).Trim('_');
                    
                    result.Add(new LyricsUrl
                    {
                        url = file.FullName,
                        voice = lrcName
                    });
                }
            }
            
            return result;
        }

        private IObservable<SongLyrics[]> LoadLyricsForUri(string uri)
        {
            return Observable.Start(() => GetLyricsUrl(uri), Scheduler.ThreadPool)
                .SelectMany(l => l)
                .SelectMany(l => LoadLyrics(l.url, l.voice))
                .ToArray();
        }
        
        private IObservable<SongLyrics> LoadLyrics(string lyricsUri, string lyricName)
        {
            return GetAssetStream(lyricsUri, false)
                .Select(s =>
                {
                    using (s)
                    {
                        var result = new List<LyricLine>();
                        var reader = new StreamReader(s);

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var openBracket = line.IndexOf('[');
                            var closeBracket = line.IndexOf(']');
                        
                            if(closeBracket <= openBracket)
                                continue;
                        
                            var tagContents = line
                                .Substring(openBracket + 1, closeBracket - openBracket - 1)
                                .Split(':');

                            if (tagContents.Length == 2
                                && float.TryParse(tagContents[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var minutes)
                                && float.TryParse(tagContents[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
                            {
                                var text = ParseLyricLine(line.Substring(closeBracket + 1).Trim());

                                result.Add(new LyricLine
                                {
                                    text = text.text,
                                    startTime = minutes * 60 + seconds,
                                    endTime = float.MaxValue,
                                    font = text.font,
                                    color = text.color,
                                    position = text.position,
                                });
                            }
                        }
                        
                        result.Sort((l, r) => l.startTime.CompareTo(r.startTime));

                        if (result.Count > 0)
                        {
                            for (var i = 1; i < result.Count; i++)
                            {
                                var current = result[i - 1];
                                var next = result[i];

                                current.endTime = next.startTime;
                                result[i - 1] = current;
                            }
                        }

                        return new SongLyrics
                        {
                            voice = lyricName,
                            lines = result.ToArray()
                        };
                    }
                });
        }

        private TextData ParseLyricLine(string text)
        {
            var result = new TextData
            {
                text = text
            };
            
            var openBracket = text.IndexOf("#{");

            if (openBracket == -1)
                return result;
            
            var closeBracket = text.IndexOf('}', openBracket);

            if (closeBracket == -1)
                return result;
            
            var tags = text
                .Substring(openBracket + 2, closeBracket - openBracket - 2)
                .Split(',');

            foreach (var tag in tags)
            {
                var type = tag[0];
                var data = tag.Substring(2);

                switch (type)
                {
                    // font
                    case 'f': 
                        result.font = data; 
                        break;
                            
                    // color
                    case 'c':
                        var color = ColorTranslator.FromHtml(data);
                        result.color = new Color32(color.R, color.G, color.B, color.A);
                        break;
                            
                    // position
                    case 'p':
                        switch (data)
                        {
                            case "left": result.position = LyricLine.Position.Left; break;
                            case "center": result.position = LyricLine.Position.Center; break;
                            case "right": result.position = LyricLine.Position.Right; break;
                        }
                        break;
                }
            }

            result.text = text.Substring(closeBracket + 1).Trim();

            return result;
        }
        
        private struct LyricsUrl
        {
            public string url;
            public string voice;
        }

        private struct TextData
        {
            public string text;
            public string font;
            public LyricLine.Position? position;
            public Color? color;
        }

        private class LoadedAudioData
        {
            public Stream stream;
            public WaveStream waveStream;
            public ISampleProvider sampleProvider;
            public AudioMetadata audioData;
            public int lengthSamples;
        }
    }
}