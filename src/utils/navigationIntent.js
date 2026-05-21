/**
 * Navigation Intent Detection System
 * Detects user intents and converts them to navigation commands
 * Uses both keyword matching and DeepSeek API for advanced NLP
 */

const DEEPSEEK_API_KEY = 'sk-963ead964342412882aa4d95fc0b9c4b';
const DEEPSEEK_API_URL = 'https://api.deepseek.com/chat/completions';

// ─── INTENT MAPPING ──────────────────────────────────────────────────
// Maps detected intents to news categories
const INTENT_MAP = {
  politics: {
    section: 'politics',
    keywords: ['politics', 'government', 'election', 'parliament', 'congress', 'bill', 'law', 'presidential', 'minister', 'policy'],
    phrases: ['go to politics', 'politics section', 'show politics', 'news about government', 'political news', 'government news'],
    response: "🏛️ Taking you to Politics section... You'll see government, elections, and policy news."
  },
  tech: {
    section: 'tech',
    keywords: ['technology', 'tech', 'ai', 'software', 'startup', 'gadget', 'app', 'digital', 'computer', 'programming', 'data'],
    phrases: ['go to tech', 'tech section', 'show tech news', 'technology news', 'ai news', 'startup news'],
    response: "⚙️ Taking you to Tech section... You'll see AI, startups, and technology breakthroughs."
  },
  science: {
    section: 'science',
    keywords: ['science', 'research', 'discovery', 'scientist', 'experiment', 'biology', 'physics', 'space', 'astronomy', 'medical'],
    phrases: ['go to science', 'science section', 'show science news', 'science discoveries', 'research news', 'space news'],
    response: "🔬 Taking you to Science section... You'll see research, discoveries, and scientific breakthroughs."
  },
  sports: {
    section: 'sports',
    keywords: ['sports', 'game', 'football', 'soccer', 'basketball', 'cricket', 'tennis', 'player', 'team', 'match', 'championship'],
    phrases: ['go to sports', 'sports section', 'show sports', 'sports news', 'game news', 'championship news'],
    response: "⚽ Taking you to Sports section... You'll see games, matches, and sports updates."
  },
  culture: {
    section: 'culture',
    keywords: ['culture', 'art', 'music', 'movie', 'film', 'entertainment', 'celebrity', 'show', 'theater', 'fashion', 'book'],
    phrases: ['go to culture', 'culture section', 'entertainment news', 'movie news', 'music news', 'celebrity news'],
    response: "🎬 Taking you to Culture section... You'll see entertainment, movies, and cultural news."
  },
  opinion: {
    section: 'opinion',
    keywords: ['opinion', 'editorial', 'analysis', 'commentary', 'column', 'view', 'perspective', 'argument'],
    phrases: ['go to opinion', 'opinion section', 'editorial', 'analysis', 'commentary news'],
    response: "💭 Taking you to Opinion section... You'll see analysis, editorials, and expert commentary."
  },
  admin: {
    section: 'admin',
    keywords: ['admin', 'administrator', 'console', 'manage', 'dashboard', 'admin panel', 'control room', 'editorial'],
    phrases: ['go to admin', 'admin section', 'admin console', 'admin panel', 'take me to admin', 'open admin', 'show admin'],
    response: "🔐 Taking you to Admin Login... You'll need admin credentials to access the control room.",
    isSpecial: true
  },
  search: {
    section: 'search',
    keywords: ['search', 'find', 'look for', 'query'],
    phrases: ['search for', 'find', 'look up', 'search'],
    response: "🔍 I can help you search! What would you like to find?",
    isSpecial: true
  }
};

// ─── LOCAL KEYWORD MATCHING (Fallback) ──────────────────────────────
/**
 * Detects intent using keyword matching
 * Fast fallback when API is unavailable
 */
const detectIntentByKeywords = (userInput) => {
  const lowerInput = userInput.toLowerCase().trim();
  
  // Check for exact section name (highest priority - just saying the section)
  for (const [intent, config] of Object.entries(INTENT_MAP)) {
    if (lowerInput === intent || lowerInput === config.section) {
      return {
        intent,
        confidence: 0.99, // Very high confidence
        method: 'exact_section_match'
      };
    }
  }
  
  // Check for exact phrase matches first (highest priority)
  for (const [intent, config] of Object.entries(INTENT_MAP)) {
    for (const phrase of config.phrases) {
      if (lowerInput.includes(phrase)) {
        return {
          intent,
          confidence: 0.95,
          method: 'phrase_match'
        };
      }
    }
  }
  
  // Check for keyword matches (lower priority)
  for (const [intent, config] of Object.entries(INTENT_MAP)) {
    for (const keyword of config.keywords) {
      if (lowerInput.includes(keyword)) {
        return {
          intent,
          confidence: 0.8, // Increased from 0.7
          method: 'keyword_match'
        };
      }
    }
  }
  
  return {
    intent: null,
    confidence: 0,
    method: 'no_match'
  };
};

// ─── DEEPSEEK API INTENT DETECTION ──────────────────────────────────
/**
 * Detects intent using DeepSeek API for advanced NLP
 * More accurate for complex user inputs
 */
const detectIntentByDeepSeek = async (userInput) => {
  try {
    const prompt = `You are a news portal navigation assistant. Analyze the user's input and determine their intent.

User input: "${userInput}"

Available sections: politics, tech, science, sports, culture, opinion

Respond ONLY with a JSON object in this format:
{
  "intent": "section_name_or_null",
  "confidence": 0.0_to_1.0,
  "reasoning": "brief explanation"
}

If the user wants to navigate to a section, respond with the section name. If unclear, respond with null.`;

    const response = await fetch(DEEPSEEK_API_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${DEEPSEEK_API_KEY}`
      },
      body: JSON.stringify({
        model: 'deepseek-chat',
        messages: [
          {
            role: 'user',
            content: prompt
          }
        ],
        temperature: 0.3,
        max_tokens: 200
      })
    });

    if (!response.ok) {
      console.warn(`DeepSeek API error: ${response.status}`);
      return null;
    }

    const data = await response.json();
    const content = data.choices[0].message.content.trim();
    
    // Parse JSON response
    const jsonMatch = content.match(/\{[\s\S]*\}/);
    if (!jsonMatch) {
      console.warn('Could not parse DeepSeek response');
      return null;
    }

    const result = JSON.parse(jsonMatch[0]);
    
    // Validate intent exists in INTENT_MAP
    if (result.intent && !INTENT_MAP[result.intent]) {
      result.intent = null;
    }
    
    return {
      intent: result.intent,
      confidence: result.confidence || 0,
      method: 'deepseek_api',
      reasoning: result.reasoning
    };
  } catch (error) {
    console.warn('DeepSeek API failed, falling back to keyword matching:', error);
    return null;
  }
};

// ─── MAIN INTENT DETECTION FUNCTION ────────────────────────────────
/**
 * Main function to detect navigation intent
 * Uses DeepSeek API first, falls back to keyword matching
 */
export const detectNavigationIntent = async (userInput) => {
  // Try DeepSeek API first (more accurate)
  const deepSeekResult = await detectIntentByDeepSeek(userInput);
  
  if (deepSeekResult && deepSeekResult.intent) {
    return deepSeekResult;
  }
  
  // Fallback to keyword matching (faster, always works)
  const keywordResult = detectIntentByKeywords(userInput);
  
  if (keywordResult.intent && keywordResult.confidence >= 0.7) {
    return keywordResult;
  }
  
  return {
    intent: null,
    confidence: 0,
    method: 'no_match'
  };
};

// ─── NAVIGATION HANDLER ────────────────────────────────────────────
/**
 * Executes navigation to the detected section
 */
export const navigateToSection = (section) => {
  if (!section || !INTENT_MAP[section]) {
    console.warn(`Invalid section: ${section}`);
    return false;
  }
  
  // Special handling for admin
  if (section === 'admin') {
    window.location.hash = '#/auth?tab=admin';
    const navigationEvent = new CustomEvent('navigate-to-admin', {
      detail: { section: 'admin' }
    });
    window.dispatchEvent(navigationEvent);
    return true;
  }
  
  // Update URL hash for SPA navigation
  window.location.hash = `#/${section}`;
  
  // Alternative: Dispatch custom event for React routing
  const navigationEvent = new CustomEvent('navigate-section', {
    detail: { section }
  });
  window.dispatchEvent(navigationEvent);
  
  return true;
};

// ─── SEARCH HANDLER ────────────────────────────────────────────────
/**
 * Triggers search functionality
 */
export const triggerSearch = (query) => {
  const searchEvent = new CustomEvent('trigger-search', {
    detail: { query, timestamp: Date.now() }
  });
  window.dispatchEvent(searchEvent);
  return true;
};

// ─── RESPONSE GENERATION ──────────────────────────────────────────
/**
 * Generates natural bot response based on detected intent
 */
export const generateNavigationResponse = (intent) => {
  if (!intent || !INTENT_MAP[intent]) {
    return "I'm not sure which section you want. Try saying:\n• 'Take me to politics'\n• 'Show tech news'\n• 'Sports section'\n• 'Culture news'\n• 'Science discoveries'\n• 'Opinion articles'";
  }
  
  return INTENT_MAP[intent].response;
};

// ─── FOLLOW-UP QUESTION ────────────────────────────────────────────
/**
 * Generates follow-up question when intent is unclear
 */
export const generateFollowUpQuestion = () => {
  return "Which section would you like to explore?\n\n📰 Available sections:\n• Politics\n• Technology\n• Science\n• Sports\n• Culture\n• Opinion\n\nJust say the name or describe what interests you!";
};

// ─── HELPER: Get all available sections ────────────────────────────
export const getAvailableSections = () => {
  return Object.keys(INTENT_MAP);
};

// ─── HELPER: Get section details ──────────────────────────────────
export const getSectionDetails = (section) => {
  return INTENT_MAP[section] || null;
};

// ─── GENERAL CONVERSATION RESPONSES ────────────────────────────
/**
 * Responds to general conversation (non-navigation queries)
 * Makes the bot feel more like a friendly assistant
 */
export const generateChatResponse = (userInput) => {
  const lowerInput = userInput.toLowerCase();
  
  // Greetings
  if (lowerInput.match(/^(hi|hello|hey|greetings)[\s!?]*$/)) {
    const greetings = [
      "Hey there! 👋 Want me to take you somewhere, or just wanna chat?",
      "Hey! How's it going? Need help finding a news section or want to talk?",
      "Hello! 😊 I can help you navigate to any section or just chat with you!"
    ];
    return greetings[Math.floor(Math.random() * greetings.length)];
  }
  
  // How are you
  if (lowerInput.match(/how are you|how are u|how's it|how you doing/)) {
    const responses = [
      "I'm doing great, thanks for asking! 😊 Always ready to help you explore news or chat!",
      "Feeling awesome! 🚀 Ready to navigate you to any section or just have a good conversation!",
      "All systems go! 💪 What can I do for you today?"
    ];
    return responses[Math.floor(Math.random() * responses.length)];
  }
  
  // Thanks
  if (lowerInput.match(/thank|thanks|appreciate/)) {
    const responses = [
      "You're welcome! 😄 Anything else I can help with?",
      "Happy to help! 🎉 Need anything else?",
      "Anytime, friend! What's next? 😊"
    ];
    return responses[Math.floor(Math.random() * responses.length)];
  }
  
  // Help/Features
  if (lowerInput.match(/what can you do|help me|features|capabilities/)) {
    return "I can do two things:\n\n🧭 **Navigation:** Take you to any section (Politics, Tech, Science, Sports, Culture, Opinion)\n\n💬 **Chat:** Just talk to me like a friend!\n\nTry saying 'take me to politics' or just chat with me about anything!";
  }
  
  // About the portal
  if (lowerInput.match(/about|what is|what's this|portal|website/)) {
    return "This is your News Portal! 📰\n\nWe have 6 main sections:\n• 🏛️ Politics - Government & Elections\n• ⚙️ Tech - AI, Startups & Innovation\n• 🔬 Science - Research & Discoveries\n• ⚽ Sports - Games & Championships\n• 🎬 Culture - Entertainment & Arts\n• 💭 Opinion - Analysis & Commentary\n\nWant me to take you to any section?";
  }
  
  // Compliments
  if (lowerInput.match(/you're (cool|awesome|great|smart|nice|good)/i)) {
    const responses = [
      "Aw, thanks! 😊 You're amazing too!",
      "That's sweet of you! 🥰 Now let's get you some great news!",
      "Thanks buddy! I'm just here to help! 💪"
    ];
    return responses[Math.floor(Math.random() * responses.length)];
  }
  
  // Jokes/Fun
  if (lowerInput.match(/joke|funny|make me laugh|haha|lol/)) {
    const jokes = [
      "Why did the newspaper go to the gym? 💪 To get some breaking news!",
      "What do you call a fake article? 📰 Mis-information!",
      "Why did the editor break up with the reporter? 📻 They had no chemistry... or were they just fake news? 😄"
    ];
    return jokes[Math.floor(Math.random() * jokes.length)];
  }
  
  // Default friendly response
  const defaultResponses = [
    "That's interesting! 😊 Want to explore a news section or keep chatting?",
    "Cool! 👌 Need me to take you somewhere or just hanging out?",
    "I hear you! 💬 So, want news from a specific section or more chat?",
    "Sounds good! 😄 What would you like to do - navigate to a section or chat more?",
    "Nice! 🤙 Anything else I can help with?"
  ];
  return defaultResponses[Math.floor(Math.random() * defaultResponses.length)];
};