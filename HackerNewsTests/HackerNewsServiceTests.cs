using HackerNewsCodingChallenge.Server.Model;
using HackerNewsCodingChallenge.Server.Services;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace HackerNewsTests
{

    [TestClass]
    public class HackerNewsServiceTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private HackerNewsService _hackerNewsService;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;

        [TestInitialize]
        public void Initialize()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        }

        [TestMethod]
        public async Task GetStories_HasCachedStories_ShouldReturnStoriesFromCache()
        {
            // Arrange
            List<Story> expectedStories = new()
            {
                new Story { Id = 1 },
                new Story { Id = 2 }
            };

            var mockedCache = Create.MockedMemoryCache();

            mockedCache.Set("NewStoriesList", expectedStories, new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(300))
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(1024));

            _hackerNewsService = new HackerNewsService(_httpClientFactoryMock.Object, mockedCache);

            // Act
            var actualStories = await _hackerNewsService.GetNewStoriesAsync();

            // Assert
            CollectionAssert.AreEqual(expectedStories, actualStories.ToList());
        }

        [TestMethod]
        public async Task GetStories_DoesNotHaveCachedStories_ShouldReturnStoriesFromApi()
        {
            // Arrange
            List<Story> expectedStories = new()
            {
                new Story { Id = 1 },
                new Story { Id = 2 }
            };

            var storyIds = new List<int> { 1, 2 };

            HttpResponseMessage result = new(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(storyIds))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
                            )
                .ReturnsAsync(result)
                .Verifiable();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://www.hackernews.com/")
            };

            _httpClientFactoryMock.Setup(_ => _.CreateClient("hackerNews")).Returns(httpClient);

            var mockedCache = Create.MockedMemoryCache();

            var hackerNewsServiceMock = new Mock<HackerNewsService>(_httpClientFactoryMock.Object, mockedCache) { CallBase = true };

            hackerNewsServiceMock.Setup(x => x.GetStoryByIdAsync(1)).Returns(Task.FromResult(expectedStories.First(s => s.Id == 1)));
            hackerNewsServiceMock.Setup(x => x.GetStoryByIdAsync(2)).Returns(Task.FromResult(expectedStories.First(s => s.Id == 2)));

            // Act
            var actualStories = await hackerNewsServiceMock.Object.GetNewStoriesAsync();

            // Assert
            CollectionAssert.AreEqual(expectedStories, actualStories.ToList());
            hackerNewsServiceMock.Verify(x => x.GetStoryByIdAsync(It.IsAny<int>()), Times.Exactly(2));

        }

        [TestMethod]
        public async Task GetStoryById_ApiReturnsSuccessResult_ShouldReturnStory()
        {
            // Arrange
            var expectedStory = new Story { Id = 1 };

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedStory))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
                            )
                .ReturnsAsync(result)
                .Verifiable();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://www.hackernews.com/")
            };

            _httpClientFactoryMock.Setup(_ => _.CreateClient("hackerNews")).Returns(httpClient);

            var mockedCache = Create.MockedMemoryCache();

            var hackerNewsService = new HackerNewsService(_httpClientFactoryMock.Object, mockedCache);

            // Act
            var actualStory = await hackerNewsService.GetStoryByIdAsync(expectedStory.Id);

            // Assert
            Assert.AreEqual(expectedStory.Id, actualStory.Id);
        }

        [TestMethod]
        public async Task GetStoryById_ApiDoesNotReturnStory_ShouldReturnEmptyStory()
        {
            // Arrange
            var expectedStory = new Story();

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedStory))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
                            )
                .ReturnsAsync(result)
                .Verifiable();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://www.hackernews.com/")
            };

            _httpClientFactoryMock.Setup(_ => _.CreateClient("hackerNews")).Returns(httpClient);

            var mockedCache = Create.MockedMemoryCache();

            var hackerNewsService = new HackerNewsService(_httpClientFactoryMock.Object, mockedCache);

            // Act
            var actualStory = await hackerNewsService.GetStoryByIdAsync(expectedStory.Id);

            // Assert
            Assert.AreEqual(expectedStory.Id, actualStory.Id);
        }
    }
}
