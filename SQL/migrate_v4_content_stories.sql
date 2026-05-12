-- ============================================================
-- News Portal — Migration v4: Content Stories
-- Run this script in your MySQL news_portal database.
-- ============================================================

CREATE TABLE IF NOT EXISTS content_stories (
    id          INT AUTO_INCREMENT PRIMARY KEY,

    title       VARCHAR(500)  NOT NULL,
    category    VARCHAR(100)  NULL,
    summary     TEXT          NULL,           -- content / summary body

    -- draft: not visible to public | published: visible to public
    status      ENUM('draft', 'published') NOT NULL DEFAULT 'draft',

    -- only ONE story may have is_hero=1 AND status='published' at a time
    -- enforced at application level (DemoteCurrentHero helper)
    is_hero     TINYINT(1)    NOT NULL DEFAULT 0,

    is_trending TINYINT(1)    NOT NULL DEFAULT 0,

    created_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                              ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_status     (status),
    INDEX idx_is_hero    (is_hero),
    INDEX idx_is_trending (is_trending),
    INDEX idx_category   (category)
);
