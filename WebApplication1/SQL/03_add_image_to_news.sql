-- Add image_url column to news_articles table
USE portalrealdb;

ALTER TABLE news_articles ADD COLUMN image_url LONGTEXT AFTER excerpt;
ALTER TABLE news_articles ADD COLUMN location VARCHAR(255) AFTER published_at;

-- Update existing records to have default values
UPDATE news_articles SET image_url = '' WHERE image_url IS NULL;
UPDATE news_articles SET location = 'General' WHERE location IS NULL;

-- Verify the changes
DESC news_articles;
