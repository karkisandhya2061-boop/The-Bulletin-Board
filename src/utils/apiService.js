/**
 * News Portal API Service
 * High-level API calls for the news portal
 */

import { get, post, put, deleteReq, setToken, clearToken, getToken } from './api';

// ─── AUTH ──────────────────────────────────────────────────────
export const authService = {
  /**
   * User Login
   */
  login: async (email, password) => {
    const response = await post('/auth/login', { email, password });
    if (response.accessToken) {
      setToken(response.accessToken);
    }
    return response;
  },

  /**
   * User Signup
   */
  signup: async (userData) => {
    const response = await post('/auth/signup', userData);
    if (response.accessToken) {
      setToken(response.accessToken);
    }
    return response;
  },

  /**
   * Admin Login
   */
  adminLogin: async (username, password) => {
    const response = await post('/auth/admin-login', { username, password });
    if (response.accessToken) {
      setToken(response.accessToken);
    }
    return response;
  },

  /**
   * Logout
   */
  logout: () => {
    clearToken();
  },

  /**
   * Get current user
   */
  getCurrentUser: async () => {
    return await get('/auth/me');
  },
};

// ─── NEWS ──────────────────────────────────────────────────────
export const newsService = {
  /**
   * Get feed (published stories grouped by type)
   */
  getFeed: async () => {
    return await get('/news/feed');
  },

  /**
   * Get stories by category
   */
  getByCategory: async (category) => {
    return await get(`/news/category/${category}`);
  },

  /**
   * Get trending stories
   */
  getTrending: async () => {
    return await get('/news/trending');
  },

  /**
   * Get story by ID
   */
  getStory: async (id) => {
    return await get(`/news/${id}`);
  },

  /**
   * Create news article with image (admin only)
   */
  createNews: async (newsData) => {
    return await post('/news/create', newsData);
  },

  /**
   * Create story (admin only)
   */
  createStory: async (storyData) => {
    return await post('/news', storyData);
  },

  /**
   * Update story (admin only)
   */
  updateStory: async (id, storyData) => {
    return await put(`/news/${id}`, storyData);
  },

  /**
   * Delete story (admin only)
   */
  deleteStory: async (id) => {
    return await deleteReq(`/news/${id}`);
  },

  /**
   * Add reaction to news article
   */
  addReaction: async (articleId, reactionType, userName = 'User', userId = 0) => {
    return await post(`/news/${articleId}/reactions`, { 
      userId, 
      userName,
      userName,
      reactionType 
    });
  },

  /**
   * Get reactions for article
   */
  getReactions: async (articleId) => {
    return await get(`/news/${articleId}/reactions`);
  },

  /**
   * Add comment to article
   */
  addComment: async (articleId, commentText, userName = 'Anonymous', userId = 0) => {
    return await post(`/news/${articleId}/comments`, {
      userId,
      userName,
      commentText
    });
  },

  /**
   * Get comments for article
   */
  getComments: async (articleId) => {
    return await get(`/news/${articleId}/comments`);
  },

  /**
   * Get all notifications
   */
  getNotifications: async () => {
    return await get('/news/notifications');
  },

  /**
   * Get unread notification count
   */
  getUnreadNotificationCount: async () => {
    return await get('/news/notifications/unread-count');
  },

  /**
   * Mark notification as read
   */
  markNotificationRead: async (notificationId) => {
    return await post(`/news/notifications/${notificationId}/read`, {});
  },
};

// ─── ADS ──────────────────────────────────────────────────────
export const adsService = {
  /**
   * Get random ad
   */
  getRandomAd: async () => {
    return await get('/ads/random');
  },

  /**
   * Get all ads (admin only)
   */
  getAds: async () => {
    return await get('/ads');
  },

  /**
   * Create ad (admin only)
   */
  createAd: async (adData) => {
    return await post('/ads', adData);
  },
};

// ─── CATEGORIES ──────────────────────────────────────────────────────
export const categoriesService = {
  /**
   * Get all categories
   */
  getAll: async () => {
    return await get('/categories');
  },

  /**
   * Get stories from a category
   */
  getStories: async (categoryId) => {
    return await get(`/categories/${categoryId}/stories`);
  },
};

// ─── QUEUE (Admin) ──────────────────────────────────────────────────────
export const queueService = {
  /**
   * Get admin queue
   */
  getQueue: async () => {
    return await get('/queue');
  },

  /**
   * Add item to queue
   */
  addItem: async (item) => {
    return await post('/queue', item);
  },

  /**
   * Remove item from queue
   */
  removeItem: async (id) => {
    return await deleteReq(`/queue/${id}`);
  },
};

// ─── CHAT ──────────────────────────────────────────────────────
export const chatService = {
  /**
   * Send chat message
   */
  sendMessage: async (message) => {
    return await post('/chat', { message });
  },

  /**
   * Get chat history
   */
  getHistory: async () => {
    return await get('/chat/history');
  },
};

// ─── RE-EXPORT UTILITIES ──────────────────────────────────────
export { getToken, setToken, clearToken };
