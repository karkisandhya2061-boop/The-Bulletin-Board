using WebApplication1.Models;

namespace WebApplication1.Services
{
    /// <summary>
    /// Singleton in-memory store for editorial queue items.
    /// Swap out for a DB-backed service later without changing the controller.
    /// </summary>
    public class QueueStore
    {
        private readonly List<QueueItem> _items = new();
        private int _nextId = 1;

        public List<QueueItem> GetAll() =>
            _items.OrderByDescending(i => PriorityRank(i.Priority))
                  .ThenBy(i => i.CreatedAt)
                  .ToList();

        public QueueItem? GetById(int id) =>
            _items.FirstOrDefault(i => i.Id == id);

        public QueueItem Add(QueueItemRequest req)
        {
            var item = new QueueItem
            {
                Id        = _nextId++,
                Title     = req.Title,
                Priority  = req.Priority,
                Status    = req.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _items.Add(item);
            return item;
        }

        public QueueItem? Update(int id, QueueItemRequest req)
        {
            var item = GetById(id);
            if (item == null) return null;

            item.Title     = req.Title;
            item.Priority  = req.Priority;
            item.Status    = req.Status;
            item.UpdatedAt = DateTime.UtcNow;
            return item;
        }

        public QueueItem? Complete(int id)
        {
            var item = GetById(id);
            if (item == null) return null;

            item.Status    = "completed";
            item.UpdatedAt = DateTime.UtcNow;
            return item;
        }

        // ── helpers ───────────────────────────────────────────────
        private static int PriorityRank(string p) => p switch
        {
            "high"   => 3,
            "normal" => 2,
            "low"    => 1,
            _        => 0
        };
    }
}
