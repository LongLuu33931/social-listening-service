-- Add is_approved column to tiktok_videos and youtube_videos
-- Default false = chưa duyệt, true = đã duyệt

ALTER TABLE tiktok_videos ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT false;
ALTER TABLE youtube_videos ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT false;

-- Also add is_approved to articles if needed (currently uses is_relevant)
ALTER TABLE articles ADD COLUMN IF NOT EXISTS is_approved BOOLEAN DEFAULT false;
