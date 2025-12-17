using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Features.Users.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Application.Features.Users.Queries
{
    public record GetCurrentUserQuery()
        : IRequest<Result<GetCurrentUserDto>>;

    public class GetCurrentUserQueryValidator
        : AbstractValidator<GetCurrentUserQuery>
    {
        public GetCurrentUserQueryValidator()
        {
        }
    }

    public class GetCurrentUserQueryHandler
        : IRequestHandler<GetCurrentUserQuery, Result<GetCurrentUserDto>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<AccountAuth, Guid> _accountAuthRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetCurrentUserQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _userRepository = unitOfWork.GetRepository<User, Guid>();
            _accountAuthRepository = unitOfWork.GetRepository<AccountAuth, Guid>();
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<Result<GetCurrentUserDto>> Handle(
            GetCurrentUserQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<GetCurrentUserDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;


            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<GetCurrentUserDto>.Failure("Người dùng không tồn tại.");

            var dto = _mapper.Map<GetCurrentUserDto>(user);

            var auths = await _accountAuthRepository
            .FindAsync(x => x.UserId == userId);

            dto.IsGoogleLinked = auths.Any(x =>
                x.Provider == EnumExtensions.GetName(AccountProvider.Google));

            dto.IsFacebookLinked = auths.Any(x =>
                x.Provider == EnumExtensions.GetName(AccountProvider.Facebook));

            return Result<GetCurrentUserDto>.Success(dto);
        }
    }
}
