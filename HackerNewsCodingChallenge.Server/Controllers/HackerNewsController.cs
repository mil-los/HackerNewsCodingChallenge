using HackerNewsCodingChallenge.Server.Model;
using HackerNewsCodingChallenge.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsCodingChallenge.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HackerNewsController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;

        public HackerNewsController(IHackerNewsService hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
        }

        [HttpGet]
        public Task<IEnumerable<Story>> Get()
        {
            return _hackerNewsService.GetNewStoriesAsync();
        }
    }
}
