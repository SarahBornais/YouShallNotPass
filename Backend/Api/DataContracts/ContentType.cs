using System.Runtime.Serialization;

namespace YouShallNotPassBackend.DataContracts
{
    [DataContract]
    public enum ContentType
    {
        /// <summary>
        ///  .pdf file
        /// </summary>
        PDF,
        /// <summary>
        /// .png file
        /// </summary>
        PNG,
        /// <summary>
        /// text
        /// </summary>
        TEXT
    }
}
