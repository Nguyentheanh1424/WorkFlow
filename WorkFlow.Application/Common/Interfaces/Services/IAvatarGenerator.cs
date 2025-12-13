namespace WorkFlow.Application.Common.Interfaces.Services
{
    public interface IAvatarGenerator
    {
        Task<string> GenerateSvgAsync(string seed);
    }

}
