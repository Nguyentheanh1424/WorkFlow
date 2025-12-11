namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IPermissionService
    {
        IWorkSpacePermissionService Workspace { get; }
        IBoardPermissionService Board { get; }
        ICardPermissionService Card { get; }
    }
}
