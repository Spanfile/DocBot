﻿using System;
using System.Collections.Generic;
using DocBot.Services.Documentation;

namespace DocBot.Services
{
    public class DocumentationCacheContainer
    {
        public IReadOnlyList<DocumentationArticle> Articles { get; }
        public DateTimeOffset CreationTime { get; }
        // ReSharper disable once InconsistentNaming
        public TimeSpan TTL { get; }
        public bool Expired => DateTimeOffset.UtcNow > CreationTime + TTL;

        public DocumentationCacheContainer(IReadOnlyList<DocumentationArticle> articles, TimeSpan ttl)
        {
            Articles = articles;
            TTL = ttl;

            CreationTime = DateTimeOffset.UtcNow;
        }
    }
}
