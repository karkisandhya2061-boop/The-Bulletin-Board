

CREATE TABLE IF NOT EXISTS culture_articles (
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
INSERT INTO culture_articles (title, excerpt, author, read_time_minutes, tone, status, published_at)
VALUES
  ('Local Art Scene Thrives Post-Pandemic',       'Galleries and studios are seeing record footfall as communities reconnect through art.', 'Maya Sen',   3, 'uplifting',  'published', UTC_TIMESTAMP()),
  ('Traditional Festivals Return to City Streets', 'After years of restrictions, cultural celebrations are back in full colour.',           'Rajan Malla', 2, 'joyful',     'published', UTC_TIMESTAMP()),
  ('New Wave Cinema Takes Centre Stage',           'Independent filmmakers are reshaping storytelling with bold, culturally rich narratives.', 'Lin Patel', 4, 'inspiring',  'draft',     NULL);