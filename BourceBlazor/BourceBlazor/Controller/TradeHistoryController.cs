using Application.Services;
using Application.ViewModel.Nomad.Actions;
using Microsoft.AspNetCore.Mvc;

namespace BourceBlazor.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeHistoryController : ControllerBase
    {
        private readonly IHttpService httpService;

        public TradeHistoryController(IHttpService httpService)
        {
            this.httpService = httpService;
        }

        [HttpGet("{insCode}/{nomadDate}/{skip}/{take}")]
        public async Task<ActionResult<List<TradeHistory>>> Get(string insCode, int nomadDate, int skip, int take)
        {
            var tradeHistories = await httpService.GetTradeHistoriesByApi(insCode, nomadDate, skip, take);
            return tradeHistories;
        }
    }
}
