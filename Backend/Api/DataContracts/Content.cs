using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YouShallNotPassBackend.DataContracts
{
    public class Content
    {
        [Required]
        public ContentType ContentType { get; init; }

        [Required]
        public string Label { get; init; } = string.Empty;

        [Required]
        public DateTime ExpirationDate { get; init; }

        [Required]
        public int MaxAccessCount { get; init; }

        [DefaultValue(0)]
        public int TimesAccessed { get; init; } = 0;

        /// <summary>
        /// For json, use a base64-encoded string
        /// 
        /// For example, if the data is "password", use "cGFzc3dvcmQ="
        /// </summary>
        [Required]
        public byte[] Data { get; init; } = Array.Empty<byte>();

        public string? SecurityQuestion { get; init; }

        public string? SecurityQuestionAnswer { get; init; } 

        public override bool Equals(object? obj)
        {
            if (obj is not Content other) return false;

            return ContentType == other.ContentType &&
                Label == other.Label &&
                ExpirationDate == other.ExpirationDate &&
                MaxAccessCount == other.MaxAccessCount &&
                Enumerable.SequenceEqual(Data, other.Data);
        }

        public override int GetHashCode()
        {
            return ContentType.GetHashCode() ^
                Label.GetHashCode() ^
                ExpirationDate.GetHashCode() ^
                MaxAccessCount.GetHashCode() ^
                Data.GetHashCode();
        }

    }
}
