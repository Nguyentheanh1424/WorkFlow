using MediatR;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Common;

namespace WorkFlow.Application.Features.WorkSpaces.Queries
{
    public record GetWorkspacesQuery(string? Search) : IRequest<Result<List<WorkSpaceDto>>>;
}
