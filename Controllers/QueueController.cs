using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Editorial queue — admin-only CRUD.
    /// Base route: /api/v1/admin/queue
    /// </summary>
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

        // ─── GET /api/v1/admin/queue ──────────────────────────────
        /// <summary>Returns all queue items, sorted high → normal → low priority.</summary>
        [HttpGet]
        public IActionResult GetAll() => Ok(_store.GetAll());

        // ─── POST /api/v1/admin/queue ─────────────────────────────
        /// <summary>Creates a new queue item.</summary>
        [HttpPost]
        public IActionResult Create([FromBody] QueueItemRequest req)
        {
            var err = Validate(req);
            if (err != null) return err;

            var item = _store.Add(req);
            return CreatedAtAction(nameof(GetAll), new { id = item.Id }, item);
        }

        // ─── PUT /api/v1/admin/queue/:id ──────────────────────────
        /// <summary>Fully updates a queue item (title, priority, status).</summary>
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

        // ─── PATCH /api/v1/admin/queue/:id/complete ───────────────
        /// <summary>Marks a queue item as completed.</summary>
        [HttpPatch("{id:int}/complete")]
        public IActionResult Complete(int id)
        {
            var item = _store.Complete(id);
            return item == null
                ? NotFound(new { message = $"Queue item {id} not found." })
                : Ok(item);
        }

        // ─── VALIDATION ───────────────────────────────────────────
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
