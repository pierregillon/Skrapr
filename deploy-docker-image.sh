#!/bin/bash

set -e

dotnet publish Skrapr/Skrapr.csproj -c Release -p linux-x64 -o out
docker build . -f NoBuildDockerfile -t skrapr -t "$1"/skrapr:"$2" --platform linux/amd64
docker push "$1"/skrapr:"$2"
rm -r out