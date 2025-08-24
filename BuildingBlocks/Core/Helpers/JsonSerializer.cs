namespace BuildingBlocks.Core.Helpers
{
    public class JsonSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            if (data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        }
    }
}
