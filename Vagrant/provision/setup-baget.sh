#!/bin/bash
set -e

echo "=== Installing Docker prerequisites ==="
sudo apt-get update -y
sudo apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release

# Завантажуємо ключ і додаємо у trusted keyring
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | \
  sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] \
https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | \
sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update -y
sudo apt-get install -y docker-ce docker-ce-cli containerd.io

# Додаємо користувача vagrant у групу docker
sudo usermod -aG docker vagrant
sudo systemctl enable --now docker

echo "=== Preparing BaGet data directory ==="
sudo mkdir -p /home/vagrant/baget_data/storage
sudo chown -R vagrant:vagrant /home/vagrant/baget_data

echo "=== Creating BaGet.json config ==="
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
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:80"
      }
    }
  }
}
EOF

echo "=== Removing old BaGet container if exists ==="
sudo docker rm -f baget 2>/dev/null || true

echo "=== Starting BaGet container ==="
sudo docker run -d --name baget \
  -p 5000:80 \
  -v /home/vagrant/baget_data/storage:/var/baget \
  -v /home/vagrant/baget_data/BaGet.json:/app/appsettings.json \
  loicsharma/baget:latest

echo "=== Waiting for BaGet to become available ==="
for i in {1..10}; do
  if curl -sf http://192.168.56.10:5000/v3/index.json >/dev/null; then
    echo "BaGet is up!"
    break
  fi
  echo "BaGet not ready yet... retrying ($i)"
  sleep 3
done

echo "=== Installing .NET SDK ==="
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update -y
sudo apt-get install -y dotnet-sdk-9.0

echo "=== Packing SmartGreenhouse.Web as NuGet package ==="
cd /vagrant/SmartGreenhouse.Web
dotnet restore
dotnet build -c Release
dotnet pack -c Release -o /vagrant/nupkgs

echo "=== Registering BaGet as NuGet source ==="
sudo -u vagrant dotnet nuget remove source baget 2>/dev/null || true
sudo -u vagrant dotnet nuget add source \
  "http://192.168.56.10:5000/v3/index.json" \
  --name baget \
  --store-password-in-clear-text \
  --username dummy \
  --password dummy

echo "=== Pushing package to BaGet ==="
dotnet nuget push /vagrant/nupkgs/*.nupkg --source baget --skip-duplicate

echo "=== Setup complete: SmartGreenhouse.Web package uploaded to BaGet ==="
