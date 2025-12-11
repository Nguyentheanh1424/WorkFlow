namespace WorkFlow.Application.Common.Constants.EventNames
{
    public static class WorkspaceEvents
    {
        public const string Create = "Workspace.Create";
        public const string Updated = "Workspace.Updated";
        public const string Deleted = "Workspace.Deleted";

        public const string MemberAdded = "Workspace.Member.Added";
        public const string MemberRemoved = "Workspace.Member.Removed";
        public const string MemberUpdateRole = "Workspace.Member.Update.Role";

        public const string BoardAdded = "Workspace.Board.Added";
        public const string BoardRemoved = "Workspace.Board.Removed";
    }
}
