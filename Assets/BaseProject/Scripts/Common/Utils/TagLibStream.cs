using System;
using System.IO;

namespace CreativeMode
{
    public class TagLibStream : TagLib.File.IFileAbstraction
    {
        public string Name { get; }
        public Stream ReadStream { get; }
        public Stream WriteStream => throw new  NotSupportedException();
        
        public TagLibStream(string name, Stream stream)
        {
            ReadStream = stream;
            Name = name;
        }
        
        public void CloseStream(Stream stream)
        {
            
        }
    }
}