using HackerNewsCodingChallenge.Server.Model;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace HackerNewsCodingChallenge.Server.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private const string NewStoriesCacheKey = "NewStoriesList";
        private IMemoryCache _memoryCache;
        private HttpClient _httpClient;

        public HackerNewsService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _httpClient = httpClientFactory.CreateClient("hackerNews");
        }

        public async Task<IEnumerable<Story>> GetNewStoriesAsync()
        {
            if (!_memoryCache.TryGetValue(NewStoriesCacheKey, out List<Story> stories))
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://hacker-news.firebaseio.com/v0/newstories.json");
                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

                List<int> storyIds = new();
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                    storyIds = await JsonSerializer.DeserializeAsync<List<int>>(contentStream);
                }

                var tasks = storyIds.Select(id => GetStoryByIdAsync(id));
                var fetchedStories = await Task.WhenAll(tasks);

                stories = fetchedStories.Where(s => s != null).ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(300))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

                _memoryCache.Set(NewStoriesCacheKey, stories, cacheEntryOptions);
            }

            return stories;
        }

        public virtual async Task<Story> GetStoryByIdAsync(int id)
        {
            var story = new Story();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://hacker-news.firebaseio.com/v0/item/{id}.json?print=pretty");
            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();
                story = JsonSerializer.Deserialize<Story>(contentStream);
            }

            return story;
        }
    }
}
