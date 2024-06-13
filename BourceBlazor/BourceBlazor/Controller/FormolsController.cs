
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppShared.Entities;
using BourceBlazor.Data;
using AppShared.ViewModel.Nomad.Actions;
using AppShared.ViewModel;

namespace BourceBlazor.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class FormolsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration configuration;

        private readonly HttpClient _httpClient;

        public FormolsController(ApplicationDbContext context, IConfiguration configuration, HttpClient httpClient)
        {
            _context = context;
            this.configuration = configuration;
            _httpClient = httpClient;
        }

        // GET: api/Formols
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Formol>>> GetFormols()
        {
            return await _context.Formols.ToListAsync();
        }

        [HttpGet("/GetFormolsByInCode/{InCode}")]
        public async Task<ActionResult<IEnumerable<Formol>>> GetFormolsByInCode(string InCode)
        {
            var formols = await _context.Formols.Where(x => x.Code == InCode).ToListAsync();

            if (!formols.Any())
            {
                return new List<Formol>();
            }

            return formols;
        }

        // GET: api/Formols/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Formol>> GetFormol(Guid id)
        {
            var formol = await _context.Formols.FindAsync(id);

            if (formol == null)
            {
                return NotFound();
            }

            return formol;
        }

        // PUT: api/Formols/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFormol(Guid id, Formol formol)
        {
            if (id != formol.Id)
            {
                return BadRequest();
            }

            _context.Entry(formol).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FormolExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Formols
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Formol>> PostFormol(Formol formol)
        {
            _context.Formols.Add(formol);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFormol", new { id = formol.Id }, formol);
        }

        // DELETE: api/Formols/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFormol(Guid id)
        {
            var formol = await _context.Formols.FindAsync(id);
            if (formol == null)
            {
                return NotFound();
            }

            _context.Formols.Remove(formol);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FormolExists(Guid id)
        {
            return _context.Formols.Any(e => e.Id == id);
        }


        [HttpPost("/GetCalculateFormols")]
        public async Task<IActionResult> GetCalculateFormols(FormolSendAction formol)
        {
            var urlAction = configuration["Urls:UrlAction"];

            var response = await _httpClient.GetFromJsonAsync<Root>(urlAction + formol.InsCode + "/" + formol.NomadDate + "/false");

            if (response != null && response.tradeHistory.Any())
            {
                var result = await GetFilterByFormolAll(formol, response.tradeHistory);

                if (!result.IsSuccess)
                {
                    return NotFound(result.ErrorMessage);
                }

                return Ok(result.MainRealBaseTradeHistories);

            }

            return NotFound();
        }
    }

    /// <summary>
    ///فرمول نویسی
    /// </summary>
    public partial class FormolsController
    {
        //ردیف های واقعی
        private List<TradeHistory> MainRealBaseTradeHistories = new();

        //ردیف های پایه
        private List<TradeHistory> BaseTradeHistories = new();

        //مقادیر  ردیف پایه
        private BaseTradeHistoriesViewModel BaseTradeHistoriesViewModel { get; set; } = new();

        //فرمول
        private FormolSwitchViewModel formol = new();

        //مرحله جاری
        private int CurrentMultiStage = 1;

        //اتمام مرحله
        private bool Restart = true;

        private string SumHajm { get; set; } = string.Empty;
        private string SumCount { get; set; } = string.Empty;

        private IEnumerable<TradeHistory> TradeHistories = default!;


        //محاسبه فرمول اصلی
        private async Task<ResultCalculateFormol> GetFilterByFormolAll(FormolSendAction formol, List<TradeHistory> TradeHistoriesList)
        {
            Restart = true;

            var _formol = formol;

            if (formol != null)
                formol = _formol;

            while (Restart)
            {
                BaseTradeHistories.Clear();

                if (TradeHistoriesList.Count <= CurrentMultiStage)
                {
                    CurrentMultiStage = 1;
                    TradeHistoriesList.Clear();
                    TradeHistories = MainRealBaseTradeHistories.AsEnumerable();
                    SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
                    SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");
                    Restart = false;

                    var model = new ResultCalculateFormol()
                    {
                        TradeHistories = TradeHistories.ToList(),
                        SumCount = SumCount ,
                        SumHajm = SumHajm,
                        MainRealBaseTradeHistories = MainRealBaseTradeHistories,
                        ErrorMessage = "successful",
                        IsSuccess=true
                    };

                    return model;
                }

                var result = TradeHistoriesList
                       .Skip(0)
                       .Take(CurrentMultiStage)
                       .ToList();

                BaseTradeHistories.AddRange(result);

                if (CurrentMultiStage == 1)
                {
                    var firstTradeHistories = BaseTradeHistories.FirstOrDefault();

                    if (firstTradeHistories == null)
                    {
                        return new ResultCalculateFormol()
                        {
                            ErrorMessage= "firstTradeHistories is null" 
                        };
                    }

                    BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
                    BaseTradeHistoriesViewModel.BaseHajm = firstTradeHistories.qTitTran;
                    BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
                    BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.TimeFormol;
                }
                else if (CurrentMultiStage > 1)
                {
                    var firstTradeHistories = BaseTradeHistories.Skip(0).Take(1).FirstOrDefault();

                    if (firstTradeHistories == null)
                    {
                        return new ResultCalculateFormol()
                        {
                            ErrorMessage = "firstTradeHistories is null"
                        };
                    }

                    BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
                    BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
                    BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.TimeFormol;
                    BaseTradeHistoriesViewModel.BaseHajm = BaseTradeHistories.Sum(b => b.qTitTran);
                }

                //محاسبه خود ردیف پایه با فرمول
                if (BaseTradeHistoriesViewModel.BaseHajm == formol.HajmFormol)
                {
                    BaseTradeHistories.ForEach(item => TradeHistoriesList.Remove(item));

                    //ریست کن مراحل        
                    //تنظیم ردیف پایه جدید
                    CurrentMultiStage = 1;

                    continue;
                }

                var calculateTradeHistories = TradeHistoriesList
                                             .Skip(CurrentMultiStage)
                                             .Where(x => x.hEven <= BaseTradeHistoriesViewModel.BaseEndTime)
                                             .ToList();

                if (calculateTradeHistories.Count < BaseTradeHistories.Count)
                {
                    //اولین ردیف از پایه هارا حذف کن
                    var first = BaseTradeHistories.FirstOrDefault();

                    if (first != null)
                    {
                        TradeHistoriesList.Remove(first);
                        MainRealBaseTradeHistories.Add(first);
                        CurrentMultiStage = 1;
                    }

                    continue;
                }

                bool isFinshFor = false;
                foreach (var item in calculateTradeHistories)
                {
                    if (!isFinshFor)
                    {
                        var sum = BaseTradeHistoriesViewModel.BaseHajm + item.qTitTran;

                        if (sum == formol.HajmFormol)
                        {
                            BaseTradeHistories.ForEach(b => TradeHistoriesList.Remove(b));
                            TradeHistoriesList.Remove(item);
                            CurrentMultiStage = 1;
                            isFinshFor = true;
                        }
                    }
                }


                if (isFinshFor)
                {
                    continue;
                }

                //مرحله جاری را افزایش بده
                CurrentMultiStage = CurrentMultiStage + 1;

                var multiStage = formol.MultiStage;

                // در صورتی که به مرحله آخر رسیده باشید
                if (multiStage < CurrentMultiStage)
                {
                    var first = BaseTradeHistories.FirstOrDefault();

                    if (first != null)
                    {
                        TradeHistoriesList.Remove(first);
                        MainRealBaseTradeHistories.Add(first);
                        CurrentMultiStage = 1;
                        continue;
                    }
                }

                var counTradeHistoriesListt = TradeHistoriesList.Count;

                if (counTradeHistoriesListt == 0 || TradeHistoriesList.Count <= CurrentMultiStage)
                {
                    CurrentMultiStage = 1;
                    TradeHistoriesList.Clear();
                    TradeHistories = MainRealBaseTradeHistories.AsEnumerable();
                    SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
                    SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");
                    Restart = false;

                    var model = new ResultCalculateFormol()
                    {
                        TradeHistories = TradeHistories.ToList(),
                        SumCount = SumCount,
                        SumHajm = SumHajm,
                        MainRealBaseTradeHistories = MainRealBaseTradeHistories,
                        ErrorMessage = "successful",
                        IsSuccess = true
                    };

                    return model;
                }
            }

            return new ResultCalculateFormol()
            {
                ErrorMessage = "firstTradeHistories is null"
            };
        }


       
    }
}
