using System.IO;
using SQLite;
using UnityEngine;

namespace CreativeMode
{
    public static class DatabaseUtils
    {
        public static SQLiteConnection OpenDb(string name)
        {
            var originalDbName = name + ".sqlite";
            var originalDbPath = Path.Combine(Application.persistentDataPath, originalDbName);
            var usedDbPath = originalDbPath;
            
            // Copy original database if it exists,
            // so any changes in editor will not ruin actual application state
            if (Application.isEditor)
            {
                var editorDbName = "Editor-" + originalDbName;
                var editorDbPath = Path.Combine(Application.persistentDataPath, editorDbName);

                if (File.Exists(originalDbPath))
                {
                    if (File.Exists(editorDbPath))
                        File.Delete(editorDbPath);

                    File.Copy(originalDbPath, editorDbPath);
                }

                usedDbPath = editorDbPath;
            }
            
            var connection = new SQLiteConnection(usedDbPath);
            connection.ExecuteScalar<string> ("PRAGMA journal_mode=MEMORY"); // TODO: disable once async access to db is implemented
            return connection;
        }
    }
}