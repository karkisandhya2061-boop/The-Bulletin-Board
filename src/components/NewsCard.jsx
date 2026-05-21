function NewsCard({ article = {}, onClick }) {
  const {
    imageUrl = '',
    title = 'Untitled',
    tone = 'blue',
    tag = '',
    category = '',
    excerpt = '',
    author = '',
    time = ''
  } = article || {};

  // Debug logging (safe)
  if (!imageUrl) {
    console.log(`[NewsCard] Article "${title}" has NO image`);
  } else if (imageUrl.length > 0) {
    console.log(`[NewsCard] Article "${title}" has image (${imageUrl.length} bytes)`);
  }
  
  return (
    <article 
      className={`news-card tone-${tone}`}
      onClick={() => onClick && onClick(article)}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => {
        if ((e.key === 'Enter' || e.key === ' ') && onClick) {
          onClick(article);
        }
      }}
      style={{ cursor: onClick ? 'pointer' : 'default' }}
    >
      <div className="news-card-image" aria-hidden="true">
        {imageUrl && imageUrl.length > 0 ? (
          <img 
            src={imageUrl} 
            alt={title}
            style={{ width: '100%', height: '100%', objectFit: 'cover' }}
            onError={(e) => console.error(`[NewsCard] Failed to load image for "${title}"`)}
            onLoad={(e) => console.log(`[NewsCard] Successfully loaded image for "${title}"`)}
          />
        ) : (
          <div className="news-card-glow" />
        )}
      </div>

      <div className="news-card-content">
        <span className="news-card-tag">{tag}</span>
        <p className="news-card-category">{category}</p>
        <h4>{title}</h4>
        <p className="news-card-excerpt">{excerpt}</p>
        <div className="news-card-meta">
          <span>{author}</span>
          <span>{time}</span>
        </div>
      </div>
    </article>
  );
}

export default NewsCard;
