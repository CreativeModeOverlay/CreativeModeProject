using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace CreativeMode
{
    public class YoutubeDL
    {
        public string Path { get; set; } = "youtube-dl";

        public YoutubeDL() {}
        public YoutubeDL(string path)
        {
            Path = path;
        }
        
        public Response GetInfo(string url)
        {
            var youtubeDlProcess = Process.Start(new ProcessStartInfo(Path)
            {
                Arguments = $"--playlist-items 0 --dump-json \"{url}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            
            if(youtubeDlProcess == null)
                throw new InvalidOperationException("Cannot start youtube-dl");

            var mediaList = new List<Media>();

            while (!youtubeDlProcess.StandardOutput.EndOfStream)
            {
                var response = youtubeDlProcess.StandardOutput.ReadLine();
                
                if(response != null)
                    mediaList.Add(JsonConvert.DeserializeObject<Media>(response));
            }

            return new Response
            {
                sourceUrl = url,
                media = mediaList.ToArray()
            };
        }

        public class Response
        {
            public string sourceUrl;
            public Media[] media;
        }
        
        public class Media
        {
            [JsonProperty("id")] 
            public string id;

            [JsonProperty("title")] 
            public string title;

            [JsonProperty("uploader")] 
            public string author;

            [JsonProperty("webpage_url")] 
            public string originalUrl;

            [JsonProperty("thumbnails")] 
            public Thumbnail[] thumbnails;

            [JsonProperty("formats")] 
            public Format[] formats;

            [JsonProperty("requested_formats")] 
            public Format[] requestedFormats;

            [JsonProperty("url")] 
            public string url;

            [JsonProperty("protocol")] 
            public string protocol;

            [JsonProperty("upload_date")]
            public string uploadDate;
            
            [JsonProperty("track")] 
            public string track;
            
            [JsonProperty("artist")]
            public string artist;

            [JsonProperty("album")]
            public string album;

            [JsonProperty("release_year")] 
            public string releaseYear;

            [JsonProperty("duration")]
            public float? duration;

            [JsonProperty("extractor_key")]
            public string extractor;
        }

        public class Thumbnail
        {
            [JsonProperty("width")]
            public int width;

            [JsonProperty("height")] 
            public int height;

            [JsonProperty("url")] 
            public string url;
        }

        public class Format
        {
            [JsonProperty("ext")]
            public string ext;

            [JsonProperty("acodec")] 
            public string audioCodec;

            [JsonProperty("vcodec")] 
            public string videoCodec;

            [JsonProperty("width")] 
            public int? width;

            [JsonProperty("height")]
            public int? height;

            [JsonProperty("fps")] 
            public float? fps;

            [JsonProperty("url")] 
            public string url;

            [JsonProperty("abr")] 
            public float? audioBitrate;
            
            [JsonProperty("vbr")] 
            public float? videoBitrate;
            
            [JsonProperty("protocol")] 
            public string protocol;

            public bool HasAudio => audioBitrate != null || (audioCodec != null && audioCodec != "none");
            public bool HasVideo => videoBitrate != null || (videoCodec != null && videoCodec != "none");
        }
    }
}