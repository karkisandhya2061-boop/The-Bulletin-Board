const tones = ['blue', 'amber', 'teal', 'rose'];

const createStory = (story) => ({
  id: story.id,
  category: story.category,
  title: story.title,
  excerpt: story.excerpt,
  content: story.content || story.excerpt,
  author: story.author || 'Editorial Team',
  time: story.time || '5 min read',
  readTime: story.readTime || story.time || '5 min read',
  tone: story.tone || 'blue',
  tag: story.tag || 'New',
  imageUrl: story.imageUrl || '',
  location: story.location || story.category,
  publishedAt: story.publishedAt || '',
});

export const sectionContent = {
  politics: {
    label: 'Politics',
    heroClass: 'hero-politics',
    kicker: 'Capital Watch',
    description:
      'Election strategy, coalition talks, and public policy shifts from the desks shaping the week.',
    tickerItems: [
      'Coalition negotiators trade transport funding for housing reform.',
      'Election officials tighten disclosure rules for digital ad buys.',
      'Provincial leaders demand faster flood relief after budget review.',
    ],
    heroStory: createStory({
      id: 'politics-hero',
      category: 'Politics',
      tag: 'Politics Desk',
      title: 'Coalition talks redraw the policy map before the summer session',
      excerpt:
        'Party leaders are swapping cabinet leverage for housing, climate, and procurement concessions as the next vote nears.',
      content:
        'Senior negotiators are rebuilding their agenda around food prices, public transport, and transparency promises. Advisers say the final package will be judged less on rhetoric and more on whether it lands quickly in city budgets.',
      author: 'By Mira Adhikari',
      time: '8 min read',
      readTime: '8 min read',
      tone: 'rose',
      location: 'Kathmandu',
    }),
    featuredSideStories: [
      createStory({
        id: 'politics-side-1',
        category: 'Politics',
        title: 'City mayors unite behind a shared transit and zoning pact',
        excerpt:
          'A regional bloc is testing whether joint procurement can speed up buses, shelters, and neighborhood rezoning.',
        content:
          'The mayors say shared purchasing power will cut costs and help standardize cleaner fleets before the next monsoon season.',
        tone: 'amber',
        tag: 'Featured',
      }),
      createStory({
        id: 'politics-side-2',
        category: 'Politics',
        title: 'Watchdogs push for live contract dashboards in every ministry',
        excerpt:
          'Anti-corruption groups want the public to track spending decisions in real time instead of waiting for annual reports.',
        content:
          'Their proposal would force ministries to publish vendor changes, overruns, and audit flags in a searchable public ledger.',
        tone: 'teal',
        tag: 'Policy',
      }),
    ],
    trendingStories: [
      createStory({
        id: 'politics-trending-1',
        category: 'Politics',
        title: 'Election auditors test a faster absentee ballot chain',
        excerpt: 'Pilot districts are using barcode checkpoints to reduce recount disputes.',
        content:
          'The trial combines pickup scans, sealed transfers, and public chain logs to reduce the gap between mail receipt and final validation.',
        author: 'Civic Desk',
        time: '4 min read',
        tone: 'blue',
        tag: 'New',
      }),
      createStory({
        id: 'politics-trending-2',
        category: 'Politics',
        title: 'Opposition leaders reframe the food-price debate around logistics',
        excerpt: 'The latest speeches focus less on slogans and more on port bottlenecks and wholesale storage.',
        content:
          'Opposition strategists say the issue is landing with voters because delivery costs remain visible in daily household shopping.',
        author: 'National Desk',
        time: '5 min read',
        tone: 'amber',
        tag: 'Analysis',
      }),
      createStory({
        id: 'politics-trending-3',
        category: 'Politics',
        title: 'Senate panel advances new campaign disclosure rules',
        excerpt: 'Platforms would need to label funders behind major digital persuasion buys.',
        content:
          'The measure stops short of a full archive mandate but introduces tighter verification for bulk political purchases.',
        author: 'Policy Wire',
        time: '6 min read',
        tone: 'teal',
        tag: 'Watch',
      }),
      createStory({
        id: 'politics-trending-4',
        category: 'Politics',
        title: 'Regional governors negotiate a flood-relief package before monsoon peak',
        excerpt: 'Infrastructure repair and crop insurance now sit at the center of a fast-moving emergency pact.',
        content:
          'Officials want to pre-approve repair vendors and release small grants within days instead of weeks after major rainfall.',
        author: 'State Desk',
        time: '5 min read',
        tone: 'rose',
        tag: 'Live',
      }),
    ],
  },
  tech: {
    label: 'Tech',
    heroClass: 'hero-tech',
    kicker: 'Future Stack',
    description:
      'AI products, chips, robotics, and startup pivots driving the next software and hardware cycle.',
    tickerItems: [
      'Chipmakers race to cool AI racks without slowing inference.',
      'Startups are rebuilding workflow apps around copilots and agents.',
      'Open source robotics stacks are moving from labs into warehouses.',
    ],
    heroStory: createStory({
      id: 'tech-hero',
      category: 'Tech',
      tag: 'Tech Desk',
      title: 'Chipmakers race to cool AI data centers before power costs spiral',
      excerpt:
        'New liquid loops, denser racks, and power-aware schedulers are becoming the real battleground behind generative AI growth.',
      content:
        'The next wave of AI infrastructure is less about flashy demos and more about how operators squeeze heat, latency, and cost out of every deployment. Cooling design is now shaping product margins as much as model quality.',
      author: 'By Nisha Karki',
      time: '7 min read',
      readTime: '7 min read',
      tone: 'blue',
      location: 'Singapore',
    }),
    featuredSideStories: [
      createStory({
        id: 'tech-side-1',
        category: 'Tech',
        title: 'Factory teams adopt open-source robot brains to cut deployment time',
        excerpt:
          'A new generation of modular control stacks is making industrial pilots cheaper and easier to retrain.',
        content:
          'Vendors are bundling simulation, safety checks, and telemetry so smaller operators can tune robotic lines without a huge integrator contract.',
        tone: 'teal',
        tag: 'Automation',
      }),
      createStory({
        id: 'tech-side-2',
        category: 'Tech',
        title: 'Product managers rewrite roadmaps around AI memory and retrieval',
        excerpt:
          'The most useful copilots are no longer chat wrappers; they remember work context and fit inside daily tools.',
        content:
          'Teams are shifting budget away from one-off experiments toward retrieval quality, action reliability, and permission design.',
        tone: 'amber',
        tag: 'Build',
      }),
    ],
    trendingStories: [
      createStory({
        id: 'tech-trending-1',
        category: 'Tech',
        title: 'Carbon-aware cloud schedulers win a new round of enterprise pilots',
        excerpt: 'Ops teams are timing non-urgent training jobs to cleaner and cheaper energy windows.',
        content:
          'Early pilots show savings in both spend and emissions by moving background workloads across time zones and grid conditions.',
        author: 'Infra Desk',
        time: '4 min read',
        tone: 'blue',
        tag: 'New',
      }),
      createStory({
        id: 'tech-trending-2',
        category: 'Tech',
        title: 'A privacy-first browser pushes local AI summaries to the mainstream',
        excerpt: 'On-device article and tab synthesis is becoming a selling point instead of a lab demo.',
        content:
          'The browser team says users respond when assistance feels immediate and private rather than cloud-dependent.',
        author: 'Product Wire',
        time: '5 min read',
        tone: 'rose',
        tag: 'Product',
      }),
      createStory({
        id: 'tech-trending-3',
        category: 'Tech',
        title: 'Design systems swap static docs for living component simulators',
        excerpt: 'Teams want tokens, accessibility states, and usage patterns in one interactive place.',
        content:
          'The newer tools help designers and engineers validate edge cases before features reach QA, reducing handoff drift.',
        author: 'UX Desk',
        time: '6 min read',
        tone: 'amber',
        tag: 'Workflow',
      }),
      createStory({
        id: 'tech-trending-4',
        category: 'Tech',
        title: 'Regional founders chase boring software again after the AI rush',
        excerpt: 'Payments reconciliation, compliance, and logistics tools are quietly attracting healthier margins.',
        content:
          'Investors say durable products with clear buyers are regaining attention as pure hype plays get harder to defend.',
        author: 'Startup Desk',
        time: '5 min read',
        tone: 'teal',
        tag: 'Signals',
      }),
    ],
  },
  science: {
    label: 'Science',
    heroClass: 'hero-science',
    kicker: 'Discovery Ledger',
    description:
      'Research breakthroughs, planetary science, climate monitoring, and lab results with real-world stakes.',
    tickerItems: [
      'Deep-ocean instruments reveal a hidden current shift below Antarctic ice.',
      'New coral nurseries survive a severe reef heatwave in field trials.',
      'Astronomers refine the mineral map of a fast-approaching asteroid.',
    ],
    heroStory: createStory({
      id: 'science-hero',
      category: 'Science',
      tag: 'Science Desk',
      title: 'Deep-ocean sensors catch a hidden current shift beneath Antarctic ice',
      excerpt:
        'Researchers say a subtle change in underwater flow could explain why one shelf is weakening faster than previous models projected.',
      content:
        'The instruments tracked temperature, salinity, and velocity through a narrow under-ice channel for months. The result gives glaciologists a better handle on where warming water is doing the most damage.',
      author: 'By Arjun Rana',
      time: '9 min read',
      readTime: '9 min read',
      tone: 'teal',
      location: 'Southern Ocean',
    }),
    featuredSideStories: [
      createStory({
        id: 'science-side-1',
        category: 'Science',
        title: 'Lab-grown coral fragments survive their first severe heatwave',
        excerpt:
          'Marine biologists call it an early but meaningful proof that assisted restoration can buy reefs time.',
        content:
          'The most resilient strains maintained color and growth despite temperature spikes that bleached nearby natural colonies.',
        tone: 'amber',
        tag: 'Field Note',
      }),
      createStory({
        id: 'science-side-2',
        category: 'Science',
        title: 'A telescope swarm sharpens the mineral profile of a near-Earth asteroid',
        excerpt:
          'The updated spectral map could help scientists plan both deflection tests and future extraction debates.',
        content:
          'Researchers stitched together observations from multiple observatories to isolate metallic bands previously hidden by noise.',
        tone: 'blue',
        tag: 'Space',
      }),
    ],
    trendingStories: [
      createStory({
        id: 'science-trending-1',
        category: 'Science',
        title: 'Urban heat sensors show tree canopies outperform reflective paint at noon',
        excerpt: 'Street-level data is giving planners clearer rules for cooling dense neighborhoods.',
        content:
          'The best results came from layering shade, lighter materials, and narrower asphalt footprints rather than relying on one intervention.',
        author: 'Climate Lab',
        time: '4 min read',
        tone: 'teal',
        tag: 'Climate',
      }),
      createStory({
        id: 'science-trending-2',
        category: 'Science',
        title: 'A new blood test shortens the path to targeted cancer therapy',
        excerpt: 'Researchers can now spot treatment-linked mutations with less invasive screening.',
        content:
          'Clinicians say the assay may help them adjust therapies earlier and reduce unnecessary cycles of ineffective drugs.',
        author: 'Health Science',
        time: '6 min read',
        tone: 'rose',
        tag: 'Medical',
      }),
      createStory({
        id: 'science-trending-3',
        category: 'Science',
        title: 'Seismic imaging points to a fresh geothermal zone under a growing city',
        excerpt: 'Energy planners are assessing whether a local heat source can support district heating.',
        content:
          'The survey gives engineers a deeper look at fault spacing and fluid channels before expensive drilling begins.',
        author: 'Earth Desk',
        time: '5 min read',
        tone: 'amber',
        tag: 'Energy',
      }),
      createStory({
        id: 'science-trending-4',
        category: 'Science',
        title: 'Drone ecologists build a faster census of alpine pollinators',
        excerpt: 'The new workflow captures flower density and insect traffic over terrain that is hard to sample on foot.',
        content:
          'Researchers say the combined image-and-audio approach is finally making remote biodiversity counts scalable.',
        author: 'Research Wire',
        time: '5 min read',
        tone: 'blue',
        tag: 'Research',
      }),
    ],
  },
  culture: {
    label: 'Culture',
    heroClass: 'hero-culture',
    kicker: 'Scene Report',
    description:
      'Festivals, film, music, books, and design stories shaping how cities feel right now.',
    tickerItems: [
      'Neighborhood festivals are turning side streets into open-air galleries.',
      'Streaming studios are reviving regional thrillers with theatrical rollouts.',
      'Independent publishers are finding new readers through live reading salons.',
    ],
    heroStory: createStory({
      id: 'culture-hero',
      category: 'Culture',
      tag: 'Culture Desk',
      title: 'Neighborhood festivals are turning city blocks into living galleries',
      excerpt:
        'Curators, food vendors, and local bands are reshaping cultural calendars around smaller, more walkable events.',
      content:
        'The new model values repeated local participation over one giant annual spectacle. Organizers say that mix is creating stronger loyalty and more room for emerging artists.',
      author: 'By Sachi Gautam',
      time: '6 min read',
      readTime: '6 min read',
      tone: 'amber',
      location: 'Patan',
    }),
    featuredSideStories: [
      createStory({
        id: 'culture-side-1',
        category: 'Culture',
        title: 'A restored single-screen theater becomes the citys new date-night magnet',
        excerpt:
          'Programmers are blending repertory classics, live scores, and filmmaker talks into one weekly ritual.',
        content:
          'The owners are betting that atmosphere and curation can compete with frictionless at-home streaming habits.',
        tone: 'rose',
        tag: 'Cinema',
      }),
      createStory({
        id: 'culture-side-2',
        category: 'Culture',
        title: 'Small publishers turn bookstore basements into packed reading salons',
        excerpt:
          'The most memorable launches now feel closer to performances than product drops.',
        content:
          'Writers are testing unpublished work live, while booksellers mix poetry, translation, and music into tighter community nights.',
        tone: 'teal',
        tag: 'Books',
      }),
    ],
    trendingStories: [
      createStory({
        id: 'culture-trending-1',
        category: 'Culture',
        title: 'Museum teams rethink labels to sound less academic and more human',
        excerpt: 'Shorter wall text and audio stories are keeping younger visitors engaged longer.',
        content:
          'The shift does not reduce rigor; it simply brings interpretation closer to spoken language and lived experience.',
        author: 'Arts Desk',
        time: '4 min read',
        tone: 'amber',
        tag: 'New',
      }),
      createStory({
        id: 'culture-trending-2',
        category: 'Culture',
        title: 'Fashion houses look to craft clusters for slower, sharper luxury',
        excerpt: 'Designers are prioritizing traceable making over trend speed.',
        content:
          'That repositioning is resonating with buyers who want fewer pieces, richer materials, and clearer provenance.',
        author: 'Style Wire',
        time: '5 min read',
        tone: 'rose',
        tag: 'Style',
      }),
      createStory({
        id: 'culture-trending-3',
        category: 'Culture',
        title: 'A regional pop tour proves small arenas can still feel electric',
        excerpt: 'Promoters are winning with tighter production, better acoustics, and smarter pricing.',
        content:
          'The format gives artists more room to connect with fans while keeping travel and stage complexity under control.',
        author: 'Music Desk',
        time: '6 min read',
        tone: 'blue',
        tag: 'Music',
      }),
      createStory({
        id: 'culture-trending-4',
        category: 'Culture',
        title: 'Public libraries become the quiet backbone of creator communities',
        excerpt: 'Podcast circles, zine swaps, and design workshops are filling rooms once reserved for lectures.',
        content:
          'Library teams say the most successful programs now treat residents as contributors rather than passive attendees.',
        author: 'City Culture',
        time: '5 min read',
        tone: 'teal',
        tag: 'Community',
      }),
    ],
  },
  opinion: {
    label: 'Opinion',
    heroClass: 'hero-opinion',
    kicker: 'Point of View',
    description:
      'Sharp arguments, informed commentary, and essays that slow the scroll and force a second look.',
    tickerItems: [
      'Editors argue for city budgets that measure dignity, not just growth.',
      'Writers challenge the obsession with frictionless software at any cost.',
      'Columnists make the case for stronger public design standards.',
    ],
    heroStory: createStory({
      id: 'opinion-hero',
      category: 'Opinion',
      tag: 'Opinion',
      title: 'Why cities should measure dignity, not only growth',
      excerpt:
        'A city can add towers, lanes, and dashboards yet still fail to feel fair. The next benchmark should be whether everyday systems respect people.',
      content:
        'Growth metrics are easy to count and easy to headline. Dignity is harder. But once you start measuring time lost to queues, inaccessible sidewalks, or hostile service design, the public picture gets more honest.',
      author: 'By Rhea Shrestha',
      time: '7 min read',
      readTime: '7 min read',
      tone: 'amber',
      location: 'Editorial Board',
    }),
    featuredSideStories: [
      createStory({
        id: 'opinion-side-1',
        category: 'Opinion',
        title: 'The software industry needs fewer features and more restraint',
        excerpt:
          'Complexity keeps being sold as ambition, even when the calmer product is clearly the better one.',
        content:
          'The best tools disappear into the work. They do not interrupt every task with a new panel, pulse, or badge.',
        tone: 'blue',
        tag: 'Essay',
      }),
      createStory({
        id: 'opinion-side-2',
        category: 'Opinion',
        title: 'Public architecture deserves the same storytelling as private towers',
        excerpt:
          'Transit halls, schools, and clinics shape civic memory more than many luxury projects ever will.',
        content:
          'If the places most people actually use are designed as afterthoughts, cities end up teaching inequality through space.',
        tone: 'rose',
        tag: 'Commentary',
      }),
    ],
    trendingStories: [
      createStory({
        id: 'opinion-trending-1',
        category: 'Opinion',
        title: 'Remote work policy should be written around outcomes, not optics',
        excerpt: 'The loudest arguments still confuse visibility with accountability.',
        content:
          'Better policy starts by defining what good work looks like, then choosing the collaboration rhythm that supports it.',
        author: 'Workplace Column',
        time: '4 min read',
        tone: 'amber',
        tag: 'New',
      }),
      createStory({
        id: 'opinion-trending-2',
        category: 'Opinion',
        title: 'Design debt is often just leadership debt wearing nicer clothes',
        excerpt: 'Teams rarely suffer from a lack of taste; they suffer from unclear decisions and late priorities.',
        content:
          'When leaders delay calls until implementation, every interface becomes a compromise between urgency and guesswork.',
        author: 'Design Notebook',
        time: '5 min read',
        tone: 'blue',
        tag: 'View',
      }),
      createStory({
        id: 'opinion-trending-3',
        category: 'Opinion',
        title: 'Universities should publish the civic value of their research in plain language',
        excerpt: 'If public money funds discovery, the public deserves clearer storytelling in return.',
        content:
          'That does not mean oversimplifying the science. It means explaining why the work matters before a press release turns it into noise.',
        author: 'Civic Voice',
        time: '6 min read',
        tone: 'teal',
        tag: 'Essay',
      }),
      createStory({
        id: 'opinion-trending-4',
        category: 'Opinion',
        title: 'The best newsroom subscriptions win by becoming useful habits',
        excerpt: 'People do not pay forever for urgency alone; they pay for rhythm, trust, and clarity.',
        content:
          'Habit is what turns a one-time article into a lasting relationship, and that demands editorial consistency more than gimmicks.',
        author: 'Media Watch',
        time: '5 min read',
        tone: 'rose',
        tag: 'Media',
      }),
    ],
  },
};

const sectionKeys = Object.keys(sectionContent);

const sectionAliases = {
  politics: 'politics',
  political: 'politics',
  tech: 'tech',
  technology: 'tech',
  science: 'science',
  culture: 'culture',
  opinion: 'opinion',
  home: 'politics',
  all: 'politics',
};

export const normalizeSectionKey = (value) => {
  const key = String(value || '')
    .replace(/^#\/?/, '')
    .replace(/^\//, '')
    .trim()
    .toLowerCase();

  return sectionAliases[key] || 'politics';
};

export const getSectionFromHash = () => {
  if (typeof window === 'undefined') return 'politics';
  return normalizeSectionKey(window.location.hash);
};

const normalizeStoryFromApi = (story, sectionKey, index) => {
  const section = sectionContent[sectionKey];

  return createStory({
    id: story.id || `${sectionKey}-api-${index}`,
    category: story.category || section.label,
    title: story.title || section.heroStory.title,
    excerpt: story.excerpt || story.summary || story.content || section.description,
    content: story.content || story.excerpt || story.summary || section.description,
    author: story.author || 'Editorial Team',
    time: story.time || story.readTime || '5 min read',
    readTime: story.readTime || story.time || '5 min read',
    tone: story.tone || tones[index % tones.length],
    tag: story.tag || 'New',
    imageUrl: story.imageUrl || '',
    location: story.location || section.label,
    publishedAt: story.publishedAt || story.date || '',
  });
};

const fillStories = (primaryStories, fallbackStories, sectionKey, count) => {
  const merged = [...primaryStories];
  const usedTitles = new Set(primaryStories.map((story) => story.title));

  for (const story of fallbackStories) {
    if (merged.length >= count) break;
    if (usedTitles.has(story.title)) continue;
    merged.push(createStory({ ...story, id: `${sectionKey}-${story.id}` }));
  }

  return merged.slice(0, count);
};

export const buildSectionExperience = (sectionKey, incomingStories = []) => {
  const safeSection = normalizeSectionKey(sectionKey);
  const base = sectionContent[safeSection];
  const normalizedStories = Array.isArray(incomingStories)
    ? incomingStories.map((story, index) => normalizeStoryFromApi(story, safeSection, index))
    : [];

  const heroStory = normalizedStories[0]
    ? {
        ...base.heroStory,
        ...normalizedStories[0],
        tag: base.heroStory.tag,
        category: base.label,
        readTime:
          normalizedStories[0].readTime || normalizedStories[0].time || base.heroStory.readTime,
      }
    : base.heroStory;

  const featuredSideStories = fillStories(
    normalizedStories.slice(1, 3),
    base.featuredSideStories,
    safeSection,
    2,
  );

  const trendingStories = fillStories(
    normalizedStories.slice(0, 4),
    base.trendingStories,
    safeSection,
    4,
  );

  return {
    ...base,
    heroStory,
    featuredSideStories,
    trendingStories,
    tickerItems:
      normalizedStories.length > 0
        ? normalizedStories.slice(0, 3).map((story) => story.title)
        : base.tickerItems,
  };
};

export const homeSections = sectionKeys.map((key) => ({
  key,
  label: sectionContent[key].label,
}));
