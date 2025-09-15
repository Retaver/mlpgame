Param([string]$Remote="")
git init
git lfs install
git add .gitattributes .gitignore
git add .
git commit -m "Initial commit: Unity project"
git branch -M main
if (-not $Remote -or $Remote -eq "") { $Remote = Read-Host "Enter GitHub repo URL (https://github.com/USER/REPO.git)" }
git remote add origin $Remote
git push -u origin main
