using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class Attachment : CreationAuditEntity<Guid>
    {
        public Guid CardId { get; set; }
        public Guid UserId { get; set; }

        public string FileType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string FileName { get; set; } = null!;

        protected Attachment() { }

        public static Attachment Create(Guid cardId, Guid userId, string fileType, string fileUrl, string fileName)
        {
            return new Attachment
            {
                CardId = cardId,
                UserId = userId,
                FileType = fileType,
                FileUrl = fileUrl,
                FileName = fileName
            };
        }
    }
}
