-- ============================================================
-- Migration: Create articles table for RSS crawler data
-- Date: 2026-03-13
-- ============================================================

CREATE TABLE IF NOT EXISTS articles (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id      UUID REFERENCES projects(id) ON DELETE SET NULL,
    title           TEXT NOT NULL,
    url             TEXT NOT NULL,
    source_name     VARCHAR(500),
    source_url      TEXT,
    pub_date        TIMESTAMPTZ,
    snippet         TEXT,
    image_url       TEXT,
    sentiment       VARCHAR(50),
    is_relevant     BOOLEAN DEFAULT TRUE,
    status          INT DEFAULT 1,
    created_at      TIMESTAMPTZ DEFAULT NOW(),
    updated_at      TIMESTAMPTZ DEFAULT NOW(),
    created_by      VARCHAR(255),
    updated_by      VARCHAR(255)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_articles_project_id ON articles(project_id);
CREATE INDEX IF NOT EXISTS idx_articles_pub_date ON articles(pub_date DESC);
CREATE UNIQUE INDEX IF NOT EXISTS idx_articles_url ON articles(url);
