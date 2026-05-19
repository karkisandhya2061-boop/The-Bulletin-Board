import { useMemo, useState, useEffect } from 'react';
import NewsCard from './components/NewsCard';
import { authService, newsService, adsService, getToken } from './utils/apiService';

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
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  role: '',
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

function App() {
  const [screen, setScreen] = useState('home');
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
  const [tickerItems, setTickerItems] = useState(defaultTickerItems);
  const [heroStory, setHeroStory] = useState(defaultHeroStory);
  const [featuredSideStories, setFeaturedSideStories] = useState(defaultFeaturedSideStories);
  const [trendingStories, setTrendingStories] = useState(defaultTrendingStories);
  const [loading, setLoading] = useState(true);

  // Fetch news feed on component mount
  useEffect(() => {
    const fetchFeed = async () => {
      try {
        setLoading(true);
        const feedData = await newsService.getFeed();
        if (feedData) {
          if (feedData.heroStory) setHeroStory(feedData.heroStory);
          if (feedData.featuredSideStories) setFeaturedSideStories(feedData.featuredSideStories);
          if (feedData.trendingStories) setTrendingStories(feedData.trendingStories);
          if (feedData.tickerItems) setTickerItems(feedData.tickerItems);
        }
      } catch (error) {
        console.warn('Failed to fetch feed, using defaults:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchFeed();
  }, []);

  const demoRole = 'user';

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

  const openRandomAd = () => {
    const ad = adLibrary[Math.floor(Math.random() * adLibrary.length)];
    setAdModal(ad);
  };

  const handleInteractiveClick = () => {
    if (screen !== 'home') {
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
          <label htmlFor="login-first-name">First Name</label>
          <input
            id="login-first-name"
            name="firstName"
            type="text"
            placeholder="sandhya"
            value={loginForm.firstName}
            onChange={updateForm(setLoginForm)}
            required
          />

          <label htmlFor="login-last-name">Last Name</label>
          <input
            id="login-last-name"
            name="lastName"
            type="text"
            placeholder="tiwari"
            value={loginForm.lastName}
            onChange={updateForm(setLoginForm)}
            required
          />

          <label htmlFor="login-email">Email</label>
          <input
            id="login-email"
            name="email"
            type="email"
            placeholder="sandhya@gmail.com"
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

          <label htmlFor="login-role">Role</label>
          <select
            id="login-role"
            name="role"
            value={loginForm.role}
            onChange={updateForm(setLoginForm)}
            required
          >
            <option value="">Select role</option>
            <option value="user">user</option>
            <option value="admin">admin</option>
          </select>

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
            <a href="#">Overview</a>
            <a href="#">Reviews</a>
            <a href="#">Campaigns</a>
            <a href="#">Ads</a>
            <a href="#">Reports</a>
          </nav>

          <div className="header-actions">
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
                    <button type="button" className="admin-action-button">
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
          </section>
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
          <a href="#">Politics</a>
          <a href="#">Tech</a>
          <a href="#">Science</a>
          <a href="#">Culture</a>
          <a href="#">Opinion</a>
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

      <div className="ticker-bar" aria-label="Breaking news ticker">
        <span className="breaking-label">BREAKING</span>
        <div className="ticker-track">
          <div className="ticker-text">{breakingText}</div>
        </div>
      </div>

      <main className="home-content">
        <section className="hero-layout">
          <article className="hero-story">
            <div className="hero-overlay" />
            <div className="hero-tag">WORLD NEWS</div>
            <h2>{heroStory.title}</h2>
            <div className="hero-meta">
              <span>{heroStory.author}</span>
              <span>{heroStory.readTime}</span>
            </div>
          </article>

          <div className="side-stack">
            {featuredSideStories.map((story) => (
              <article key={story.id} className="side-story-card">
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
          <h3>Trending Now</h3>
          <span className="section-rule" />
        </section>

        <section className="trending-grid">
          {trendingStories.map((story) => (
            <NewsCard key={story.id} article={story} />
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
    </div>
  );
}

export default App;
