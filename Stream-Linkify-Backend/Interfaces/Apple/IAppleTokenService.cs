namespace Stream_Linkify_Backend.Interfaces.Apple
{
    public interface IAppleTokenService
    {
        Task<string> GetValidTokenAsync();
    }
}
