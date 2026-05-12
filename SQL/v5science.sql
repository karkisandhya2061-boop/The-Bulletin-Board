
CREATE TABLE IF NOT EXISTS science_articles (
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

    INDEX idx_status        (status),
    INDEX idx_pinned_order  (is_pinned DESC, pin_order ASC, updated_at DESC),
    INDEX idx_published_at  (published_at DESC)
);

-- Optional seed data
INSERT INTO science_articles (title, excerpt, author, read_time_minutes, tone, status, published_at)
VALUES
  ('Mars Rover Sends Unprecedented Images',  'Scientists are analysing the clearest photographs of the Martian surface yet.', 'Ann Cole',  5, 'wonder',      'published', UTC_TIMESTAMP()),
  ('AI Breakthrough in Protein Folding',     'Researchers have mapped thousands of new protein structures using AI models.',   'Sam Park',  4, 'optimistic',  'published', UTC_TIMESTAMP()),
  ('New Species Discovered in Amazon Basin', 'A joint expedition uncovered three previously unknown amphibian species.',       'Lena Cruz', 3, 'fascinating', 'draft',     NULL);