import React, { useState, useEffect } from 'react';
import { newsService } from '../utils/apiService';

export default function NewsDetailModal({ article, onClose }) {
  const [reactions, setReactions] = useState({
    like: 0,
    love: 0,
    haha: 0,
    wow: 0,
    sad: 0,
    angry: 0
  });
  const [userReaction, setUserReaction] = useState(null);
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [showReactions, setShowReactions] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [imageLoaded, setImageLoaded] = useState(false);

  if (!article) return null;

  const reactionEmojis = {
    like: '👍',
    love: '❤️',
    haha: '😂',
    wow: '😮',
    sad: '😢',
    angry: '😠'
  };

  // Load reactions and comments on mount
  useEffect(() => {
    const loadData = async () => {
      try {
        if (article.id) {
          const reactionsData = await newsService.getReactions(article.id);
          if (reactionsData?.reactions) {
            setReactions(prev => ({
              like: reactionsData.reactions.like || 0,
              love: reactionsData.reactions.love || 0,
              haha: reactionsData.reactions.haha || 0,
              wow: reactionsData.reactions.wow || 0,
              sad: reactionsData.reactions.sad || 0,
              angry: reactionsData.reactions.angry || 0
            }));
          }

          const commentsData = await newsService.getComments(article.id);
          if (commentsData?.comments) {
            setComments(commentsData.comments);
          }
        }
      } catch (error) {
        console.error('Failed to load reactions/comments:', error);
      }
    };

    loadData();
  }, [article.id]);

  const formatDate = (dateStr) => {
    if (!dateStr) return 'Today';
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('en-US', { 
        year: 'numeric', 
        month: 'short', 
        day: 'numeric' 
      });
    } catch {
      return 'Today';
    }
  };

  const handleReaction = async (reactionType) => {
    try {
      setIsLoading(true);
      
      if (userReaction === reactionType) {
        // Remove reaction - just update UI for now
        setUserReaction(null);
        setReactions(prev => ({
          ...prev,
          [reactionType]: Math.max(0, prev[reactionType] - 1)
        }));
      } else if (userReaction) {
        // Change reaction
        await newsService.addReaction(article.id, reactionType, 'You');
        setReactions(prev => ({
          ...prev,
          [userReaction]: Math.max(0, prev[userReaction] - 1),
          [reactionType]: prev[reactionType] + 1
        }));
        setUserReaction(reactionType);
      } else {
        // Add new reaction
        await newsService.addReaction(article.id, reactionType, 'You');
        setUserReaction(reactionType);
        setReactions(prev => ({
          ...prev,
          [reactionType]: prev[reactionType] + 1
        }));
      }
      setShowReactions(false);
    } catch (error) {
      console.error('Failed to add reaction:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddComment = async () => {
    if (newComment.trim() && article.id) {
      try {
        setIsLoading(true);
        await newsService.addComment(article.id, newComment.trim(), 'You');
        
        const comment = {
          id: comments.length + 1,
          user_name: 'You',
          comment_text: newComment,
          created_at: new Date().toISOString()
        };
        setComments([comment, ...comments]);
        setNewComment('');
      } catch (error) {
        console.error('Failed to add comment:', error);
      } finally {
        setIsLoading(false);
      }
    }
  };

  const totalReactions = Object.values(reactions).reduce((a, b) => a + b, 0);

  return (
    <div className="news-detail-backdrop" onClick={onClose} role="presentation">
      <article 
        className="news-detail-modal" 
        onClick={(e) => e.stopPropagation()} 
        role="dialog" 
        aria-modal="true"
      >
        <button 
          type="button" 
          className="news-detail-close" 
          onClick={onClose}
          aria-label="Close article"
        >
          ×
        </button>

        {/* Article Image - Responsive */}
        {article.imageUrl && (
          <div className="news-detail-image">
            <img 
              src={article.imageUrl} 
              alt={article.title}
              onLoad={() => setImageLoaded(true)}
              onError={(e) => console.error('Failed to load article image')}
            />
          </div>
        )}

        {/* Header Section */}
        <div className="news-detail-header">
          <h1 className="news-detail-title">{article.title}</h1>
          <div className="news-detail-meta">
            <span>{article.category || article.tag || 'News'}</span>
            <span>{formatDate(article.publishedAt || article.date)}</span>
            <span>{article.author || 'Editorial Team'}</span>
            {article.location && <span>📍 {article.location}</span>}
          </div>
        </div>

        {/* Content Section */}
        <div className="news-detail-content">
          {article.excerpt && (
            <p className="news-detail-excerpt">{article.excerpt}</p>
          )}
          <div className="news-detail-body">
            {article.content || article.excerpt || 'Content not available'}
          </div>
        </div>

        {/* Reactions Section */}
        <div className="reactions-section">
          <div className="reactions-header">
            <span>
              {totalReactions > 0 ? `${totalReactions} reaction${totalReactions !== 1 ? 's' : ''}` : 'No reactions yet'}
            </span>
            <span>
              {comments.length} comment{comments.length !== 1 ? 's' : ''}
            </span>
          </div>

          {/* Reaction Buttons */}
          <div className="reactions-buttons">
            <div className="reaction-button-wrapper">
              <button
                type="button"
                className="reaction-main-button"
                onClick={() => setShowReactions(!showReactions)}
                disabled={isLoading}
              >
                😊 React
              </button>

              {/* Emoji Picker - Smooth animation */}
              {showReactions && (
                <div className="emoji-picker">
                  {Object.entries(reactionEmojis).map(([type, emoji]) => (
                    <button
                      key={type}
                      type="button"
                      className={userReaction === type ? 'active' : ''}
                      onClick={() => handleReaction(type)}
                      title={type.charAt(0).toUpperCase() + type.slice(1)}
                    >
                      {emoji}
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Reaction Count Badges */}
            {Object.entries(reactions).map(([type, count]) => 
              count > 0 && (
                <div key={type} className="reaction-count-badge">
                  {reactionEmojis[type]} <span>{count}</span>
                </div>
              )
            )}
          </div>
        </div>

        {/* Comments Section */}
        <div className="comments-section">
          <h3 className="comments-title">Comments</h3>

          {/* Add Comment Input */}
          <div className="comment-input-wrapper">
            <input
              type="text"
              placeholder="Add a comment..."
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleAddComment()}
            />
            <button
              type="button"
              className="comment-submit-button"
              onClick={handleAddComment}
              disabled={isLoading || !newComment.trim()}
            >
              {isLoading ? 'Posting...' : 'Post'}
            </button>
          </div>

          {/* Comments List */}
          <div className="comments-list">
            {comments.length === 0 ? (
              <p className="no-comments-placeholder">
                No comments yet. Be the first to comment!
              </p>
            ) : (
              comments.map(comment => (
                <div key={comment.id} className="comment-item">
                  <div className="comment-item-header">
                    <strong className="comment-item-username">{comment.user_name}</strong>
                    <span className="comment-item-date">
                      {new Date(comment.created_at).toLocaleDateString()}
                    </span>
                  </div>
                  <p className="comment-item-text">
                    {comment.comment_text}
                  </p>
                </div>
              ))
            )}
          </div>
        </div>
      </article>
    </div>
  );
}
