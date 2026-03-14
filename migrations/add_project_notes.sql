-- Add notes column to projects table for client submissions
ALTER TABLE projects ADD COLUMN IF NOT EXISTS notes TEXT;
