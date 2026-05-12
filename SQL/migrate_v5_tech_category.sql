
INSERT INTO stories
    (bucket, category, title, excerpt, author, read_time_minutes,
     tone, is_pinned, pin_order, status, published_at, created_at, updated_at)
VALUES
    ('heroStory', 'tech', 'The Future of AI in Everyday Life',
     'Artificial intelligence is reshaping how we live, work, and communicate — here''s what comes next.',
     'Admin', 5, 'informative', 0, 0, 'published', NOW(), NOW(), NOW()),

    ('featuredSideStories', 'tech', 'Top 5 Programming Languages in 2026',
     'From Python to Rust, we break down which languages are dominating the developer landscape this year.',
     'Admin', 4, 'informative', 0, 0, 'published', NOW(), NOW(), NOW()),

    ('featuredSideStories', 'tech', 'Cloud Computing: AWS vs Azure vs GCP',
     'A detailed comparison of the three biggest cloud platforms to help your team choose wisely.',
     'Admin', 6, 'analytical', 0, 0, 'published', NOW(), NOW(), NOW()),

    ('trendingStories', 'tech', 'Open Source is Eating the World',
     'More companies than ever are betting on open source software — and winning.',
     'Admin', 3, 'positive', 1, 1, 'published', NOW(), NOW(), NOW()),

    ('trendingStories', 'tech', 'Cybersecurity Threats on the Rise in 2026',
     'New attack vectors are emerging faster than defenses can adapt. What you need to know.',
     'Admin', 5, 'urgent', 1, 2, 'published', NOW(), NOW(), NOW()),

    ('trendingStories', 'tech', 'The Rise of Edge Computing',
     'Why processing data closer to the source is becoming the new normal for IoT and mobile apps.',
     'Admin', 4, 'informative', 0, 0, 'published', NOW(), NOW(), NOW());