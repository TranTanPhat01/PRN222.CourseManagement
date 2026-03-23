-- =============================================
-- Script: Update Course Table Schema
-- Description: Add missing Status column
-- Date: 2026-01-31
-- =============================================

USE CourseManagementDB;
GO

-- Check if Status column already exists before adding
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Course') 
    AND name = 'Status'
)
BEGIN
    PRINT 'Adding Status column to Course table...';
    
    -- Status is an enum (CourseStatus):
    -- 0 = Active, 1 = Inactive, 2 = Archived
    ALTER TABLE Course
    ADD Status INT NOT NULL DEFAULT 0;
    
    PRINT 'Status column added successfully!';
    PRINT 'Default value: 0 (Active)';
END
ELSE
BEGIN
    PRINT 'Status column already exists.';
END
GO

-- Verify the changes
PRINT 'Verifying Course table schema...';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Course'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Schema update completed successfully!';
PRINT '';
PRINT 'Status values:';
PRINT '  0 = Active (default)';
PRINT '  1 = Inactive';
PRINT '  2 = Archived';
