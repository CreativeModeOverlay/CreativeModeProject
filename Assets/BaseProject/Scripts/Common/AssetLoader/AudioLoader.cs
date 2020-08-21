using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using NAudio.Wave;
using UniRx;
using UnityEngine;
using Color = UnityEngine.Color;

namespace CreativeMode
{
    public class AudioLoader : AssetLoader<AudioAsset>
    {
        public static readonly string[] SupportedExtensions = 
        {
            "mp3"
        };
        
        protected override IObservable<SharedAsset<AudioAsset>.IReferenceProvider> CreateAssetProvider(Stream stream, string url)
        {
            var beginning = stream.Position;

            return Observable.Start(() =>
                {
                    // TODO: detect file format
                    
                    stream.Position = beginning;
                    var tags = TagLib.File.Create(new TagLibStream("audio.mp3", stream));
                    stream.Position = beginning;

                    var waveStream = new Mp3FileReader(stream);
                    var sampleProvider = waveStream.ToSampleProvider();
                    var bytesPerSample = waveStream.WaveFormat.BitsPerSample / 8 * waveStream.WaveFormat.Channels;
                    var lengthSamples = (int) (waveStream.Length / bytesPerSample);
                    var tag = tags.Tag;

                    return new LoadedAudioData
                    {
                        waveStream = waveStream,
                        sampleProvider = sampleProvider,
                        lengthSamples = lengthSamples,
                        audioData = new AudioMetadata
                        {
                            url = url,
                            coverUrl = url,
                            album = tag.Album,
                            artist = string.IsNullOrWhiteSpace(tag.JoinedAlbumArtists) 
                                ? tag.JoinedPerformers 
                                : tag.JoinedAlbumArtists,
                            title = tag.Title,
                            year = tag.Year.ToString(),
                        }
                    };
                })
                .SelectMany(d => GetFileLyrics(url)
                    .Catch<LyricLine[], Exception>(e => Observable.Return(new LyricLine[0]))
                    .Select(l => 
                    { 
                        d.audioData.lyrics = l; 
                        return d;
                    }))
                .SubscribeOn(Scheduler.ThreadPool)
                .ObserveOnMainThread()
                .Select(data =>
                {
                    var asset = new AudioAsset(
                        LoadAudioClip(data.waveStream, data.sampleProvider, data.lengthSamples),
                        data.audioData);

                    return SharedAsset<AudioAsset>.Create(asset, a => !a.Clip,
                        a => {
                            data.waveStream.Dispose();
                            stream.Dispose();
                            UnityEngine.Object.Destroy(a.Clip);
                        });
                });
        }
        
        private static AudioClip LoadAudioClip(WaveStream waveStream, ISampleProvider sampleProvider, int lengthSamples)
        {
            var clip = AudioClip.Create("SampledAudio", lengthSamples, sampleProvider.WaveFormat.Channels,
                sampleProvider.WaveFormat.SampleRate, true,
                data =>
                {
                    sampleProvider.Read(data, 0, data.Length);
                },
                position => waveStream.Position = position);

            return clip;
        }

        private string GetLyricsUrl(string fileUrl)
        {
            foreach (var ext in SupportedExtensions)
            {
                if (fileUrl.EndsWith(ext))
                    return fileUrl.Substring(0, fileUrl.Length - ext.Length) + "lrc";
            }

            return fileUrl + ".lrc";
        }
        
        private IObservable<LyricLine[]> GetFileLyrics(string originalFileUri)
        {
            return GetAssetStream(GetLyricsUrl(originalFileUri))
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
                                var text = GetTextData(line.Substring(closeBracket + 1).Trim());

                                result.Add(new LyricLine
                                {
                                    text = text.text,
                                    startTime = minutes * 60 + seconds,
                                    endTime = float.MaxValue,
                                    font = text.font,
                                    color = text.color,
                                    position = text.position,
                                    voice = text.voice
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

                        return result.ToArray();
                    }
                });
        }

        private TextData GetTextData(string text)
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
                    
                    case 'v':
                        result.voice = data;
                        break;
                }
            }

            result.text = text.Substring(closeBracket + 1).Trim();

            return result;
        }

        private struct TextData
        {
            public string text;
            public string font;
            public LyricLine.Position? position;
            public Color? color;
            public string voice;
        }

        private class LoadedAudioData
        {
            public WaveStream waveStream;
            public ISampleProvider sampleProvider;
            public AudioMetadata audioData;
            public int lengthSamples;
        }
    }
}