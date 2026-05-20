import { useMemo, useState, useEffect } from 'react';
import NewsCard from './components/NewsCard';
import NewsDetailModal from './components/NewsDetailModal';
import Chatbot from './components/Chatbot';
import NotificationBell from './components/NotificationBell';
import { authService, newsService, adsService, getToken } from './utils/apiService';
import {
  buildSectionExperience,
  getSectionFromHash,
  homeSections,
  normalizeSectionKey,
} from './utils/sectionContent';

// Fallback data in case API calls fail
const defaultTickerItems = [
  'Global Summit addresses climate crisis in urgent session...',
  'Market indices reach historic high following tech breakthrough...',
  'New archaeological discovery rewrites early trade history...',
];

const defaultHeroStory = {
  tag: 'World News',
  title:
    'The Architecture of the Future: How Digital Twins are Reshaping Urban Living',
  author: 'By Elena Vance',
  readTime: '12 Min Read',
  imageClass: 'hero-visual',
};

const defaultFeaturedSideStories = [
  {
    id: 1,
    category: 'Tech',
    title: 'The Silicon Valley Pivot: Why AI is Swallowing SaaS',
    excerpt:
      'Industry giants are restructuring entire divisions as LLMs become the core interface of modern software.',
    tone: 'blue',
    tag: 'Featured',
  },
  {
    id: 2,
    category: 'Culture',
    title: 'Retrograde: The Surprising Resurgence of Analog Film',
    excerpt:
      'Gen Z is abandoning pixel-perfect smartphone cameras for the grain and soul of 35mm photography.',
    tone: 'amber',
    tag: 'Popular',
  },
];

const defaultTrendingStories = [
  {
    id: 1,
    category: 'Markets',
    title: 'Investors react to the latest earnings cycle',
    excerpt: 'A sharp rise in clean energy and AI sectors lifted the broader market.',
    author: 'Financial Desk',
    time: '4 min read',
    tone: 'blue',
    tag: 'Hot',
  },
  {
    id: 2,
    category: 'Education',
    title: 'Students collaborate on a new campus research exchange',
    excerpt: 'Cross-disciplinary teams are publishing faster with a shared digital lab.',
    author: 'Campus Desk',
    time: '6 min read',
    tone: 'teal',
    tag: 'New',
  },
  {
    id: 3,
    category: 'Science',
    title: 'Satellite imagery shows how cities are cooling streets',
    excerpt: 'Reflective materials and shade planning are reducing local heat spikes.',
    author: 'Science Desk',
    time: '8 min read',
    tone: 'amber',
    tag: 'Insight',
  },
  {
    id: 4,
    category: 'Sports',
    title: 'A marathon start line reimagined for a bigger crowd',
    excerpt: 'A fresh layout gives spectators better views and keeps runners moving.',
    author: 'Sports Desk',
    time: '5 min read',
    tone: 'rose',
    tag: 'Live',
  },
];

const loginFormDefaults = {
  email: '',
  password: '',
};

const signupFormDefaults = {
  name: 'Sandhya Tiwari',
  email: 'sandhya@gmail.com',
  password: 'password123',
};

const adminLoginDefaults = {
  username: '',
  password: '',
};

const adminHighlights = [
  {
    label: 'Pending Reviews',
    value: '18',
    detail: 'Stories waiting for editorial approval',
  },
  {
    label: 'Active Campaigns',
    value: '7',
    detail: 'Sponsored placements currently running',
  },
  {
    label: 'Homepage CTR',
    value: '4.8%',
    detail: 'Engagement across the last 24 hours',
  },
];

const adminQueue = [
  {
    id: 1,
    title: 'Approve investigative feature on urban mobility',
    meta: 'Editorial review due in 2 hours',
  },
  {
    id: 2,
    title: 'Refresh promotional placements for the science desk',
    meta: 'Ad inventory update needed today',
  },
  {
    id: 3,
    title: 'Audit the trending module for duplicate stories',
    meta: 'Cross-check source tags and thumbnails',
  },
];

const adLibrary = [
  {
    label: 'Sponsored Insight',
    title: 'Upgrade your newsroom workflow',
    copy: 'Run faster editorial approvals with a unified planning board and shared notes.',
    cta: 'Explore Suite',
    tone: 'blue',
  },
  {
    label: 'Partner Offer',
    title: 'A premium analytics layer for publishers',
    copy: 'Track story lift, audience retention, and campaign value from one dashboard.',
    cta: 'View Demo',
    tone: 'amber',
  },
  {
    label: 'Breaking Promotion',
    title: 'Turn attention into subscription growth',
    copy: 'Convert high-intent readers with tailored placement and dynamic offers.',
    cta: 'See Pricing',
    tone: 'teal',
  },
];

const demoRole = 'user';
const initialSectionKey = getSectionFromHash();
const initialSectionExperience = buildSectionExperience(initialSectionKey);

function App() {
  const [screen, setScreen] = useState('home');
  const [activeSection, setActiveSection] = useState(initialSectionKey);
  const [isLoggedIn, setIsLoggedIn] = useState(!!getToken());
  const [activeAuthTab, setActiveAuthTab] = useState('login');
  const [loginForm, setLoginForm] = useState(loginFormDefaults);
  const [signupForm, setSignupForm] = useState(signupFormDefaults);
  const [adminForm, setAdminForm] = useState(adminLoginDefaults);
  const [showAuthPrompt, setShowAuthPrompt] = useState(false);
  const [adModal, setAdModal] = useState(null);
  const [clickCount, setClickCount] = useState(0);
  const [nextAdTrigger, setNextAdTrigger] = useState(() => (Math.random() < 0.5 ? 5 : 6));
  const [message, setMessage] = useState('');

  // API data state
  const [sectionView, setSectionView] = useState(initialSectionExperience);
  const [loading, setLoading] = useState(false);
  const [selectedNews, setSelectedNews] = useState(null);
  const [adminTab, setAdminTab] = useState('overview'); // Admin navigation state
  const [reviewModalItem, setReviewModalItem] = useState(null); // For review modals
  const [showBreakingNews, setShowBreakingNews] = useState(true); // Toggle breaking news visibility
  
  // News creation form state
  const [newsFormData, setNewsFormData] = useState({
    title: '',
    excerpt: '',
    content: '',
    category: 'Politics',
    location: '',
    imageUrl: ''
  });
  const [newsCreating, setNewsCreating] = useState(false);
  const [showNewsForm, setShowNewsForm] = useState(false);

  // Fetch section-specific content
  useEffect(() => {
    if (!window.location.hash) {
      window.location.hash = `#/${initialSectionKey}`;
    }

    const syncSectionFromHash = () => {
      setActiveSection(getSectionFromHash());
    };

    const handleNavigateSection = (event) => {
      const nextSection = normalizeSectionKey(event.detail?.section);
      setActiveSection(nextSection);
      setSelectedNews(null);
      window.location.hash = `#/${nextSection}`;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    window.addEventListener('hashchange', syncSectionFromHash);
    window.addEventListener('navigate-section', handleNavigateSection);

    return () => {
      window.removeEventListener('hashchange', syncSectionFromHash);
      window.removeEventListener('navigate-section', handleNavigateSection);
    };
  }, []);

  useEffect(() => {
    let isActive = true;
    const fallbackSection = buildSectionExperience(activeSection);
    setSectionView(fallbackSection);
    setSelectedNews(null);

    const fetchSectionStories = async () => {
      try {
        setLoading(true);
        const stories = await newsService.getByCategory(fallbackSection.label);

        if (!isActive) {
          return;
        }

        if (Array.isArray(stories) && stories.length > 0) {
          setSectionView(buildSectionExperience(activeSection, stories));
        }
      } catch (error) {
        console.error('[DEBUG] Failed to fetch section feed:', error);
      } finally {
        if (isActive) {
          setLoading(false);
        }
      }
    };

    fetchSectionStories();

    return () => {
      isActive = false;
    };
  }, [activeSection]);

  const demoRole = 'user';
  const tickerItems = sectionView.tickerItems;
  const liveBreakingText = sectionView.tickerItems.join('   |   ');

  const breakingText = useMemo(() => tickerItems.join('   •   '), []);
  const adminVisibleAd = useMemo(() => {
    if (adModal) {
      return adModal;
    }

    return null;
  }, [adModal]);

  const updateForm = (setter) => (event) => {
    const { name, value } = event.target;
    setter((previous) => ({ ...previous, [name]: value }));
  };

  const handleSectionSelect = (sectionKey) => {
    const nextSection = normalizeSectionKey(sectionKey);
    setActiveSection(nextSection);
    setSelectedNews(null);
    window.location.hash = `#/${nextSection}`;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const openRandomAd = () => {
    const ad = adLibrary[Math.floor(Math.random() * adLibrary.length)];
    setAdModal(ad);
  };

  const handleInteractiveClick = (event) => {
    if (screen !== 'home') {
      return;
    }

    if (
      event.target.closest('.site-nav') ||
      event.target.closest('.header-actions') ||
      event.target.closest('.ticker-close-button') ||
      event.target.closest('.chatbot-window') ||
      event.target.closest('.chatbot-toggle')
    ) {
      return;
    }

    if (!isLoggedIn) {
      setShowAuthPrompt(true);
      return;
    }

    if (adModal) {
      return;
    }

    setClickCount((previousCount) => {
      const nextCount = previousCount + 1;

      if (nextCount >= nextAdTrigger) {
        openRandomAd();
        setNextAdTrigger(Math.random() < 0.5 ? 5 : 6);
        return 0;
      }

      return nextCount;
    });
  };

  const handleLogin = async (event) => {
    event.preventDefault();
    try {
      setMessage('Logging in...');
      await authService.login(loginForm.email, loginForm.password);
      setMessage('Login successful.');
      setIsLoggedIn(true);
      setScreen('home');
      setShowAuthPrompt(false);
      setLoginForm(loginFormDefaults);
    } catch (error) {
      setMessage(error.data?.message || error.message || 'Login failed. Please try again.');
    }
  };

  const handleAdminLogin = async (event) => {
    event.preventDefault();
    try {
      setMessage('Logging in to admin console...');
      await authService.adminLogin(adminForm.username, adminForm.password);
      setMessage('Admin access granted.');
      setIsLoggedIn(true);
      setScreen('admin');
      setShowAuthPrompt(false);
      setAdminForm(adminLoginDefaults);
    } catch (error) {
      setMessage(error.data?.message || error.message || 'Invalid admin credentials.');
    }
  };

  const handleSignup = async (event) => {
    event.preventDefault();
    try {
      setMessage('Creating account...');
      await authService.signup({
        firstName: signupForm.name.split(' ')[0],
        lastName: signupForm.name.split(' ')[1] || '',
        email: signupForm.email,
        password: signupForm.password,
        role: 'user',
      });
      setMessage('Account created successfully! You can now login.');
      setActiveAuthTab('login');
    } catch (error) {
      setMessage(error.data?.message || error.message || 'Sign up failed. Please try again.');
    }
  };

  const handleLogout = () => {
    authService.logout();
    setIsLoggedIn(false);
    setScreen('auth');
    setActiveAuthTab('login');
    setMessage('You have been logged out.');
    setLoginForm(loginFormDefaults);
    setAdminForm(adminLoginDefaults);
    setShowAuthPrompt(false);
    setAdModal(null);
    setClickCount(0);
    setNextAdTrigger(Math.random() < 0.5 ? 5 : 6);
  };

  const closeAdModal = () => {
    setAdModal(null);
  };

  const openNewsDetail = (article) => {
    setSelectedNews(article);
  };

  const closeNewsDetail = () => {
    setSelectedNews(null);
  };

  const closeBreakingNews = () => {
    setShowBreakingNews(false);
  };

  // Admin button handlers
  const handleAdminNavigation = (tab) => {
    setAdminTab(tab);
    setMessage(`Navigating to ${tab.charAt(0).toUpperCase() + tab.slice(1)}...`);
    setTimeout(() => setMessage(''), 3000);
  };

  const handleReviewClick = (item) => {
    setReviewModalItem(item);
    setMessage(`Opening review: ${item.title}`);
  };

  const closeReviewModal = () => {
    setReviewModalItem(null);
    setMessage('Review closed.');
  };

  const handleApproveReview = (item) => {
    setMessage(`✓ Approved: ${item.title}`);
    setTimeout(() => setMessage(''), 3000);
    closeReviewModal();
  };

  const handleRejectReview = (item) => {
    setMessage(`✗ Rejected: ${item.title}`);
    setTimeout(() => setMessage(''), 3000);
    closeReviewModal();
  };

  // News form handlers
  const handleNewsFormChange = (e) => {
    const { name, value } = e.target;
    setNewsFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleImageUpload = (e) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file size (max 5MB original)
      const maxSize = 5 * 1024 * 1024; // 5MB
      if (file.size > maxSize) {
        setMessage('Image file is too large. Please select an image smaller than 5MB.');
        e.target.value = ''; // Clear the input
        return;
      }
      
      // Read and compress the image
      const reader = new FileReader();
      reader.onload = (event) => {
        // Compress image by reducing quality
        const img = new Image();
        img.onload = () => {
          const canvas = document.createElement('canvas');
          const ctx = canvas.getContext('2d');
          
          // Calculate new dimensions (max 1200x800)
          let width = img.width;
          let height = img.height;
          const maxWidth = 1200;
          const maxHeight = 800;
          
          if (width > height) {
            if (width > maxWidth) {
              height *= maxWidth / width;
              width = maxWidth;
            }
          } else {
            if (height > maxHeight) {
              width *= maxHeight / height;
              height = maxHeight;
            }
          }
          
          canvas.width = width;
          canvas.height = height;
          ctx.drawImage(img, 0, 0, width, height);
          
          // Convert to base64 with compression (0.7 quality)
          const compressedBase64 = canvas.toDataURL('image/jpeg', 0.7);
          setNewsFormData(prev => ({ ...prev, imageUrl: compressedBase64 }));
          setMessage(`✓ Image compressed and ready (${(compressedBase64.length / 1024 / 1024).toFixed(2)}MB)`);
          setTimeout(() => setMessage(''), 2000);
        };
        img.onerror = () => {
          setMessage('Error processing image. Please try a different image.');
          e.target.value = '';
        };
        img.src = event.target?.result;
      };
      reader.onerror = () => {
        setMessage('Error reading file. Please try again.');
      };
      reader.readAsDataURL(file);
    }
  };

  const handleCreateNews = async (e) => {
    e.preventDefault();
    
    // Validate required fields
    if (!newsFormData.title.trim()) {
      setMessage('❌ Article title is required.');
      return;
    }
    if (!newsFormData.content.trim()) {
      setMessage('❌ Article content is required.');
      return;
    }
    if (!newsFormData.location.trim()) {
      setMessage('❌ Location/place is required.');
      return;
    }
    if (!newsFormData.imageUrl) {
      setMessage('❌ Please upload an image for the article.');
      return;
    }

    try {
      setNewsCreating(true);
      setMessage('Creating news article...');
      
      const newsPayload = {
        title: newsFormData.title.trim(),
        excerpt: newsFormData.excerpt.trim() || newsFormData.content.substring(0, 100),
        content: newsFormData.content.trim(),
        category: newsFormData.category,
        location: newsFormData.location.trim(),
        imageUrl: newsFormData.imageUrl
      };
      
      console.log('Sending news payload, image size:', newsPayload.imageUrl.length, 'bytes');
      
      const response = await newsService.createNews(newsPayload);

      if (response.success) {
        setMessage('✓ News article created successfully! Refreshing feed...');
        setTimeout(() => setMessage(''), 3000);
        
        // Reset form
        setNewsFormData({
          title: '',
          excerpt: '',
          content: '',
          category: 'Politics',
          location: '',
          imageUrl: ''
        });
        setShowNewsForm(false);
        
        const createdSectionKey = normalizeSectionKey(newsFormData.category);
        if (createdSectionKey === activeSection) {
          try {
            const stories = await newsService.getByCategory(sectionView.label);
            if (Array.isArray(stories) && stories.length > 0) {
              setSectionView(buildSectionExperience(activeSection, stories));
            }
          } catch (refreshError) {
            console.error('Failed to refresh section after create:', refreshError);
          }
        }
      } else {
        setMessage(`❌ Failed to create article: ${response.message || 'Unknown error'}`);
      }
    } catch (error) {
      console.error('Create news error:', error);
      const errorMsg = error.data?.message || error.message || 'Failed to create article';
      setMessage(`❌ Error: ${errorMsg}`);
    } finally {
      setNewsCreating(false);
    }
  };

  const renderAuthPanel = () => (
    <>
      <div className="auth-panel-head">
        <h2>Welcome Back</h2>
        <p>Sign in to manage your editorial subscriptions and saved articles.</p>
        <span className="auth-role-badge">Role: {demoRole}</span>
      </div>

      <div className="auth-tabs" role="tablist" aria-label="Authentication tabs">
        <button
          type="button"
          className={activeAuthTab === 'login' ? 'active' : ''}
          onClick={() => setActiveAuthTab('login')}
        >
          Login
        </button>
        <button
          type="button"
          className={activeAuthTab === 'admin' ? 'active' : ''}
          onClick={() => setActiveAuthTab('admin')}
        >
          Admin
        </button>
        <button
          type="button"
          className={activeAuthTab === 'signup' ? 'active' : ''}
          onClick={() => setActiveAuthTab('signup')}
        >
          Create Account
        </button>
      </div>

      {activeAuthTab === 'login' ? (
        <form className="auth-form" onSubmit={handleLogin}>
          <label htmlFor="login-email">Email</label>
          <input
            id="login-email"
            name="email"
            type="email"
            placeholder="your@email.com"
            value={loginForm.email}
            onChange={updateForm(setLoginForm)}
            required
          />

          <label htmlFor="login-password">Password</label>
          <div className="input-row">
            <input
              id="login-password"
              name="password"
              type="password"
              placeholder="••••••••"
              value={loginForm.password}
              onChange={updateForm(setLoginForm)}
              required
            />
            <span className="field-link">Forgot Password?</span>
          </div>

          <button type="submit" className="auth-button">
            Login <span aria-hidden="true">→</span>
          </button>
        </form>
      ) : activeAuthTab === 'admin' ? (
        <form className="auth-form" onSubmit={handleAdminLogin}>
          <label htmlFor="admin-username">Username</label>
          <input
            id="admin-username"
            name="username"
            type="text"
            placeholder="admin"
            value={adminForm.username}
            onChange={updateForm(setAdminForm)}
            required
          />

          <label htmlFor="admin-password">Password</label>
          <input
            id="admin-password"
            name="password"
            type="password"
            placeholder="admin"
            value={adminForm.password}
            onChange={updateForm(setAdminForm)}
            required
          />

          <button type="submit" className="auth-button auth-button-admin">
            Open Admin Console <span aria-hidden="true">→</span>
          </button>
        </form>
      ) : (
        <form className="auth-form" onSubmit={handleSignup}>
          <label htmlFor="signup-name">Full Name</label>
          <input
            id="signup-name"
            name="name"
            type="text"
            placeholder="Arbish Banta Wara"
            value={signupForm.name}
            onChange={updateForm(setSignupForm)}
          />

          <label htmlFor="signup-email">Email</label>
          <input
            id="signup-email"
            name="email"
            type="email"
            placeholder="your@email.com"
            value={signupForm.email}
            onChange={updateForm(setSignupForm)}
          />

          <label htmlFor="signup-password">Password</label>
          <input
            id="signup-password"
            name="password"
            type="password"
            placeholder="Create a password"
            value={signupForm.password}
            onChange={updateForm(setSignupForm)}
          />

          <button type="submit" className="auth-button">
            Create an account <span aria-hidden="true">→</span>
          </button>
        </form>
      )}

      {message ? <p className="auth-message">{message}</p> : null}
    </>
  );

  if (screen === 'auth') {
    return (
      <div className="auth-page">
        <section className="auth-hero">
          <div className="auth-brand">ESTABLISHED 1924</div>
          <h1>
            The truth is
            <span>asymmetric.</span>
          </h1>
          <p>
            Access our global network of investigative journalism, deep-dive analysis,
            and real-time reporting from the digital broadsheet.
          </p>
          <div className="auth-footer-note">© 2024 Editorial Authority</div>
        </section>

        <section className="auth-panel">
          {renderAuthPanel()}
        </section>
      </div>
    );
  }

  if (screen === 'admin') {
    return (
      <div className="portal-shell admin-shell" onClickCapture={handleInteractiveClick}>
        <header className="site-header admin-header">
          <div className="logo-block">
            <h1>News Portal</h1>
          </div>

          <nav className="site-nav" aria-label="Primary navigation">
            <button type="button" className={adminTab === 'overview' ? 'admin-nav-link active' : 'admin-nav-link'} onClick={() => handleAdminNavigation('overview')}>Overview</button>
            <button type="button" className={adminTab === 'reviews' ? 'admin-nav-link active' : 'admin-nav-link'} onClick={() => handleAdminNavigation('reviews')}>Reviews</button>
            <button type="button" className={adminTab === 'campaigns' ? 'admin-nav-link active' : 'admin-nav-link'} onClick={() => handleAdminNavigation('campaigns')}>Campaigns</button>
            <button type="button" className={adminTab === 'ads' ? 'admin-nav-link active' : 'admin-nav-link'} onClick={() => handleAdminNavigation('ads')}>Ads</button>
            <button type="button" className={adminTab === 'reports' ? 'admin-nav-link active' : 'admin-nav-link'} onClick={() => handleAdminNavigation('reports')}>Reports</button>
          </nav>

          <div className="header-actions">
            <NotificationBell />
            <span className="admin-badge">Admin Console</span>
            <button type="button" className="logout-button" onClick={handleLogout}>
              Logout
            </button>
          </div>
        </header>

        <div className="ticker-bar" aria-label="Breaking news ticker">
          <span className="breaking-label">ADMIN</span>
          <div className="ticker-track">
            <div className="ticker-text">Editorial dashboards, promotions, and moderation tools in one view.</div>
          </div>
        </div>

        <main className="home-content admin-content">
          <section className="admin-hero">
            <div>
              <span className="hero-tag">CONTROL ROOM</span>
              <h2>Manage the newsroom from a single polished console.</h2>
              <p>
                Review stories, monitor campaigns, and surface premium placements with the same visual rhythm as the public homepage.
              </p>
            </div>

            <div className="admin-status-panel">
              <span className="admin-status-label">Session status</span>
              <strong>Signed in as admin</strong>
              <p>Click anywhere in the console and a sponsored card appears after 5 or 6 interactions.</p>
            </div>
          </section>

          <section className="admin-highlights">
            {adminHighlights.map((item) => (
              <article key={item.label} className="admin-metric-card">
                <span>{item.label}</span>
                <strong>{item.value}</strong>
                <p>{item.detail}</p>
              </article>
            ))}
          </section>

          <section className="admin-grid">
            {adminTab === 'overview' && (
              <>
                <article className="admin-panel admin-panel-large">
                  <div className="section-title-row">
                    <h3>Editorial Queue</h3>
                    <span className="section-rule" />
                  </div>

                  <div className="admin-list">
                    {adminQueue.map((item) => (
                      <div key={item.id} className="admin-list-item">
                        <div>
                          <h4>{item.title}</h4>
                          <p>{item.meta}</p>
                        </div>
                        <button type="button" className="admin-action-button" onClick={() => handleReviewClick(item)}>
                          Review
                        </button>
                      </div>
                    ))}
                  </div>
                </article>

                <article className="admin-panel">
                  <div className="section-title-row">
                    <h3>Ad Rotation</h3>
                    <span className="section-rule" />
                  </div>

                  <div className="admin-copy-stack">
                    <p>
                      Random ad cards are triggered at a 5 or 6 click interval to mimic an editorial sponsorship flow.
                    </p>
                    <button type="button" className="auth-button auth-button-admin" onClick={openRandomAd}>
                      Preview Ad Placement <span aria-hidden="true">→</span>
                    </button>
                  </div>
                </article>
              </>
            )}

            {adminTab === 'reviews' && (
              <article className="admin-panel admin-panel-large">
                <div className="section-title-row">
                  <h3>Editorial Reviews</h3>
                  <span className="section-rule" />
                </div>
                <div className="admin-list">
                  {adminQueue.map((item) => (
                    <div key={item.id} className="admin-list-item">
                      <div>
                        <h4>{item.title}</h4>
                        <p>{item.meta}</p>
                      </div>
                      <button type="button" className="admin-action-button" onClick={() => handleReviewClick(item)}>
                        Review
                      </button>
                    </div>
                  ))}
                </div>
              </article>
            )}

            {adminTab === 'campaigns' && (
              <article className="admin-panel admin-panel-large">
                <div className="section-title-row">
                  <h3>Add News for Users</h3>
                  <span className="section-rule" />
                </div>
                
                <button 
                  type="button" 
                  className="auth-button auth-button-admin"
                  onClick={() => setShowNewsForm(!showNewsForm)}
                  style={{ marginBottom: '1.5rem' }}
                >
                  {showNewsForm ? '✕ Cancel' : '+ Add New Article'} <span aria-hidden="true">→</span>
                </button>

                {showNewsForm && (
                  <form onSubmit={handleCreateNews} className="news-creation-form">
                    <div className="form-group">
                      <label htmlFor="news-title">Article Title *</label>
                      <input
                        id="news-title"
                        type="text"
                        name="title"
                        placeholder="Enter article title"
                        value={newsFormData.title}
                        onChange={handleNewsFormChange}
                        required
                        disabled={newsCreating}
                      />
                    </div>

                    <div className="form-group">
                      <label htmlFor="news-excerpt">Excerpt (Optional)</label>
                      <textarea
                        id="news-excerpt"
                        name="excerpt"
                        placeholder="Brief summary of the article"
                        value={newsFormData.excerpt}
                        onChange={handleNewsFormChange}
                        rows="2"
                        disabled={newsCreating}
                      />
                    </div>

                    <div className="form-group">
                      <label htmlFor="news-content">Article Content *</label>
                      <textarea
                        id="news-content"
                        name="content"
                        placeholder="Full article content"
                        value={newsFormData.content}
                        onChange={handleNewsFormChange}
                        rows="6"
                        required
                        disabled={newsCreating}
                      />
                    </div>

                    <div className="form-row">
                      <div className="form-group">
                        <label htmlFor="news-category">Category</label>
                        <select
                          id="news-category"
                          name="category"
                          value={newsFormData.category}
                          onChange={handleNewsFormChange}
                          disabled={newsCreating}
                        >
                          <option>Politics</option>
                          <option>Tech</option>
                          <option>Science</option>
                          <option>Culture</option>
                          <option>Opinion</option>
                        </select>
                      </div>

                      <div className="form-group">
                        <label htmlFor="news-location">Location/Place *</label>
                        <input
                          id="news-location"
                          type="text"
                          name="location"
                          placeholder="e.g., New York, India, Tech Hub"
                          value={newsFormData.location}
                          onChange={handleNewsFormChange}
                          required
                          disabled={newsCreating}
                        />
                      </div>
                    </div>

                    <div className="form-group">
                      <label htmlFor="news-image">Upload Image * (Max 5MB, auto-compressed)</label>
                      <input
                        id="news-image"
                        type="file"
                        accept="image/*"
                        onChange={handleImageUpload}
                        disabled={newsCreating}
                        required
                      />
                      {newsFormData.imageUrl && (
                        <div style={{ marginTop: '0.75rem' }}>
                          <small style={{ color: '#10b981' }}>✓ Image ready for upload</small>
                        </div>
                      )}
                    </div>

                    <div style={{ display: 'flex', gap: '0.75rem', marginTop: '1.5rem' }}>
                      <button 
                        type="submit" 
                        className="auth-button auth-button-admin"
                        disabled={newsCreating}
                      >
                        {newsCreating ? 'Creating...' : 'Create Article'} <span aria-hidden="true">→</span>
                      </button>
                      <button 
                        type="button" 
                        className="admin-dismiss-button"
                        onClick={() => setShowNewsForm(false)}
                        disabled={newsCreating}
                      >
                        Cancel
                      </button>
                    </div>
                  </form>
                )}

                {!showNewsForm && (
                  <div className="admin-copy-stack">
                    <p>Add new articles that will appear in the user section immediately.</p>
                    <p style={{ fontSize: '0.9rem', color: '#666' }}>Fill out the form to create a new news article with an image and location details.</p>
                  </div>
                )}
              </article>
            )}

            {adminTab === 'ads' && (
              <article className="admin-panel admin-panel-large">
                <div className="section-title-row">
                  <h3>Ad Management</h3>
                  <span className="section-rule" />
                </div>
                <div className="admin-copy-stack">
                  <p>Manage all advertising placements and rotations.</p>
                  <button type="button" className="auth-button auth-button-admin" onClick={openRandomAd}>
                    Preview Ad Placement <span aria-hidden="true">→</span>
                  </button>
                  <div style={{ marginTop: '1.5rem' }}>
                    <p style={{ color: '#999', fontSize: '0.9rem' }}>Ad analytics and performance metrics will be displayed here.</p>
                  </div>
                </div>
              </article>
            )}

            {adminTab === 'reports' && (
              <article className="admin-panel admin-panel-large">
                <div className="section-title-row">
                  <h3>Analytics & Reports</h3>
                  <span className="section-rule" />
                </div>
                <div className="admin-copy-stack">
                  <p>Homepage CTR: 4.8%</p>
                  <p>Pending Reviews: 18</p>
                  <p>Active Campaigns: 7</p>
                  <div style={{ marginTop: '1.5rem' }}>
                    <p style={{ color: '#999', fontSize: '0.9rem' }}>Detailed analytics charts and reports will be displayed here.</p>
                  </div>
                </div>
              </article>
            )}
          </section>

          {message && (
            <div className="admin-message-toast">
              {message}
            </div>
          )}

          {reviewModalItem && (
            <div className="review-modal-backdrop" role="presentation" onClick={closeReviewModal}>
              <article className="review-modal" role="dialog" aria-modal="true" onClick={(e) => e.stopPropagation()}>
                <button type="button" className="review-modal-close" onClick={closeReviewModal} aria-label="Close review">×</button>
                <h2>{reviewModalItem.title}</h2>
                <p className="review-meta">{reviewModalItem.meta}</p>
                <div className="review-content">
                  <p>This item is pending your editorial review. You can approve or reject it below.</p>
                  <details className="review-details">
                    <summary>View Details</summary>
                    <pre>{JSON.stringify(reviewModalItem, null, 2)}</pre>
                  </details>
                </div>
                <div className="review-actions">
                  <button type="button" className="btn-approve" onClick={() => handleApproveReview(reviewModalItem)}>
                    ✓ Approve
                  </button>
                  <button type="button" className="btn-reject" onClick={() => handleRejectReview(reviewModalItem)}>
                    ✗ Reject
                  </button>
                </div>
              </article>
            </div>
          )}

          {adModal && (
            <div className="ad-modal-backdrop" role="presentation" onClick={closeAdModal}>
              <article className={`ad-modal tone-${adModal.tone}`} role="dialog" aria-modal="true" aria-label={adModal.title} onClick={(event) => event.stopPropagation()}>
                <span className="news-card-tag">{adModal.label}</span>
                <h3>{adModal.title}</h3>
                <p>{adModal.copy}</p>
                <div className="ad-modal-actions">
                  <button type="button" className="auth-button auth-button-admin" onClick={closeAdModal}>
                    {adModal.cta}
                  </button>
                  <button type="button" className="admin-dismiss-button" onClick={closeAdModal}>
                    Dismiss
                  </button>
                </div>
              </article>
            </div>
          )}
        </main>

      </div>
    );
  }

  return (
    <div className="portal-shell" onClickCapture={handleInteractiveClick}>
      <header className="site-header">
        <div className="logo-block">
          <h1>News Portal</h1>
        </div>

        <nav className="site-nav" aria-label="Primary navigation">
          {homeSections.map((section) => (
            <a
              key={section.key}
              href={`#/${section.key}`}
              className={activeSection === section.key ? 'active' : ''}
              aria-current={activeSection === section.key ? 'page' : undefined}
              onClick={(event) => {
                event.preventDefault();
                handleSectionSelect(section.key);
              }}
            >
              {section.label}
            </a>
          ))}
        </nav>

        <div className="header-actions">
          <div className="search-box">
            <span aria-hidden="true">⌕</span>
            <input type="text" placeholder="Search news..." />
          </div>
          {isLoggedIn ? (
            <>
              <span className="signed-in-badge">Signed in</span>
              <button type="button" className="logout-button" onClick={handleLogout}>
                Logout
              </button>
            </>
          ) : (
            <button type="button" className="login-button" onClick={() => setScreen('auth')}>
              Login
            </button>
          )}
        </div>
      </header>

      {showBreakingNews && (
        <div className="ticker-bar" aria-label="Breaking news ticker">
          <span className="breaking-label">BREAKING</span>
          <div className="ticker-track">
            <div className="ticker-text">{liveBreakingText}</div>
          </div>
          <button 
            type="button" 
            className="ticker-close-button" 
            onClick={closeBreakingNews}
            aria-label="Close breaking news"
          >
            ×
          </button>
        </div>
      )}

      <main className="home-content">
        <section className={`section-brief section-brief-${activeSection}`}>
          <div>
            <span className="section-brief-kicker">{sectionView.kicker}</span>
            <p>{sectionView.description}</p>
          </div>
          <span className="section-brief-status">
            {loading ? `Refreshing ${sectionView.label}...` : `${sectionView.label} desk live`}
          </span>
        </section>

        <section className="hero-layout">
          <article
            className={`hero-story ${sectionView.heroClass}`}
            onClick={() => openNewsDetail(sectionView.heroStory)}
            role="button"
            tabIndex={0}
            onKeyDown={(event) => {
              if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                openNewsDetail(sectionView.heroStory);
              }
            }}
          >
            <div className="hero-overlay" />
            <div className="hero-tag">{sectionView.heroStory.tag}</div>
            <h2>{sectionView.heroStory.title}</h2>
            <div className="hero-meta">
              <span>{sectionView.heroStory.author}</span>
              <span>{sectionView.heroStory.readTime || sectionView.heroStory.time}</span>
            </div>
          </article>

          <div className="side-stack">
            {sectionView.featuredSideStories.map((story) => (
              <article
                key={story.id}
                className="side-story-card"
                onClick={() => openNewsDetail(story)}
                role="button"
                tabIndex={0}
                onKeyDown={(event) => {
                  if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault();
                    openNewsDetail(story);
                  }
                }}
              >
                <div className="side-story-text">
                  <span className="card-category">{story.category}</span>
                  <h3>{story.title}</h3>
                  <p>{story.excerpt}</p>
                </div>
                <div className={`side-story-art tone-${story.tone}`} aria-hidden="true">
                  <div className="art-sheen" />
                </div>
              </article>
            ))}
          </div>
        </section>

        <section className="section-title-row">
          <h3>{`Trending in ${sectionView.label}`}</h3>
          <span className="section-rule" />
        </section>

        <section className="trending-grid">
          {sectionView.trendingStories.map((story) => (
            <NewsCard key={story.id} article={story} onClick={openNewsDetail} />
          ))}
        </section>
      </main>

      {!isLoggedIn && showAuthPrompt ? (
        <div className="auth-modal-backdrop" role="presentation" onClick={() => setShowAuthPrompt(false)}>
          <section className="auth-modal-panel" role="dialog" aria-modal="true" aria-labelledby="auth-modal-title" onClick={(event) => event.stopPropagation()}>
            <button type="button" className="auth-modal-close" onClick={() => setShowAuthPrompt(false)} aria-label="Close login prompt">
              ×
            </button>
            <div className="auth-modal-copy">
              <span className="auth-modal-kicker">Members only</span>
              <h2 id="auth-modal-title">Sign in to use the newsroom</h2>
              <p>Clicking the page opens this prompt. Log in or create an account to continue browsing the portal.</p>
            </div>
            <section className="auth-panel auth-panel-compact">
              {renderAuthPanel()}
            </section>
          </section>
        </div>
      ) : null}

      {isLoggedIn && adModal ? (
        <div className="ad-modal-backdrop" role="presentation" onClick={closeAdModal}>
          <article className={`ad-modal tone-${adModal.tone}`} role="dialog" aria-modal="true" aria-label={adModal.title} onClick={(event) => event.stopPropagation()}>
            <span className="news-card-tag">{adModal.label}</span>
            <h3>{adModal.title}</h3>
            <p>{adModal.copy}</p>
            <div className="ad-modal-actions">
              <button type="button" className="auth-button auth-button-admin" onClick={closeAdModal}>
                {adModal.cta}
              </button>
              <button type="button" className="admin-dismiss-button" onClick={closeAdModal}>
                Dismiss
              </button>
            </div>
          </article>
        </div>
      ) : null}

      {selectedNews && <NewsDetailModal article={selectedNews} onClose={closeNewsDetail} />}
      <Chatbot />
    </div>
  );
}

export default App;
