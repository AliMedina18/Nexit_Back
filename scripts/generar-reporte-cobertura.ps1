# Genera un reporte HTML de cobertura de código para Nexit_Back.
# Ver docs/08-tipos-de-pruebas.md, sección "Cobertura de código: cómo verla".
#
# Uso: desde la raíz del repo, en PowerShell:
#   .\scripts\generar-reporte-cobertura.ps1
#
# La primera vez necesitas instalar la herramienta (una sola vez por repo):
#   dotnet new tool-manifest
#   dotnet tool install dotnet-reportgenerator-globaltool

$ErrorActionPreference = "Stop"

Write-Host "Corriendo las pruebas y recolectando cobertura..." -ForegroundColor Cyan
dotnet test tests/Nexit.Tests/Nexit.Tests.csproj --collect:"XPlat Code Coverage"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Las pruebas no pasaron -- corrige eso antes de ver el reporte de cobertura." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Generando el reporte HTML..." -ForegroundColor Cyan
dotnet reportgenerator -reports:"tests/Nexit.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html

$reportPath = Join-Path (Get-Location) "CoverageReport\index.html"
Write-Host "Listo. Abre este archivo en el navegador: $reportPath" -ForegroundColor Green
