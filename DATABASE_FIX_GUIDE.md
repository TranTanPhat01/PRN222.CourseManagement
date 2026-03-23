# 🔧 DATABASE SCHEMA UPDATE GUIDE

## ❌ PROBLEM
Database is missing two columns in the `Student` table:
- `DateOfBirth` (DATE)
- `IsActive` (BIT)

Error message:
```
Invalid column name 'DateOfBirth'.
Invalid column name 'IsActive'.
```

---

## ✅ SOLUTION - Choose ONE of the following methods:

### **METHOD 1: Run SQL Script (RECOMMENDED - Fastest & Safest)**

#### Step 1: Open SQL Server Management Studio (SSMS)
- Or use Azure Data Studio
- Or Visual Studio SQL Server Object Explorer
- Or any SQL client

#### Step 2: Connect to your SQL Server
- Server: `LAPTOP-IMPJU6CL\SQLEXPRESS`
- Authentication: SQL Server Authentication
- Username: `sa`
- Password: `12345`

#### Step 3: Run the SQL Script
1. Open file: `UpdateStudentSchema.sql` (in the solution root folder)
2. Execute the script (F5 or click Execute button)
3. Verify output shows "Schema update completed successfully!"

#### Step 4: Restart your application
```bash
cd "d:\KI 7 FPT\PRN222\PRN222\PRN222.CourseManagement\PRN222.CourseManagement.Web"
dotnet run
```

---

### **METHOD 2: Run SQL via Command Line (Alternative)**

If you don't have SSMS, you can use sqlcmd:

```powershell
# Navigate to solution folder
cd "d:\KI 7 FPT\PRN222\PRN222\PRN222.CourseManagement"

# Run SQL script using sqlcmd
sqlcmd -S LAPTOP-IMPJU6CL\SQLEXPRESS -U sa -P 12345 -i UpdateStudentSchema.sql

# Or connect interactively
sqlcmd -S LAPTOP-IMPJU6CL\SQLEXPRESS -U sa -P 12345 -d CourseManagementDB
```

Then paste and execute:
```sql
ALTER TABLE Student ADD DateOfBirth DATE NOT NULL DEFAULT '2000-01-01';
ALTER TABLE Student ADD IsActive BIT NOT NULL DEFAULT 1;
GO
```

---

### **METHOD 3: EF Core Migrations (If you want to maintain migration history)**

#### Step 1: Install EF Core Tools (if not already installed)
```bash
dotnet tool install --global dotnet-ef
```

#### Step 2: Create and Apply Migration from Web Project
```bash
# Navigate to Web project
cd "d:\KI 7 FPT\PRN222\PRN222\PRN222.CourseManagement\PRN222.CourseManagement.Web"

# Add migration (references CourseManagement project which has DbContext)
dotnet ef migrations add AddDateOfBirthAndIsActive --project ../CourseManagement/CourseManagement.csproj --context CourseManagementContext

# Apply migration to database
dotnet ef database update --project ../CourseManagement/CourseManagement.csproj --context CourseManagementContext
```

---

## 🔍 VERIFY THE FIX

After running the script, verify the columns exist:

```sql
USE CourseManagementDB;
GO

-- Check Student table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Student'
ORDER BY ORDINAL_POSITION;
GO
```

Expected output should include:
```
DateOfBirth | date | NO  | ('2000-01-01')
IsActive    | bit  | NO  | ((1))
```

---

## 📊 EXPECTED STUDENT TABLE SCHEMA (After Fix)

| Column Name   | Data Type      | Nullable | Default      |
|---------------|----------------|----------|--------------|
| StudentId     | int            | NO       | IDENTITY     |
| StudentCode   | nvarchar(20)   | NO       | -            |
| FullName      | nvarchar(100)  | NO       | -            |
| Email         | nvarchar(100)  | NO       | -            |
| DepartmentId  | int            | NO       | -            |
| **DateOfBirth** | **date**     | **NO**   | **'2000-01-01'** |
| **IsActive**    | **bit**      | **NO**   | **1**        |

---

## 🚀 AFTER FIXING

1. **Restart your application**
   ```bash
   cd "d:\KI 7 FPT\PRN222\PRN222\PRN222.CourseManagement\PRN222.CourseManagement.Web"
   dotnet run
   ```

2. **Access the application**
   - URL: https://localhost:5001 or http://localhost:5000
   - You should now be able to:
     - ✅ View students list
     - ✅ Create new students (with DateOfBirth and IsActive)
     - ✅ Edit students
     - ✅ See active/inactive status badges

3. **Test the fix**
   - Navigate to Students page
   - Try creating a new student
   - Verify DateOfBirth and IsActive fields work properly

---

## 🆘 TROUBLESHOOTING

### Issue: "Cannot open database 'CourseManagementDB'"
**Solution:** The database doesn't exist yet. Create it first:
```sql
CREATE DATABASE CourseManagementDB;
GO
```

### Issue: "Login failed for user 'sa'"
**Solution:** Check SQL Server authentication mode:
1. Right-click SQL Server instance in SSMS
2. Properties → Security
3. Select "SQL Server and Windows Authentication mode"
4. Restart SQL Server service

### Issue: Still getting "Invalid column name" error
**Solution:** 
1. Verify the script ran successfully
2. Check you're connected to the correct database
3. Restart the application (important!)
4. Clear EF Core query cache by restarting

---

## 📝 NOTES

- **Default Values:**
  - `DateOfBirth`: Set to '2000-01-01' for all existing records
  - `IsActive`: Set to `1` (true) for all existing records
  
- **Data Safety:**
  - This script is non-destructive
  - Only ADDS columns, doesn't DELETE or MODIFY existing data
  - Safe to run multiple times (has existence checks)

- **After Update:**
  - All existing students will have:
    - DateOfBirth = 2000-01-01 (you can update manually if needed)
    - IsActive = true (active status)

---

## ✅ CHECKLIST

- [ ] Backup database (optional but recommended)
- [ ] Run `UpdateStudentSchema.sql` script
- [ ] Verify columns added (check schema)
- [ ] Restart application
- [ ] Test Students CRUD operations
- [ ] Verify no more "Invalid column name" errors

---

**Ready to proceed!** Choose Method 1 (SQL Script) for fastest results. 🚀
