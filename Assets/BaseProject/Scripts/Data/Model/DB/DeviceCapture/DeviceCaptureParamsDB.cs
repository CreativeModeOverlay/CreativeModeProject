using SQLite;

namespace CreativeMode
{
    public class DeviceCaptureParamsDB
    {
        [PrimaryKey]
        public string Id { get; set; }
        public int ResolutionWidth { get; set; }
        public int ResolutionHeight { get; set; }
        public int RefreshRate { get; set; }
    }
}