-- Fix image_url column type for better compatibility with large base64 strings
USE portalrealdb;

-- Check current column info
DESC news_articles;

-- Modify image_url column to MEDIUMTEXT if needed (supports up to 16MB)
-- This is more reliable than LONGTEXT for base64 image data
ALTER TABLE news_articles MODIFY COLUMN image_url MEDIUMTEXT;

-- Verify the change
SHOW COLUMNS FROM news_articles WHERE Field = 'image_url';

-- Select a sample to verify data integrity
SELECT id, title, LENGTH(image_url) as image_size FROM news_articles WHERE image_url IS NOT NULL AND image_url != '' LIMIT 5;
