import React, { useState, useEffect } from 'react';
import { newsService } from '../utils/apiService';

export default function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isLoading, setIsLoading] = useState(false);

  // Load notifications on component mount
  useEffect(() => {
    loadNotifications();
    
    // Poll for new notifications every 30 seconds
    const interval = setInterval(loadNotifications, 30000);
    return () => clearInterval(interval);
  }, []);

  const loadNotifications = async () => {
    try {
      const response = await newsService.getNotifications();
      if (response.notifications) {
        setNotifications(response.notifications);
        
        // Count unread
        const unread = response.notifications.filter(n => !n.isRead).length;
        setUnreadCount(unread);
      }
    } catch (error) {
      console.error('Failed to load notifications:', error);
    }
  };

  const handleMarkAsRead = async (notificationId) => {
    try {
      await newsService.markNotificationRead(notificationId);
      
      // Update local state
      setNotifications(prev =>
        prev.map(n => n.id === notificationId ? { ...n, isRead: true } : n)
      );
      
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch (error) {
      console.error('Failed to mark notification as read:', error);
    }
  };

  const handleClearAll = async () => {
    // Mark all unread notifications as read
    const unreadNotifications = notifications.filter(n => !n.isRead);
    
    for (const notification of unreadNotifications) {
      await handleMarkAsRead(notification.id);
    }
  };

  const formatTime = (dateString) => {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;

    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;

    return date.toLocaleDateString();
  };

  const getNotificationIcon = (type) => {
    switch (type) {
      case 'reaction':
        return '👍';
      case 'comment':
        return '💬';
      default:
        return '🔔';
    }
  };

  return (
    <div className="notification-bell-container">
      {/* Bell Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="notification-bell"
        title="Notifications"
      >
        🔔
        {unreadCount > 0 && (
          <span className="notification-badge">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {/* Notification Dropdown */}
      {isOpen && (
        <div className="notification-dropdown">
          <div className="notification-header">
            <h3>Notifications</h3>
            <button
              onClick={() => setIsOpen(false)}
              className="notification-close"
              title="Close"
            >
              ×
            </button>
          </div>

          {notifications.length === 0 ? (
            <div className="notification-empty">
              <p>No notifications yet</p>
            </div>
          ) : (
            <>
              <div className="notifications-list">
                {notifications.map(notification => (
                  <div
                    key={notification.id}
                    className={`notification-item ${notification.isRead ? 'read' : 'unread'}`}
                    onClick={() => !notification.isRead && handleMarkAsRead(notification.id)}
                  >
                    <span className="notification-icon">
                      {getNotificationIcon(notification.type)}
                    </span>
                    
                    <div className="notification-content">
                      <p className="notification-action">
                        <strong>{notification.userName}</strong> {
                          notification.type === 'reaction' 
                            ? 'reacted to your post' 
                            : 'commented on your post'
                        }
                      </p>
                      <p className="notification-text">{notification.actionText}</p>
                      <small className="notification-time">
                        {formatTime(notification.createdAt)}
                      </small>
                    </div>

                    {!notification.isRead && (
                      <div className="notification-unread-dot"></div>
                    )}
                  </div>
                ))}
              </div>

              {unreadCount > 0 && (
                <div className="notification-footer">
                  <button
                    onClick={handleClearAll}
                    className="notification-clear-btn"
                  >
                    Mark all as read
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
}
