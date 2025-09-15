#!/usr/bin/env bash
set -e
git init
git lfs install
git add .gitattributes .gitignore
git add .
git commit -m "Initial commit: Unity project"
git branch -M main
read -p "Enter GitHub repo URL (https://github.com/USER/REPO.git): " REMOTE
git remote add origin "$REMOTE"
git push -u origin main
