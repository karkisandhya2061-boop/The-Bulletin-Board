-- Create portalrealdb schema
-- Tables: users, categories, news_articles

CREATE DATABASE IF NOT EXISTS portalrealdb;
USE portalrealdb;

-- Users table
CREATE TABLE IF NOT EXISTS users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  first_name VARCHAR(100),
  last_name VARCHAR(100),
  email VARCHAR(255) UNIQUE NOT NULL,
  phone VARCHAR(20),
  address VARCHAR(255),
  country VARCHAR(100),
  password VARCHAR(255) NOT NULL,
  role ENUM('user', 'admin') DEFAULT 'user',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Categories table
CREATE TABLE IF NOT EXISTS categories (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) UNIQUE NOT NULL,
  slug VARCHAR(100),
  description TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- News Articles table
CREATE TABLE IF NOT EXISTS news_articles (
  id INT AUTO_INCREMENT PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  content LONGTEXT,
  excerpt VARCHAR(500),
  category_id INT,
  author_id INT,
  status ENUM('draft', 'published', 'archived') DEFAULT 'draft',
  published_at TIMESTAMP NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE SET NULL,
  FOREIGN KEY (author_id) REFERENCES users(id) ON DELETE SET NULL
);

-- Insert sample categories
INSERT INTO categories (name, description) VALUES
  ('Politics', 'Political news and updates'),
  ('Tech', 'Technology and innovation'),
  ('Science', 'Scientific discoveries and research'),
  ('Culture', 'Cultural events and trends'),
  ('Opinion', 'Opinion pieces and analysis');

-- Insert sample admin user
INSERT INTO users (first_name, last_name, email, password, role) VALUES
  ('Admin', 'User', 'admin@portal.com', SHA2('admin', 256), 'admin');

-- Insert sample news article
INSERT INTO news_articles (title, content, excerpt, category_id, author_id, status, published_at) VALUES
  ('Breaking News', 'This is a sample breaking news article.', 'Breaking news excerpt...', 1, 1, 'published', NOW());
