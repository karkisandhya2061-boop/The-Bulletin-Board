-- Create notifications table for admin notifications system

USE portalrealdb;

-- Admin notifications for reactions and comments
CREATE TABLE IF NOT EXISTS admin_notifications (
  id INT AUTO_INCREMENT PRIMARY KEY,
  news_article_id INT NOT NULL,
  notification_type ENUM('reaction', 'comment') NOT NULL,
  user_name VARCHAR(255),
  action_text VARCHAR(500),
  is_read BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (news_article_id) REFERENCES news_articles(id) ON DELETE CASCADE,
  INDEX idx_is_read (is_read),
  INDEX idx_created_at (created_at DESC)
);

-- Verify table
SHOW TABLES LIKE 'admin_%';
DESC admin_notifications;
