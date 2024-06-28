
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.ViewModel;
using Infrastructure.Data;
using Application.Services;
using Domain.Entities;

namespace BourceBlazor.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class FormolsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IFormolService formolService;

        private readonly IHttpService httpService;

        public FormolsController(ApplicationDbContext context, IFormolService formolService, IHttpService httpService)
        {
            _context = context;
            this.formolService = formolService;
            this.httpService = httpService;
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

        [HttpPost("/GetCalculateFormols/{skip}/{take}/{reload}")]
        public async Task<IActionResult> GetCalculateFormols([FromBody] List<FormolSendAction> formols,int skip , int take, bool reload)
        {
            try
            {
                var firstFormol = formols.FirstOrDefault();

                var tradeHistories = await httpService.GetTradeHistoriesByApi(firstFormol.InsCode, firstFormol.NomadDate, skip, take, reload);

                var result = await formolService.GetFilterByFormolAll(formols, tradeHistories);

                if (!result.IsSuccess)
                {
                    return NotFound(result.ErrorMessage);
                }

                if (firstFormol.IsDataRemoved)
                {
                    return Ok(result.DeletedTradeHistories.Distinct());
                }

                return Ok(result.MainRealBaseTradeHistories.Distinct());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

        }

        private bool FormolExists(Guid id)
        {
            return _context.Formols.Any(e => e.Id == id);
        }

    }

}
