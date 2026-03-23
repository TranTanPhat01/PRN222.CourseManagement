-- =============================================
-- Script: Update Enrollment Table Schema
-- Description: Add missing IsGradeFinalized column
-- Date: 2026-03-23
-- =============================================

USE CourseManagementDB;
GO

-- Check if IsGradeFinalized column already exists before adding
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Enrollment') 
    AND name = 'IsGradeFinalized'
)
BEGIN
    PRINT 'Adding IsGradeFinalized column to Enrollment table...';
    
    ALTER TABLE Enrollment
    ADD IsGradeFinalized BIT NOT NULL DEFAULT 0;
    
    PRINT 'IsGradeFinalized column added successfully!';
    PRINT 'Default value: 0 (False)';
END
ELSE
BEGIN
    PRINT 'IsGradeFinalized column already exists.';
END
GO

-- Verify the changes
PRINT 'Verifying Enrollment table schema...';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Enrollment'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Schema update completed successfully!';
