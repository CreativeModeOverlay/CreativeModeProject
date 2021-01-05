using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using UniRx;
using UnityEngine;
using ATL;
using Color = UnityEngine.Color;

namespace CreativeMode.Impl
{
    public class MediaProvider : IMediaProvider
    {
        private static readonly string DefaultVoiceName = "Lyrics";
        
        private CacheDictionary<string, YoutubeDL.Response> youtubeDLResponseCache =
            new CacheDictionary<string, YoutubeDL.Response>(TimeSpan.FromMinutes(15));
        
        private readonly YoutubeDL youtubeDL;

        public MediaProvider(YoutubeDL youtubeDL)
        {
            this.youtubeDL = youtubeDL;
        }
        
        public IObservable<List<MediaInfo>> GetMediaByUrl(string url, 
            int maxWidth = Int32.MaxValue, 
            int maxHeight = Int32.MaxValue,
            bool preferAudioOnly = false)
        {
            if (IsYoutubeDlLink(url))
                return ResolveMediaUsingYoutubeDL(url, maxWidth, maxHeight, preferAudioOnly);

            return ResolveMediaUsingLocalPath(url);
        }

        public IObservable<List<SongLyrics>> GetMediaLyrics(string url)
        {
            return LoadLyricsForUri(url);
        }

        public void Prefetch(string url)
        {
            if (IsYoutubeDlLink(url)) 
                ResolveMediaUsingYoutubeDL(url).Subscribe();
        }

        private bool IsYoutubeDlLink(string url)
        {
            return url.StartsWith("http") || url.StartsWith("https");
        }

        private IObservable<List<MediaInfo>> ResolveMediaUsingLocalPath(string path)
        {
            return Observable.Start(() => GetMediaFromPath(path).ToList(), Scheduler.ThreadPool);
        }

        private IEnumerable<MediaInfo> GetMediaFromPath(string path)
        {
            var pathAttrs = File.GetAttributes(path);

            if ((pathAttrs & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return Directory.EnumerateFiles(path)
                    .SelectMany(GetMediaFromPath)
                    .ToList();
            }

            return GetMediaFromFile(path);
        }

        private List<MediaInfo> GetMediaFromFile(string filePath)
        {
            var track = new Track(filePath);

            return new List<MediaInfo>
            {
                new MediaInfo
                {
                    sourceUrl = filePath,
                    streamUrl = filePath,
                    thumbnailUrl = filePath,
                    audioStreamUrl = null,
                    
                    album = track.Album,
                    artist = string.IsNullOrWhiteSpace(track.AlbumArtist)
                        ? track.AlbumArtist
                        : track.Artist,
                    title = track.Title,
                    year = track.Year.ToString(),
                    duration = TimeSpan.FromMilliseconds(track.DurationMs),
                    source = ""
                }
            };
        }

        private IObservable<List<MediaInfo>> ResolveMediaUsingYoutubeDL(string url,
            int maxHeight = Int32.MaxValue,
            int maxWidth = Int32.MaxValue,
            bool preferAudioOnly = false)
        {
            return Observable.Start(() =>
            {
                if (!youtubeDLResponseCache.Get(url, out var mediaInfo))
                {
                    mediaInfo = youtubeDL.GetInfo(url);
                    youtubeDLResponseCache.Put(url, mediaInfo);
                }

                var result = new List<MediaInfo>();

                foreach (var media in mediaInfo.media)
                {
                    var urls = GetYoutubeDLMediaUrl(media, maxHeight, maxWidth, preferAudioOnly);
                    
                    result.Add(new MediaInfo
                    {
                        sourceUrl = media.originalUrl,
                        thumbnailUrl = GetYoutubeDLThumbnailUrl(media),
                        streamUrl = urls.contentUrl,
                        audioStreamUrl = urls.audioTrackUrl,
                        
                        title = media.track ?? media.title,
                        artist = media.artist ?? media.author,
                        album = media.album,
                        year = media.releaseYear ?? GetYoutubeDLUploadYear(media.uploadDate),
                        duration = TimeSpan.FromSeconds(media.duration ?? 0),
                        source = media.extractor
                    });
                }
                
                return result;
            }, Scheduler.ThreadPool);
        }
        
        public static bool IsProtocolSupported(string protocol)
        {
            return protocol != "m3u8_native" && 
                   protocol != "http_dash_segments";
        }

        private static string GetYoutubeDLThumbnailUrl(YoutubeDL.Media media)
        {
            return media.thumbnails
                .Where(t => !t.url.Contains(".webp"))
                .OrderByDescending(t => t.width * t.height)
                .First()?.url;
        }

        private static string GetYoutubeDLUploadYear(string year)
        {
            if (DateTime.TryParseExact(year, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var result))
                return result.Year.ToString();

            return year;
        }
        
        private static YoutubeDL.Format GetYoutubeDLBestAudioOnlyFormat(YoutubeDL.Format[] formats)
        {
            return formats
                .Where(f => !f.HasVideo && f.HasAudio && IsProtocolSupported(f.protocol))
                .OrderByDescending(m => m.audioBitrate ?? 0)
                .FirstOrDefault();
        }

        private static YoutubeDL.Format GetYoutubeDLBestVideoFormat(YoutubeDL.Format[] formats, 
            int maxHeight = int.MaxValue, 
            int maxWidth = int.MaxValue)
        {
            return formats.Where(f => f.HasVideo && f.width <= maxWidth && f.height <= maxHeight && IsProtocolSupported(f.protocol))
                .OrderByDescending(f => f.width * f.height)
                .FirstOrDefault();
        }

        private static MediaUrl GetYoutubeDLMediaUrl(YoutubeDL.Media media, 
            int maxHeight = int.MaxValue, 
            int maxWidth = int.MaxValue,
            bool preferAudioOnly = false)
        {
            if (media.formats == null)
            {
                if(media.url != null) // only format
                    return new MediaUrl { contentUrl = media.url };
                
                throw new InvalidOperationException("Media without formats and url");
            }
            
            var audioOnlyStream = GetYoutubeDLBestAudioOnlyFormat(media.formats);

            if (!preferAudioOnly)
            {
                var videoStream = GetYoutubeDLBestVideoFormat(media.formats, maxHeight, maxWidth);

                if (videoStream != null)
                {
                    var vlcMedia = new MediaUrl { contentUrl = videoStream.url };

                    if (!videoStream.HasAudio && audioOnlyStream != null)
                        vlcMedia.audioTrackUrl = audioOnlyStream.url;

                    return vlcMedia;
                }
            }

            if(audioOnlyStream != null)
                return new MediaUrl { contentUrl = audioOnlyStream.url };
            
            throw new InvalidOperationException("No video or audio found");
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
                    var lrcName = lrcFileName.Substring(fileName.Length).Trim().Trim('#');
                    
                    result.Add(new LyricsUrl
                    {
                        url = file.FullName,
                        voice = string.IsNullOrWhiteSpace(lrcName) ? DefaultVoiceName : lrcName
                    });
                }
            }
            
            return result;
        }

        private IObservable<List<SongLyrics>> LoadLyricsForUri(string uri)
        {
            return Observable.Start(() => GetLyricsUrl(uri), Scheduler.ThreadPool)
                .SelectMany(l => l)
                .SelectMany(l => LoadLyrics(l.url, l.voice))
                .ToList() as IObservable<List<SongLyrics>>;
        }

        private IObservable<Stream> GetAssetStream(string url, bool requireSeek)
        {
            return Observable.Start(() =>
            {
                var stream = WebRequest.Create(url)
                    .GetResponse()
                    .GetResponseStream();

                if (requireSeek && !stream.CanSeek)
                {
                    using (stream)
                    {
                        var memoryStream = new MemoryStream((int) stream.Length);
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        return memoryStream;
                    }
                }

                return stream;
            }, Scheduler.ThreadPool);
        }
        
        private IObservable<SongLyrics> LoadLyrics(string lyricsUri, string lyricName)
        {
            return GetAssetStream(lyricsUri, false)
                .Select(s =>
                {
                    using (s)
                    {
                        var result = new List<SongLyrics.Line>();
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

                                result.Add(new SongLyrics.Line()
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
                            case "left": result.position = SongLyrics.Line.Position.Left; break;
                            case "center": result.position = SongLyrics.Line.Position.Center; break;
                            case "right": result.position = SongLyrics.Line.Position.Right; break;
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
            public SongLyrics.Line.Position? position;
            public Color? color;
        }
        
        private struct MediaUrl
        {
            public string contentUrl;
            public string audioTrackUrl;
        }
    }
}