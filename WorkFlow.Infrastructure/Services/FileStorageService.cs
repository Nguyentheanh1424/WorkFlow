using Microsoft.Extensions.Logging;
using WorkFlow.Application.Common.Interfaces.Services;
using Supabase;

namespace WorkFlow.Infrastructure.Services
{
    public sealed class SupabaseStorageOptions
    {
        public string Url { get; set; } = default!;
        public string AnonKey { get; set; } = default!;
        public string Bucket { get; set; } = "uploads";
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly Client _supabase;
        private readonly SupabaseStorageOptions _options;

        public FileStorageService(
            ILogger<FileStorageService> logger,
            Client supabase,
            SupabaseStorageOptions options)
        {
            _logger = logger;
            _supabase = supabase;
            _options = options;
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string? contentType = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms, cancellationToken);
                var fileBytes = ms.ToArray();

                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var supabasePath = uniqueFileName;

                Supabase.Storage.FileOptions? fileOptions = null;

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    fileOptions = new Supabase.Storage.FileOptions
                    {
                        ContentType = contentType,
                        Upsert = false
                    };
                }

                await _supabase.Storage
                    .From(_options.Bucket)
                    .Upload(fileBytes, supabasePath, fileOptions);

                var publicUrl = _supabase.Storage
                    .From(_options.Bucket)
                    .GetPublicUrl(supabasePath);

                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload file to Supabase Storage failed");
                throw;
            }
        }

        public async Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);

                await _supabase.Storage
                    .From(_options.Bucket)
                    .Remove(fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete file from Supabase Storage failed");
                throw;
            }
        }
    }
}