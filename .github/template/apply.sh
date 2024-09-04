#!/bin/bash

repository=$1

REPO_NAME=$(echo "${repository}" | awk -F '/' '{print $2}')
REPO_NAME_PASCAL_CASE=$(echo "$REPO_NAME" | sed -r 's/(^|-)([a-z])/\U\2/g')

find . -name .git -prune -o -name .github -prune -o -type f -exec sed -i "s/cdp-dotnet-backend-template/${REPO_NAME}/g" {} \;
find . -name .git -prune -o -name .github -prune -o -type f -exec sed -i "s/Backend\.Api/${REPO_NAME_PASCAL_CASE}/g" {} \;
find . -name .git -prune -o -name .github -prune -o -type f -exec sed -i "s/CDP C# ASP\.NET Backend Template/${REPO_NAME}/g" {} \;
find . -name .git -prune -o -name .github -prune -o -depth -name '*Backend\.Api*' -execdir bash -c 'mv -i "$1" "${1//Backend\.Api/${{ env.REPO_NAME_PASCAL_CASE }}}"' bash {} \;   
mv CdpDotnetBackendTemplate.sln "${REPO_NAME_PASCAL_CASE}".sln  
