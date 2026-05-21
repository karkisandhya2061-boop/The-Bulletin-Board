-- Create reactions and comments tables for news articles

USE portalrealdb;

-- Reactions table (likes, loves, haha, wow, sad, angry - Facebook style)
CREATE TABLE IF NOT EXISTS news_reactions (
  id INT AUTO_INCREMENT PRIMARY KEY,
  news_article_id INT NOT NULL,
  user_id INT,
  reaction_type ENUM('like', 'love', 'haha', 'wow', 'sad', 'angry') DEFAULT 'like',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (news_article_id) REFERENCES news_articles(id) ON DELETE CASCADE,
  UNIQUE KEY unique_user_reaction (news_article_id, user_id),
  INDEX idx_news_article (news_article_id)
);

-- Comments table
CREATE TABLE IF NOT EXISTS news_comments (
  id INT AUTO_INCREMENT PRIMARY KEY,
  news_article_id INT NOT NULL,
  user_id INT,
  user_name VARCHAR(255),
  comment_text TEXT NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (news_article_id) REFERENCES news_articles(id) ON DELETE CASCADE,
  INDEX idx_news_article (news_article_id),
  INDEX idx_created_at (created_at DESC)
);

-- Verify tables
SHOW TABLES LIKE 'news_%';
DESC news_reactions;
DESC news_comments;
