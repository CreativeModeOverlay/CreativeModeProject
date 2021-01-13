using SQLite;

namespace CreativeMode
{
    internal struct UserColorDB
    {
        [PrimaryKey]
        public string AuthorId { get; set; }
        public int ARGB { get; set; }
    }
}