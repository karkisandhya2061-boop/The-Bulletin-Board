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
        // PUBLIC
        [HttpGet("api/v1/ads/random")]
        public IActionResult GetRandom()
        {
            var ad = _store.GetRandom();
            return ad == null
                ? NoContent()
                : Ok(ad);
        }
        // ADMIN
        [Authorize(Roles = "admin")]
        [HttpGet("api/v1/admin/ads")]
        public IActionResult GetAll() => Ok(_store.GetAll());
        [Authorize(Roles = "admin")]
        [HttpPost("api/v1/admin/ads")]
        public IActionResult Create([FromBody] AdRequest req)
        {
            var err = Validate(req);
            if (err != null) return err;

            var ad = _store.Add(req);
            return StatusCode(201, ad);
        }
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
        [Authorize(Roles = "admin")]
        [HttpDelete("api/v1/admin/ads/{id:int}")]
        public IActionResult Delete(int id)
        {
            return _store.Delete(id)
                ? Ok(new { message = "Ad deleted." })
                : NotFound(new { message = $"Ad {id} not found." });
        }
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
