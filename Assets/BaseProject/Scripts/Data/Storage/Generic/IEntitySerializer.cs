namespace CreativeMode.Generic
{
    public interface IEntitySerializer<T>
    {
        byte[] Serialize(T value);
        T Deserialize(byte[] data);
    }
}