using System;
using System.Threading.Tasks;
using Aspen.CharityApi.Http;
using Aspen.Core;
using Aspen.Core.Models;
using Aspen.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Aspen.CharityApi.Controllers
{
    [ApiController]
    [Route("{controller}/")]
    public class CharityController : ControllerBase
    {
        private readonly ICharityRepository charityRepository;
        private readonly IThemeRepository themeRepository;

        public CharityController(
            ICharityRepository charityRepository,
            IThemeRepository themeRepository)
        {
            this.charityRepository = charityRepository;
            this.themeRepository = themeRepository;
        }

        [HttpGet]
        public async Task<ApiResult> Get([FromQuery(Name = "domain")] string domain) =>
            await domain
                .ValidateFunction(validateDomain)
                .ApplyAsync(charityRepository.GetByDomain)
                .ReturnApiResult();

        [HttpGet("getbyid")]
        public async Task<ApiResult> Get([FromQuery(Name = "CharityId")] Guid charityId) =>
            await charityId
                .ValidateFunction(validateCharityId)
                .ApplyAsync(charityRepository.GetById)
                .ReturnApiResult();

        private Result<Domain> validateDomain(string domain)
        {
            try
            {
                var d = new Domain(domain);
                return Result<Domain>.Success(d);
            }
            catch(ArgumentException e)
            {
                return Result<Domain>.Failure(e.Message);
            }
        }

        [HttpGet("gettheme")]
        public async Task<ApiResult> GetTheme([FromQuery(Name = "charityId")] Guid charityId) =>
            await new Result<Guid>(charityId)
                .ApplyAsync(charityRepository.GetById)
                .ApplyAsync(themeRepository.GetByCharity)
                .ReturnApiResult();

        private Result<Guid> validateCharityId(Guid id) => Result<Guid>.Success(id);

        [HttpGet("gethomepage")]
        public ApiResult GetHomePage([FromQuery(Name = "charityId")] Guid charityId)
        {
            const string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            return ApiResult.Success(new HomePage(loremIpsum));
        }

        [HttpPost("updatetheme")]
        public async Task<ApiResult> UpdateTheme([FromBody]ThemeRequest request) =>
            await request
                .ValidateFunction(getValidCharity)
                .ApplyAsync(async c => await themeRepository.Update(request.Theme, c.ConnectionString))
                .ReturnApiResult();

        private async Task<Result<Charity>> getValidCharity(ThemeRequest request)
        {
            return await charityRepository.GetById(request.CharityId);
        }
    }
}