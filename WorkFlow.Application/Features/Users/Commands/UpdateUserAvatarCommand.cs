using AutoMapper;
using FluentValidation;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Features.Users.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Users.Commands
{
    public record UpdateUserAvatarCommand(string Avatar)
        : IRequest<Result<GetCurrentUserDto>>;

    public class UpdateUserAvatarCommandValidator
        : AbstractValidator<UpdateUserAvatarCommand>
    {
        public UpdateUserAvatarCommandValidator()
        {
            RuleFor(x => x.Avatar)
                .NotEmpty()
                .WithMessage("Avatar không được để trống.")
                .MaximumLength(500)
                .WithMessage("Avatar không hợp lệ.");
        }
    }

    public class UpdateUserAvatarCommandHandler
        : IRequestHandler<UpdateUserAvatarCommand, Result<GetCurrentUserDto>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateUserAvatarCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _userRepository = unitOfWork.GetRepository<User, Guid>();
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<GetCurrentUserDto>> Handle(
            UpdateUserAvatarCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<GetCurrentUserDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<GetCurrentUserDto>.Failure("Người dùng không tồn tại.");

            user.UpdateAvatar(request.Avatar);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<GetCurrentUserDto>(user);
            return Result<GetCurrentUserDto>.Success(dto, "Cập nhật Avatar thành công.");
        }
    }
}
