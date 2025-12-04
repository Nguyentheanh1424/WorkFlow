using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Common.Constants.EventNames
{
    public static class ListEvents
    {
        public const string Created = "List.Created";
        public const string Updated = "List.Updated";
        public const string Moved = "List.Moved";
        public const string Archived = "List.Archived";
        public const string Unarchived = "List.Unarchived";
        public const string Deleted = "List.Deleted";
    }
}
