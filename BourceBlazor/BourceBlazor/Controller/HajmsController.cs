﻿using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BourceBlazor.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class HajmsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public HajmsController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // GET: api/Hajms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hajm>>> GetHajm()
        {
            string cacheKey = "cachedHajms";
            var cachedData = _memoryCache.Get<List<Hajm>>(cacheKey);

            if (cachedData == null)
            {
                 cachedData = await _context.Hajm.OrderByDescending(x => x.Counter).ToListAsync();
                _memoryCache.Set(cacheKey, cachedData);
            }

            return cachedData;

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
                return new List<Hajm>();
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
                _memoryCache.Remove("cachedHajms");
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
            _memoryCache.Remove("cachedHajms");

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
            _memoryCache.Remove("cachedHajms");

            return NoContent();
        }

        [HttpDelete("/DeleteHajmsByTagsAndCode/{hajmValue}/{insCode}")]
        public async Task<IActionResult> DeleteHajmsByTagsAndCode(int hajmValue, string insCode)
        {
            var hajm = await _context.Hajm.FirstOrDefaultAsync(x => x.HajmValue == hajmValue && x.Code == insCode);

            if (hajm == null)
            {
                return NotFound();
            }

            _context.Hajm.Remove(hajm);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("cachedHajms");
            return NoContent();
        }

        
        private bool HajmExists(Guid id)
        {
            return _context.Hajm.Any(e => e.Id == id);
        }
    }
}
