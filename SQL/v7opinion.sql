
CREATE TABLE IF NOT EXISTS opinion_articles (
    id                 INT AUTO_INCREMENT PRIMARY KEY,
    title              VARCHAR(500)  NOT NULL,
    excerpt            TEXT          NULL,
    author             VARCHAR(200)  NULL,
    read_time_minutes  INT           NOT NULL DEFAULT 1,
    tone               VARCHAR(100)  NULL,
    is_pinned          TINYINT(1)    NOT NULL DEFAULT 0,
    pin_order          INT           NOT NULL DEFAULT 0,
    status             ENUM('draft', 'published', 'archived') NOT NULL DEFAULT 'draft',
    published_at       DATETIME      NULL,
    created_at         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                                     ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_status       (status),
    INDEX idx_pinned_order (is_pinned DESC, pin_order ASC, updated_at DESC),
    INDEX idx_published_at (published_at DESC)
);

-- Optional seed data
INSERT INTO opinion_articles (title, excerpt, author, read_time_minutes, tone, status, published_at)
VALUES
  ('Why Democracy Needs a Rethink',          'Our institutions are struggling to keep pace with the speed of modern crises.',        'A. Sharma',  5, 'critical',   'published', UTC_TIMESTAMP()),
  ('The Case for Universal Basic Income',    'Evidence from pilot programmes around the world is hard to ignore any longer.',        'B. Tamang',  4, 'persuasive', 'published', UTC_TIMESTAMP()),
  ('Social Media Is Rewriting Civic Life',   'Platforms designed for engagement are quietly reshaping how we relate to each other.', 'C. Rai',     3, 'reflective', 'draft',     NULL);