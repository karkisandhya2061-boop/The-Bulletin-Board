-- ============================================================
-- Chat feature migration
-- Run against the news_portal database
-- ============================================================

-- Stores one row per conversation thread
CREATE TABLE IF NOT EXISTS chat_conversations (
    id         INT AUTO_INCREMENT PRIMARY KEY,
    user_id    INT          NULL,                          -- NULL = anonymous guest
    session_id VARCHAR(128) NOT NULL UNIQUE,               -- client UUID
    title      VARCHAR(255) NOT NULL DEFAULT 'New conversation',
    created_at DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_chat_session   (session_id),
    INDEX idx_chat_user      (user_id),
    INDEX idx_chat_updated   (updated_at)
);

-- Stores every user and assistant message
CREATE TABLE IF NOT EXISTS chat_messages (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    conversation_id INT          NOT NULL,
    role            ENUM('user','assistant') NOT NULL,
    content         TEXT         NOT NULL,
    created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (conversation_id)
        REFERENCES chat_conversations(id) ON DELETE CASCADE,
    INDEX idx_msg_conversation (conversation_id),
    INDEX idx_msg_created      (created_at)
);