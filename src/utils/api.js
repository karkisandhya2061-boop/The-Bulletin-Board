/**
 * API Utility
 * Handles all HTTP requests with JWT token management
 */

const API_BASE_URL = '/api';

/**
 * Get JWT token from localStorage
 */
const getToken = () => {
  return localStorage.getItem('jwt_token');
};

/**
 * Store JWT token in localStorage
 */
const setToken = (token) => {
  if (token) {
    localStorage.setItem('jwt_token', token);
  }
};

/**
 * Remove JWT token from localStorage
 */
const clearToken = () => {
  localStorage.removeItem('jwt_token');
};

/**
 * Make HTTP request with JWT token
 */
const request = async (endpoint, options = {}) => {
  const url = `${API_BASE_URL}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  const token = getToken();
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const config = {
    ...options,
    headers,
  };

  try {
    const response = await fetch(url, config);

    // Handle 401 Unauthorized
    if (response.status === 401) {
      clearToken();
      window.location.href = '/'; // Redirect to home
    }

    // Parse JSON response
    const data = await response.json().catch(() => ({}));

    if (!response.ok) {
      const error = new Error(data.message || `HTTP Error: ${response.status}`);
      error.status = response.status;
      error.data = data;
      throw error;
    }

    return data;
  } catch (error) {
    console.error(`API Error [${endpoint}]:`, error);
    throw error;
  }
};

/**
 * GET request
 */
export const get = (endpoint, options = {}) => {
  return request(endpoint, { ...options, method: 'GET' });
};

/**
 * POST request
 */
export const post = (endpoint, body, options = {}) => {
  return request(endpoint, {
    ...options,
    method: 'POST',
    body: JSON.stringify(body),
  });
};

/**
 * PUT request
 */
export const put = (endpoint, body, options = {}) => {
  return request(endpoint, {
    ...options,
    method: 'PUT',
    body: JSON.stringify(body),
  });
};

/**
 * DELETE request
 */
export const deleteReq = (endpoint, options = {}) => {
  return request(endpoint, { ...options, method: 'DELETE' });
};

export { getToken, setToken, clearToken };
