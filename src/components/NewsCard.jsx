function NewsCard({ article, onClick }) {
  // Debug logging
  if (!article.imageUrl) {
    console.log(`[NewsCard] Article "${article.title}" has NO image`);
  } else if (article.imageUrl.length > 0) {
    console.log(`[NewsCard] Article "${article.title}" has image (${article.imageUrl.length} bytes)`);
  }
  
  return (
    <article 
      className={`news-card tone-${article.tone}`}
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
        {article.imageUrl && article.imageUrl.length > 0 ? (
          <img 
            src={article.imageUrl} 
            alt={article.title}
            style={{ width: '100%', height: '100%', objectFit: 'cover' }}
            onError={(e) => console.error(`[NewsCard] Failed to load image for "${article.title}"`)}
            onLoad={(e) => console.log(`[NewsCard] Successfully loaded image for "${article.title}"`)}
          />
        ) : (
          <div className="news-card-glow" />
        )}
      </div>

      <div className="news-card-content">
        <span className="news-card-tag">{article.tag}</span>
        <p className="news-card-category">{article.category}</p>
        <h4>{article.title}</h4>
        <p className="news-card-excerpt">{article.excerpt}</p>
        <div className="news-card-meta">
          <span>{article.author}</span>
          <span>{article.time}</span>
        </div>
      </div>
    </article>
  );
}

export default NewsCard;
