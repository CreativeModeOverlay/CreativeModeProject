using SQLite;

namespace CreativeMode.Impl
{
    public class DeviceCaptureStorage : IDeviceCaptureStorage
    {
        private SQLiteConnection connection;
        
        public DeviceCaptureStorage(SQLiteConnection connection)
        {
            this.connection = connection;

            connection.CreateTable<DeviceCaptureParamsDB>();
        }
        
        public DeviceCaptureParamsDB GetCaptureParams(string id)
        {
            return connection.Table<DeviceCaptureParamsDB>()
                .FirstOrDefault(w => w.Id == id);
        }

        public void PutCaptureParams(DeviceCaptureParamsDB captureParams)
        {
            connection.Insert(captureParams);
        }
    }
}