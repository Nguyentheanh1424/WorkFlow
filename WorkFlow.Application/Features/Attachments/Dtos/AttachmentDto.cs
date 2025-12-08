using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Application.Common.Mappings;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Attachments.Dtos
{
    public class AttachmentDto : IMapFrom<Attachment>
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Guid UserId { get; set; }

        public string FileType { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public string FileName { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
