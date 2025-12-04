using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Common.Constants.EventNames
{
    public static class BoardEvents
    {
        public const string Updated = "Board.Updated";
        public const string Deleted = "Board.Deleted";

        public const string MemberAdded = "Board.member.Added";
        public const string MemberRemoved = "Board.member.Removed";
    }
}
