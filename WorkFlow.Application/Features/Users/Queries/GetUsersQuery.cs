using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Models;
using WorkFlow.Application.Features.Users.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Users.Queries
{
    public record GetUsersQuery(
        string? Search,
        int PageIndex,
        int PageSize
    ) : IRequest<Result<PagedResult<UserDto>>>;

    public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUsersQueryHandler(IUnitOfWork uow)
        {
            _unitOfWork = uow;
        }

        public async Task<Result<PagedResult<UserDto>>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            var _userRepository = _unitOfWork.GetRepository<User, Guid>();

            var query = _userRepository.GetAll().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(x =>
                    x.Name.Contains(request.Search) ||
                    x.Email.Contains(request.Search) ||
                    x.PhoneNumber.Contains(request.Search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    AvatarUrl = x.AvatarUrl
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<UserDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return Result<PagedResult<UserDto>>.Success(result);
        }
    }
}
