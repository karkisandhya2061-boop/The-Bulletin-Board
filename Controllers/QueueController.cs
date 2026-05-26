using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/v1/admin/queue")]
    [Authorize(Roles = "admin")]
    public class QueueController : ControllerBase
    {
        private static readonly string[] ValidPriorities = { "low", "normal", "high" };
        private static readonly string[] ValidStatuses   = { "pending", "inReview", "completed" };

        private readonly QueueStore _store;

        public QueueController(QueueStore store)
        {
            _store = store;
        }
        [HttpGet]
        public IActionResult GetAll() => Ok(_store.GetAll());
        [HttpPost]
        public IActionResult Create([FromBody] QueueItemRequest req)
        {
            var err = Validate(req);
            if (err != null) return err;

            var item = _store.Add(req);
            return CreatedAtAction(nameof(GetAll), new { id = item.Id }, item);
        }
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] QueueItemRequest req)
        {
            var err = Validate(req);
            if (err != null) return err;

            var item = _store.Update(id, req);
            return item == null
                ? NotFound(new { message = $"Queue item {id} not found." })
                : Ok(item);
        }
        [HttpPatch("{id:int}/complete")]
        public IActionResult Complete(int id)
        {
            var item = _store.Complete(id);
            return item == null
                ? NotFound(new { message = $"Queue item {id} not found." })
                : Ok(item);
        }
        private IActionResult? Validate(QueueItemRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(new { message = "title is required." });

            if (!ValidPriorities.Contains(req.Priority))
                return BadRequest(new { message = $"priority must be one of: {string.Join(", ", ValidPriorities)}" });

            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"status must be one of: {string.Join(", ", ValidStatuses)}" });

            return null;
        }
    }
}
