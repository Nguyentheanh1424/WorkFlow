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
    public record UpdateUserNameCommand(string Name)
        : IRequest<Result<GetCurrentUserDto>>;

    public class UpdateUserNameCommandValidator
        : AbstractValidator<UpdateUserNameCommand>
    {
        public UpdateUserNameCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên không được để trống.")
                .MaximumLength(100)
                .WithMessage("Tên không được vượt quá 100 ký tự.");
        }
    }

    public class UpdateUserNameCommandHandler
        : IRequestHandler<UpdateUserNameCommand, Result<GetCurrentUserDto>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateUserNameCommandHandler(
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
            UpdateUserNameCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<GetCurrentUserDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<GetCurrentUserDto>.Failure("Người dùng không tồn tại.");

            var (isSuccess, remainingDays) = user.UpdateName(request.Name);

            if (!isSuccess)
            {
                return Result<GetCurrentUserDto>.Failure(
                    $"Bạn có thể đổi tên sau {remainingDays} ngày nữa."
                );
            }

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<GetCurrentUserDto>(user);
            return Result<GetCurrentUserDto>.Success(dto, "Cập nhật tên thành công");
        }
    }
}
