using SQLite;

namespace CreativeMode
{
    public struct UserColorDB
    {
        [PrimaryKey]
        public string AuthorId { get; set; }
        public int ARGB { get; set; }
    }
}