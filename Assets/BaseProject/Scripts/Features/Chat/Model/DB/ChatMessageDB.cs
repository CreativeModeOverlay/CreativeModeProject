using System;
using SQLite;

namespace CreativeMode
{
    internal class ChatMessageDB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        
        public string AuthorName { get; set; }
        public string AuthorId { get; set; }
        
        public string Message { get; set; }
    }
}