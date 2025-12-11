namespace WorkFlow.Application.Common.Constants.EventNames
{
    public static class TaskEvents
    {
        public const string Created = "Task.Created";
        public const string Updated = "Task.Updated";
        public const string Deleted = "Task.Deleted";

        public const string Moved = "Task.Moved";

        public const string SubTaskCreated = "Task.SubTask.Created";
        public const string SubTaskUpdated = "Task.SubTask.Updated";
        public const string SubTaskDeleted = "Task.SubTask.Deleted";

        public const string SubTaskMoved = "Task.SubTask.Moved";
    }
}
