using WebApplication1.Models;

namespace WebApplication1.Services
{
    /// <summary>
    /// Singleton in-memory store for ads.
    /// Swap for a DB-backed service later without touching the controller.
    /// </summary>
    public class AdStore
    {
        private readonly List<Ad> _ads  = new();
        private readonly Random   _rng  = new();
        private int _nextId = 1;

        public List<Ad> GetAll() =>
            _ads.OrderByDescending(a => a.CreatedAt).ToList();

        public Ad? GetById(int id) =>
            _ads.FirstOrDefault(a => a.Id == id);

        public Ad? GetRandom()
        {
            var active = _ads.Where(a => a.IsActive).ToList();
            return active.Count == 0 ? null : active[_rng.Next(active.Count)];
        }

        public Ad Add(AdRequest req)
        {
            var ad = new Ad
            {
                Id        = _nextId++,
                Title     = req.Title,
                Copy      = req.Copy,
                Cta       = req.Cta,
                IsActive  = req.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _ads.Add(ad);
            return ad;
        }

        public Ad? Update(int id, AdRequest req)
        {
            var ad = GetById(id);
            if (ad == null) return null;

            ad.Title     = req.Title;
            ad.Copy      = req.Copy;
            ad.Cta       = req.Cta;
            ad.IsActive  = req.IsActive;
            ad.UpdatedAt = DateTime.UtcNow;
            return ad;
        }

        public bool Delete(int id)
        {
            var ad = GetById(id);
            if (ad == null) return false;
            _ads.Remove(ad);
            return true;
        }
    }
}
