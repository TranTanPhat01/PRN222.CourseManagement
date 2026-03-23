-- =============================================
-- Script: Update Student Table Schema
-- Description: Add missing DateOfBirth and IsActive columns
-- Date: 2026-01-31
-- =============================================

USE CourseManagementDB;
GO

-- Check if columns already exist before adding
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Student') 
    AND name = 'DateOfBirth'
)
BEGIN
    PRINT 'Adding DateOfBirth column to Student table...';
    
    ALTER TABLE Student
    ADD DateOfBirth DATE NOT NULL DEFAULT '2000-01-01';
    
    PRINT 'DateOfBirth column added successfully!';
END
ELSE
BEGIN
    PRINT 'DateOfBirth column already exists.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Student') 
    AND name = 'IsActive'
)
BEGIN
    PRINT 'Adding IsActive column to Student table...';
    
    ALTER TABLE Student
    ADD IsActive BIT NOT NULL DEFAULT 1;
    
    PRINT 'IsActive column added successfully!';
END
ELSE
BEGIN
    PRINT 'IsActive column already exists.';
END
GO

-- Verify the changes
PRINT 'Verifying Student table schema...';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Student'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Schema update completed successfully!';
