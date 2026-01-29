# Script chạy tests với output đầy đủ
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CHẠY UNIT TESTS - SERVICE LAYER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Chạy tests với detailed output
dotnet test PRN222.CourseManagement.Services.Tests/PRN222.CourseManagement.Services.Tests.csproj `
    --logger:"console;verbosity=detailed" `
    --nologo

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  KẾT QUẢ TESTS" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
