-- ============================================================
-- News Content Module - Migration v3
-- Run this script in your MySQL news_portal database
-- ============================================================

-- ── Stories table ────────────────────────────────────────────
-- Supports buckets: heroStory | featuredSideStories | trendingStories
-- (tickerItems have their own table below)

CREATE TABLE IF NOT EXISTS stories (
    id                 INT AUTO_INCREMENT PRIMARY KEY,

    -- Content bucket this story belongs to
    bucket             ENUM('heroStory', 'featuredSideStories', 'trendingStories')
                           NOT NULL DEFAULT 'featuredSideStories',

    category           VARCHAR(100)  NULL,         -- category / tag
    title              VARCHAR(500)  NOT NULL,
    excerpt            TEXT          NULL,          -- body summary
    author             VARCHAR(200)  NULL,
    read_time_minutes  INT           NOT NULL DEFAULT 1,
    tone               VARCHAR(100)  NULL,          -- tone / theme token
    is_pinned          TINYINT(1)    NOT NULL DEFAULT 0,  -- pinned to top of trending
    pin_order          INT           NOT NULL DEFAULT 0,  -- lower = higher when pinned

    status             ENUM('draft', 'published', 'archived')
                           NOT NULL DEFAULT 'draft',

    published_at       DATETIME      NULL,
    created_at         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                                     ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_bucket        (bucket),
    INDEX idx_status        (status),
    INDEX idx_bucket_status (bucket, status),
    INDEX idx_category      (category),
    INDEX idx_published_at  (published_at DESC)
);

-- ── Ticker items table ───────────────────────────────────────
-- Text-only entries that scroll in the news ticker

CREATE TABLE IF NOT EXISTS ticker_items (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    text        VARCHAR(1000) NOT NULL,
    status      ENUM('draft', 'published', 'archived')
                    NOT NULL DEFAULT 'published',
    sort_order  INT           NOT NULL DEFAULT 0,
    created_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                              ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_status     (status),
    INDEX idx_sort_order (sort_order)
);

-- ── Sample seed data (optional — delete if not needed) ───────

INSERT INTO stories (bucket, category, title, excerpt, author, read_time_minutes, tone, status, published_at)
VALUES
  ('heroStory',           'Politics',      'Breaking: National Budget Announced',      'The government unveiled the annual budget with record infrastructure spending.', 'Jane Doe',  4, 'serious',    'published', UTC_TIMESTAMP()),
  ('featuredSideStories', 'Technology',    'AI Transforms Healthcare Diagnostics',     'New AI tools are helping doctors detect diseases earlier than ever before.',      'John Smith', 3, 'optimistic', 'published', UTC_TIMESTAMP()),
  ('featuredSideStories', 'Environment',   'Reforestation Drive Hits 1 Million Trees', 'Community volunteers celebrate a major milestone in the green initiative.',       'Sara Lee',  2, 'positive',   'published', UTC_TIMESTAMP()),
  ('trendingStories',     'Sports',        'National Team Advances to Semi-Finals',    'A stunning comeback secured their place in the tournament's final four.',         'Mike Ray',  2, 'exciting',   'published', UTC_TIMESTAMP()),
  ('trendingStories',     'Science',       'Mars Rover Sends Unprecedented Images',    'Scientists are analysing the clearest photographs of the Martian surface yet.',   'Ann Cole',  5, 'wonder',     'published', UTC_TIMESTAMP());

INSERT INTO ticker_items (text, status, sort_order)
VALUES
  ('Markets close higher as investors await Fed decision',          'published', 1),
  ('Weather alert: Heavy rain expected across southern provinces',  'published', 2),
  ('Election commission confirms voter registration deadline',      'published', 3);


-- ============================================================
-- News Content Module - Migration v3.1 (3.3.2 Public Retrieval)
-- Adds is_pinned and pin_order to stories for trending ordering.
-- Run after migrate_v3_news.sql if table already exists,
-- OR the CREATE TABLE above already includes these columns on fresh installs.
-- ============================================================

ALTER TABLE stories
    ADD COLUMN IF NOT EXISTS is_pinned  TINYINT(1) NOT NULL DEFAULT 0  AFTER tone,
    ADD COLUMN IF NOT EXISTS pin_order  INT        NOT NULL DEFAULT 0  AFTER is_pinned;

-- Index to make the pinned-first ordering fast
CREATE INDEX IF NOT EXISTS idx_pinned_order ON stories (is_pinned DESC, pin_order ASC, updated_at DESC);
