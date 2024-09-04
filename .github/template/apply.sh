#!/bin/bash

repository=$1

REPO_NAME=$(echo "${repository}" | awk -F '/' '{print $2}')
REPO_NAME_PASCAL_CASE=$(echo "$REPO_NAME" | sed -r 's/(^|-)([a-z])/\U\2/g')

echo "applying template..."

echo "renaming cdp-dotnet-backend-template -> ${REPO_NAME}"
find . -name .git -prune -o -name .github -prune -o -type f -exec sed -i "s/cdp-dotnet-backend-template/${REPO_NAME}/g" {} \;

echo "Backend.Api -> ${REPO_NAME_PASCAL_CASE}"
find . -name .git -prune -o -name .github -prune -o -type f -exec sed -i "s/Backend\.Api/${REPO_NAME_PASCAL_CASE}/g" {} \;

echo "CDP C# ASP\.NET Backend Template -> ${REPO_NAME}"
find . -name .git -prune -o -name .github -prune -o -type f -exec sed -i "s/CDP C# ASP\.NET Backend Template/${REPO_NAME}/g" {} \;

echo "Moving Backend\.Api* -> ${REPO_NAME_PASCAL_CASE}"
find . -depth -name .git -prune -o -name .github -prune -o -name 'Backend\.Api*' -execdir bash -c 'mv -f "$1" "${1//Backend\.Api/${REPO_NAME_PASCAL_CASE}}"' bash {} \;   

echo "Renaming CdpDotnetBackendTemplate.sln -> ${REPO_NAME_PASCAL_CASE}.sln"   
mv CdpDotnetBackendTemplate.sln "${REPO_NAME_PASCAL_CASE}".sln  

tree