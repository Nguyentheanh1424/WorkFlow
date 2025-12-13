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
    public record UpdateUserDateOfBirthCommand(
        DateTime DateOfBirth
    ) : IRequest<Result<GetCurrentUserDto>>;

    public class UpdateUserDateOfBirthCommandValidator
        : AbstractValidator<UpdateUserDateOfBirthCommand>
    {
        public UpdateUserDateOfBirthCommandValidator()
        {
            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow)
                .WithMessage("Ngày sinh phải nhỏ hơn ngày hiện tại.");
        }
    }

    public class UpdateUserDateOfBirthCommandHandler
        : IRequestHandler<UpdateUserDateOfBirthCommand, Result<GetCurrentUserDto>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateUserDateOfBirthCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
            _userRepository = _unitOfWork.GetRepository<User, Guid>();
        }

        public async Task<Result<GetCurrentUserDto>> Handle(
            UpdateUserDateOfBirthCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<GetCurrentUserDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<GetCurrentUserDto>.Failure("Người dùng không tồn tại.");

            user.UpdateDateOfBirth(request.DateOfBirth);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<GetCurrentUserDto>(user);

            return Result<GetCurrentUserDto>.Success(
                dto,
                "Cập nhật ngày sinh thành công."
            );
        }
    }
}
