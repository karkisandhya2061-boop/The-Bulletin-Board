using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly AdStore _store;

        public AdsController(AdStore store)
        {
            _store = store;
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/v1/ads/random
        /// Returns one random active ad. 204 No Content if none exist.
        /// </summary>
        [HttpGet("api/v1/ads/random")]
        public IActionResult GetRandom()
        {
            var ad = _store.GetRandom();
            return ad == null
                ? NoContent()
                : Ok(ad);
        }

        // ═══════════════════════════════════════════════════════════
        // ADMIN
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/v1/admin/ads — all ads regardless of isActive</summary>
        [Authorize(Roles = "admin")]
        [HttpGet("api/v1/admin/ads")]
        public IActionResult GetAll() => Ok(_store.GetAll());

        /// <summary>POST /api/v1/admin/ads — create an ad</summary>
        [Authorize(Roles = "admin")]
        [HttpPost("api/v1/admin/ads")]
        public IActionResult Create([FromBody] AdRequest req)
        {
            var err = Validate(req);
            if (err != null) return err;

            var ad = _store.Add(req);
            return StatusCode(201, ad);
        }

        /// <summary>PUT /api/v1/admin/ads/:id — full update</summary>
        [Authorize(Roles = "admin")]
        [HttpPut("api/v1/admin/ads/{id:int}")]
        public IActionResult Update(int id, [FromBody] AdRequest req)
        {
            var err = Validate(req);
            if (err != null) return err;

            var ad = _store.Update(id, req);
            return ad == null
                ? NotFound(new { message = $"Ad {id} not found." })
                : Ok(ad);
        }

        /// <summary>DELETE /api/v1/admin/ads/:id</summary>
        [Authorize(Roles = "admin")]
        [HttpDelete("api/v1/admin/ads/{id:int}")]
        public IActionResult Delete(int id)
        {
            return _store.Delete(id)
                ? Ok(new { message = "Ad deleted." })
                : NotFound(new { message = $"Ad {id} not found." });
        }

        // ─── validation ───────────────────────────────────────────
        private IActionResult? Validate(AdRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(new { message = "title is required." });
            if (string.IsNullOrWhiteSpace(req.Cta))
                return BadRequest(new { message = "cta is required." });
            return null;
        }
    }
}
