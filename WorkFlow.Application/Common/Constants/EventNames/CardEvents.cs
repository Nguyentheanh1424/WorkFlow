using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Common.Constants.EventNames
{
    public static class CardEvents
    {
        public const string Created = "Card.Created";
        public const string Updated = "Card.Updated";
        public const string Moved = "Card.Moved";

        public const string AssigneeAdded = "Card.Assignee.Added";
        public const string AssigneeRemoved = "Card.Assignee.Removed";

        public const string AttachmentAdded = "Card.Attachment.Added";
        public const string AttachmentRemoved = "Card.Attachment.Removed";

        public const string Deleted = "Card.Deleted";
    }
}
