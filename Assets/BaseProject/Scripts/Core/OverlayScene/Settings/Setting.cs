using System;

namespace CreativeMode
{
    public interface ISetting
    {
        string Path { get; }
        string Description { get; }
        Type ValueType { get; }

        object ObjectValue { get; set; }
    }
    
    public class Setting<T> : ISetting
    {
        public string Path { get; }
        public string Description { get; }
        public Type ValueType { get; }
        public object ObjectValue { get; set; }
        
        /*public T Value { get; set; }
        
        public string Path { get; }
        
        public Type ValueType => typeof(T);
        public object ObjectValue
        {
            get => Value;
            set => Value = (T) value;
        }

        public object DefaultValue => defaultSettingValue;

        private string settingPath;
        private T defaultSettingValue;*/

        public Setting(string path, string description, T defaultValue = default)
        {
            
        }
    }
}