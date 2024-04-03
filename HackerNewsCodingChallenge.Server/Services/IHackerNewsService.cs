using HackerNewsCodingChallenge.Server.Model;

namespace HackerNewsCodingChallenge.Server.Services
{
    public interface IHackerNewsService
    {
        Task<IEnumerable<Story>> GetNewStoriesAsync();
        Task<Story> GetStoryByIdAsync(int id);
    }
}
