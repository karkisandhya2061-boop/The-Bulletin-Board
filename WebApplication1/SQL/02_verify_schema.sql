-- Migration script to set up portalrealdb
-- Run this script in MySQL to create the database and tables

USE portalrealdb;

-- Verify tables exist
SHOW TABLES;

-- Check users table structure
DESCRIBE users;

-- Check categories table structure  
DESCRIBE categories;

-- Check news_articles table structure
DESCRIBE news_articles;

-- Verify data
SELECT COUNT(*) as user_count FROM users;
SELECT COUNT(*) as category_count FROM categories;
SELECT COUNT(*) as article_count FROM news_articles;
