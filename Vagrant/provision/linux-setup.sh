#!/bin/bash
set -e

echo "=== Installing Docker ==="
sudo apt-get update -y
sudo apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release

curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update -y
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

echo "=== Starting BaGet container ==="
mkdir -p /home/vagrant/baget_data/storage
cat > /home/vagrant/baget_data/BaGet.json <<'EOF'
{
  "Database": {
    "Type": "Sqlite",
    "ConnectionString": "Data Source=/var/baget/baget.db"
  },
  "Storage": {
    "Type": "FileSystem",
    "Path": "/var/baget"
  },
  "Search": {
    "Type": "Database"
  },
  "NuGet": {
    "AllowPackageUpload": true,
    "AllowSymbolUpload": true
  },
  "Mirror": {
    "Enabled": false
  }
}
EOF

cat > /home/vagrant/baget_data/docker-compose.yml <<'EOF'
version: '3.8'
services:
  baget:
    image: loicsharma/baget:latest
    container_name: baget
    environment:
      - ASPNETCORE_URLS=http://+:80
    ports:
      - "5000:80"
    volumes:
      - ./storage:/var/baget
      - ./BaGet.json:/app/appsettings.json
EOF

cd /home/vagrant/baget_data
sudo docker compose up -d

echo "=== BaGet is running at http://192.168.56.10:5000 ==="

echo "=== Installing .NET SDK ==="
sudo apt-get install -y wget apt-transport-https
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update -y
sudo apt-get install -y dotnet-sdk-7.0

echo "=== Adding BaGet as NuGet Source ==="
sudo -u vagrant dotnet nuget remove source baget 2>/dev/null || true
sudo -u vagrant dotnet nuget add source \
  "http://192.168.56.10:5000/v3/index.json" \
  --name baget
