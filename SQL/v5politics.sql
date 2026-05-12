

CREATE TABLE IF NOT EXISTS politics_articles (
    id                 INT AUTO_INCREMENT PRIMARY KEY,

    title              VARCHAR(500)  NOT NULL,
    excerpt            TEXT          NULL,          -- short summary shown in cards
    content            LONGTEXT      NULL,          -- full article body
    author             VARCHAR(200)  NULL,
    image_url          VARCHAR(1000) NULL,          -- hero / thumbnail image
    tags               VARCHAR(500)  NULL,          -- comma-separated tags e.g. "election,budget"

    read_time_minutes  INT           NOT NULL DEFAULT 1,
    is_featured        TINYINT(1)    NOT NULL DEFAULT 0,  -- promoted to top of politics feed
    is_pinned          TINYINT(1)    NOT NULL DEFAULT 0,
    pin_order          INT           NOT NULL DEFAULT 0,  -- lower = higher position

    status             ENUM('draft', 'published', 'archived')
                           NOT NULL DEFAULT 'draft',

    published_at       DATETIME      NULL,
    created_at         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                                     ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_status        (status),
    INDEX idx_featured      (is_featured),
    INDEX idx_published_at  (published_at DESC),
    INDEX idx_pinned_order  (is_pinned DESC, pin_order ASC, updated_at DESC)
);

-- ── Sample seed data (optional — delete if not needed) ───────

INSERT INTO politics_articles
    (title, excerpt, content, author, image_url, tags, read_time_minutes, is_featured, status, published_at)
VALUES
(
    'Parliament Passes Historic Infrastructure Bill',
    'Lawmakers approved a sweeping infrastructure package worth billions in new spending.',
    'In a landmark vote, parliament passed the Infrastructure Investment Act, allocating funds across roads, bridges, and digital connectivity for underserved regions. Supporters argue the bill will create hundreds of thousands of jobs over the next decade, while critics raise concerns about deficit spending.',
    'Jane Doe', '', 'parliament,infrastructure,economy', 5, 1, 'published', UTC_TIMESTAMP()
),
(
    'Opposition Leaders Call for Early Elections',
    'Three opposition parties have jointly demanded the prime minister dissolve parliament and hold snap elections.',
    'Following a series of cabinet reshuffles and falling approval ratings, opposition leaders convened a press conference demanding an early general election. The ruling coalition rejected the call, citing ongoing budget negotiations.',
    'John Smith', '', 'election,opposition,coalition', 3, 0, 'published', UTC_TIMESTAMP()
),
(
    'New Electoral Reform Bill Under Review',
    'A cross-party committee has begun reviewing proposed changes to the electoral system.',
    'The Electoral Reform Committee met for the first time this week to examine proposals including ranked-choice voting and updated constituency boundaries. The review is expected to take six months before any draft legislation is tabled.',
    'Sara Lee', '', 'election,reform,committee', 4, 0, 'published', UTC_TIMESTAMP()
);