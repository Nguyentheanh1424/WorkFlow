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
    public record UpdateUserPhoneNumberCommand(
        string PhoneNumber
    ) : IRequest<Result<GetCurrentUserDto>>;

    public class UpdateUserPhoneNumberCommandValidator
        : AbstractValidator<UpdateUserPhoneNumberCommand>
    {
        public UpdateUserPhoneNumberCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Số điện thoại không được để trống.")
                .Matches(@"^(0|\+84)[0-9]{9}$")
                .WithMessage("Số điện thoại Việt Nam không hợp lệ.");
        }
    }

    public class UpdateUserPhoneNumberCommandHandler
        : IRequestHandler<UpdateUserPhoneNumberCommand, Result<GetCurrentUserDto>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateUserPhoneNumberCommandHandler(
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
            UpdateUserPhoneNumberCommand request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return Result<GetCurrentUserDto>.Failure("Không xác định được người dùng.");

            var userId = _currentUser.UserId.Value;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<GetCurrentUserDto>.Failure("Người dùng không tồn tại.");

            user.UpdatePhoneNumber(request.PhoneNumber);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<GetCurrentUserDto>(user);

            return Result<GetCurrentUserDto>.Success(
                dto,
                "Cập nhật số điện thoại thành công."
            );
        }
    }
}
