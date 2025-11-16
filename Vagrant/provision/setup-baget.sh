#!/bin/bash
set -e

echo "=== Updating apt ==="
sudo apt-get update -y

echo "=== Installing Docker and docker-compose ==="
sudo apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --batch --yes --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update -y
sudo apt-get install -y docker-ce docker-ce-cli containerd.io


# docker-compose plugin (modern)
sudo apt-get install -y docker-compose-plugin

# add vagrant user to docker group to run docker without sudo
sudo usermod -aG docker vagrant

# Create folder for baget data
sudo mkdir -p /home/vagrant/baget_data
sudo mkdir -p /home/vagrant/baget_data/storage  
sudo chown -R vagrant:vagrant /home/vagrant/baget_data

echo "=== Creating docker-compose.yml for BaGet ==="
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

# === Creating minimal BaGet.json config ===
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
  "PackagePublish": {
    "ApiKey": "my-secret-key"
  },
  "Mirror": {
    "Enabled": false
  }
}
EOF


# Fix permissions
sudo chown -R vagrant:vagrant /home/vagrant/baget_data

echo "=== Starting BaGet container ==="
cd /home/vagrant/baget_data

# Remove old stopped containers
sudo docker rm -f baget 2>/dev/null || true

# start container in detached mode
sudo docker compose up -d

echo "=== BaGet should be running at http://192.168.56.10:5000 ==="

sleep 5
sudo docker ps --filter "name=baget" --format "table {{.Names}}\t{{.Status}}"

# ──────────────────────────────────────────────
echo "=== Installing .NET SDK ==="
sudo apt-get install -y wget apt-transport-https
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update -y
sudo apt-get install -y dotnet-sdk-7.0

echo "=== Adding BaGet as NuGet Source inside VM ==="
sudo -u vagrant dotnet nuget remove source baget 2>/dev/null || true
sudo -u vagrant dotnet nuget add source \
  "http://192.168.56.10:5000/v3/index.json" \
  --name baget

# Optional test install (won't fail if package missing)
echo "=== Attempting to install test package (optional) ==="
sudo -u vagrant dotnet new console -o /home/vagrant/test-nuget 2>/dev/null || true
sudo -u vagrant dotnet add /home/vagrant/test-nuget package TestPackage --source baget || true

echo "=== Setup complete ==="
