using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Application.Common.Constants.EventNames
{
    public static class WorkspaceEvents
    {
        public const string Create = "Workspace.Create";
        public const string Updated = "Workspace.Updated";
        public const string Deleted = "Workspace.Deleted";

        public const string MemberAdded = "Workspace.Member.Added";
        public const string MemberRemoved = "Workspace.Member.Removed";

        public const string BoardAdded = "Workspace.Board.Added";
        public const string BoardRemoved = "Workspace.Board.Removed";
    }
}
