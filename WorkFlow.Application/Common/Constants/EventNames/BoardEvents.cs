namespace WorkFlow.Application.Common.Constants.EventNames
{
    public static class BoardEvents
    {
        public const string Created = "Board.Created";
        public const string Updated = "Board.Updated";
        public const string Deleted = "Board.Deleted";

        public const string ListMoved = "Board.List.Moved";

        public const string Archived = "Board.Archived";
        public const string Restored = "Board.Restored";

        public const string MemberAdded = "Board.Member.Added";
        public const string MemberRemoved = "Board.Member.Removed";
        public const string MemberUpdateRole = "Board.Member.Update.Role";
    }
}
