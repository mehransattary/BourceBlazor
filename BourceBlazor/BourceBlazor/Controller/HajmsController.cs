using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppShared.Entities;
using BourceBlazor.Data;

namespace BourceBlazor.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class HajmsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HajmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Hajms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hajm>>> GetHajm()
        {
            return await _context.Hajm.OrderByDescending(x=>x.Counter).ToListAsync();
        }

        // GET: api/Hajms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Hajm>> GetHajm(Guid id)
        {
            var hajm = await _context.Hajm.FindAsync(id);

            if (hajm == null)
            {
                return NotFound();
            }

            return hajm;
        }

        // GET: api/Hajms/5
        [HttpGet("/GetHajmByCode/{code}")]
        public async Task<ActionResult<IEnumerable<Hajm>>> GetHajmByCode(string code)
        {
            var hajms = await _context.Hajm.Where(x => x.Code == code).ToListAsync();

            if (!hajms.Any())
            {
                return NotFound();
            }

            return hajms;
        }

        // PUT: api/Hajms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHajm(Guid id, Hajm hajm)
        {
            if (id != hajm.Id)
            {
                return BadRequest();
            }

            _context.Entry(hajm).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HajmExists(id))
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

        // POST: api/Hajms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Hajm>> PostHajm(Hajm hajm)
        {
            var lastHajmCounter = await _context.Hajm.OrderByDescending(x => x.Counter).Select(x=>x.Counter).FirstOrDefaultAsync();
            hajm.Counter = lastHajmCounter+1;
            await _context.Hajm.AddAsync(hajm);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHajm", new { id = hajm.Id }, hajm);
        }

        // DELETE: api/Hajms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHajm(Guid id)
        {
            var hajm = await _context.Hajm.FindAsync(id);
            if (hajm == null)
            {
                return NotFound();
            }

            _context.Hajm.Remove(hajm);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HajmExists(Guid id)
        {
            return _context.Hajm.Any(e => e.Id == id);
        }
    }
}
