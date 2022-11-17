using System.Collections;

namespace YouShallNotPassBackend.Storage
{
    public class ByteString
    {
        public byte[] Bytes { get; init; } = Array.Empty<byte>();

        public static implicit operator byte[](ByteString byteString) => byteString.Bytes;

        public static implicit operator ByteString(byte[] bytes) => new() { Bytes = bytes };

        public override bool Equals(object? obj)
        {
            if (obj is not ByteString other) return false;

            return Enumerable.SequenceEqual(Bytes, other.Bytes);
        }

        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }
    }
}
