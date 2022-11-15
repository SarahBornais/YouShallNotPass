using YouShallNotPassBackend.Storage;

namespace YouShallNotPassBackend.Security
{
    public record EncryptedData
    {
        public ByteString Data { get; init; } = Array.Empty<byte>();
        public ByteString IV { get; init; } = Array.Empty<byte>();
        public int Length { get; init; } = 0;
    }
}
