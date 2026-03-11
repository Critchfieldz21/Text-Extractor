#!/bin/bash
set -e

# Install .NET 9.0
curl https://dot.net/v1/dotnet-install.sh | bash -s -- --version 9.0.0 --install-dir /tmp/dotnet

# Add dotnet to PATH
export PATH="/tmp/dotnet:$PATH"

# Publish the application
dotnet publish -c Release -o publish
