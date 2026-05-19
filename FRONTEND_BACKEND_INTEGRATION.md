# Frontend-Backend Integration Setup Guide

## Overview
The frontend React app (Vite) is now fully connected to the C# .NET backend API. The frontend fetches live data from the backend for news, authentication, ads, and other content.

## Running the Application

### Prerequisites
- **Node.js** (v16+) - for frontend development
- **.NET 8+** - for backend development
- **MySQL** - for database (configured on localhost)

### Step 1: Start the Backend
```bash
# Navigate to the backend directory
cd WebApplication1

# Run the backend server
dotnet run
```

**Backend Info:**
- URL: `http://localhost:5177` (HTTP) or `https://localhost:7247` (HTTPS)
- API Base: `/auth`, `/news`, `/ads`, `/categories`, `/queue`, `/chat`
- Database: MySQL configured in `appsettings.Development.json`

### Step 2: Start the Frontend (in another terminal)
```bash
# From workspace root
npm run dev
```

**Frontend Info:**
- URL: `http://localhost:5173` (default Vite port)
- Dev Server includes proxy to backend at `/api`
- Automatically reloads on file changes

## API Integration Details

### Frontend Utilities Created

#### 1. `src/utils/api.js` - Low-level HTTP Client
Handles:
- JWT token management (get/set/clear)
- HTTP requests (GET, POST, PUT, DELETE)
- Error handling and 401 redirect
- Automatic token injection in headers

**Functions:**
- `get(endpoint, options?)` - GET request
- `post(endpoint, body, options?)` - POST request
- `put(endpoint, body, options?)` - PUT request
- `deleteReq(endpoint, options?)` - DELETE request
- `getToken()` - Get stored JWT token
- `setToken(token)` - Store JWT token
- `clearToken()` - Remove JWT token

#### 2. `src/utils/apiService.js` - High-level API Service
Organized endpoints for:

**Authentication (`authService`):**
- `login(email, password)` - User login
- `signup(userData)` - User registration
- `adminLogin(username, password)` - Admin console access
- `logout()` - Clear session
- `getCurrentUser()` - Fetch current user

**News (`newsService`):**
- `getFeed()` - Get all published stories grouped by type
- `getByCategory(category)` - Stories by category
- `getTrending()` - Trending stories
- `getStory(id)` - Single story by ID
- `createStory(storyData)` - Admin: Create new story
- `updateStory(id, storyData)` - Admin: Update story
- `deleteStory(id)` - Admin: Delete story

**Ads (`adsService`):**
- `getRandomAd()` - Get random ad for display
- `getAds()` - Admin: Get all ads
- `createAd(adData)` - Admin: Create ad

**Categories (`categoriesService`):**
- `getAll()` - List all categories
- `getStories(categoryId)` - Stories in category

**Queue (`queueService`):**
- `getQueue()` - Admin editorial queue
- `addItem(item)` - Add to queue
- `removeItem(id)` - Remove from queue

**Chat (`chatService`):**
- `sendMessage(message)` - Send chat message
- `getHistory()` - Get message history

### Vite Proxy Configuration

The `vite.config.js` includes a proxy that routes all `/api` requests to the backend:

```javascript
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:5177',
      changeOrigin: true,
      rewrite: (path) => path.replace(/^\/api/, ''),
    },
  },
}
```

This means:
- Frontend call: `GET /api/news/feed`
- Routes to backend: `GET http://localhost:5177/news/feed`

### App.jsx Updates

**State Management:**
- Now uses `useEffect` to fetch feed on component mount
- Uses API service for authentication instead of hardcoded login
- Stores JWT token in localStorage
- Checks for existing token on app load (auto-login if available)

**Key Changes:**
1. Login now calls `authService.login()` instead of checking hardcoded credentials
2. Signup calls `authService.signup()` to create account
3. Admin login calls `authService.adminLogin()`
4. Feed data fetches from `newsService.getFeed()`
5. Fallback to default data if API fails (graceful degradation)

## Troubleshooting

### Frontend Can't Connect to Backend
**Error:** `Failed to fetch /api/...`

**Solutions:**
1. Check backend is running: `http://localhost:5177/swagger`
2. Verify Vite config proxy is correct
3. Check CORS is enabled in backend (it is)
4. Clear browser cache and refresh

### JWT Token Issues
**Error:** Redirects to login page unexpectedly

**Solutions:**
1. Token stored in `localStorage` as `jwt_token`
2. If needed, clear with: `localStorage.clear()` in browser console
3. Token expires after 60 minutes (set in `appsettings.json`)

### Database Connection Issues
**Error:** 500 error from backend

**Solutions:**
1. Verify MySQL is running on `localhost`
2. Check connection string in `appsettings.Development.json`
3. Run migrations if needed: `dotnet ef database update`

## Example Usage

### Making API Calls in Components

```javascript
import { newsService, authService } from './utils/apiService';

// In a React component
useEffect(() => {
  // Fetch all published stories
  const fetchNews = async () => {
    try {
      const data = await newsService.getFeed();
      setNews(data);
    } catch (error) {
      console.error('Failed to fetch:', error);
    }
  };
  
  fetchNews();
}, []);

// Login
const handleLogin = async (email, password) => {
  try {
    await authService.login(email, password);
    // Token automatically stored and attached to future requests
  } catch (error) {
    console.error('Login failed:', error);
  }
};
```

## Security Notes

- JWT tokens are stored in localStorage (client-side)
- Tokens are automatically attached to all API requests
- 401 Unauthorized responses redirect user to login
- CORS is configured to accept requests from `http://localhost:3000` and `http://localhost:5173`
- Passwords are hashed on backend before storage

## Next Steps

- Test authentication flow (login, signup, logout)
- Verify news feed loads with real data from backend
- Test admin console functionality
- Add error boundaries for better error handling
- Consider adding loading/error states to components
