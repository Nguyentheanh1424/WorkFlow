using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using WorkFlow.Application.Common.Interfaces.Auth;
using WorkFlow.Application.Common.Interfaces.Repositories;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Application.Features.Users.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Users.Commands
{
    public record UpdateUserAvatarCommand(IFormFile? File, bool IsRandom = false)
        : IRequest<Result<GetCurrentUserDto>>;

    public class UpdateMyAvatarCommandValidator : AbstractValidator<UpdateUserAvatarCommand>
    {
        private static readonly string[] AllowedContentTypes =
            { "image/jpeg", "image/png", "image/webp" };

        private const long MaxBytes = 36 * 1024 * 1024;

        public UpdateMyAvatarCommandValidator()
        {
            When(x => !x.IsRandom, () =>
            {
                RuleFor(x => x.File)
                    .NotNull().WithMessage("Vui lòng chọn file ảnh avatar.");

                RuleFor(x => x.File!)
                    .Must(f => f.Length > 0)
                    .WithMessage("File ảnh rỗng.");

                RuleFor(x => x.File!)
                    .Must(f => f.Length <= MaxBytes)
                    .WithMessage($"File ảnh quá lớn (tối đa {MaxBytes / 1024 / 1024}MB).");

                RuleFor(x => x.File!)
                    .Must(f => AllowedContentTypes.Contains(f.ContentType))
                    .WithMessage("Định dạng ảnh không hỗ trợ (chỉ JPG/PNG/WEBP).");
            });

            When(x => x.IsRandom, () =>
            {
                RuleFor(x => x.File)
                    .Null().WithMessage("Không cần upload file khi chọn avatar random.");
            });
        }
    }

    public class UpdateUserAvatarCommandHandler
        : IRequestHandler<UpdateUserAvatarCommand, Result<GetCurrentUserDto>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IAvatarGenerator _avatarGenerator;
        private readonly IFileStorageService _fileStorage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateUserAvatarCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IAvatarGenerator avatarGenerator,
            IFileStorageService fileStorage,
            IMapper mapper)
        {
            _userRepository = unitOfWork.GetRepository<User, Guid>();
            _currentUser = currentUser;
            _avatarGenerator = avatarGenerator;
            _fileStorage = fileStorage;
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

            var oldAvatarUrl = user.AvatarUrl;

            if (request.IsRandom)
            {
                var avatarUrl = await _avatarGenerator.GenerateSvgAsync(user.Id.ToString());
                user.UpdateAvatar(avatarUrl);

                if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
                {
                    try { await _fileStorage.DeleteAsync(oldAvatarUrl!, cancellationToken); } catch { }
                }
            }
            else
            {
                if (request.File == null || request.File.Length == 0)
                    return Result<GetCurrentUserDto>.Failure("Vui lòng chọn file ảnh avatar.");

                var ext = Path.GetExtension(request.File.FileName);
                if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

                var fileName = $"avatars/{user.Id}/{Path.GetFileNameWithoutExtension(request.File.FileName)}{ext}";

                await using var stream = request.File.OpenReadStream();
                var avatarUrl = await _fileStorage.UploadAsync(
                    stream,
                    fileName,
                    request.File.ContentType,
                    cancellationToken);

                user.UpdateAvatar(avatarUrl);

                if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
                {
                    try { await _fileStorage.DeleteAsync(oldAvatarUrl!, cancellationToken); } catch { }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<GetCurrentUserDto>(user);
            return Result<GetCurrentUserDto>.Success(dto, "Cập nhật Avatar thành công.");
        }
    }
}
