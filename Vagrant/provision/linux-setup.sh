#!/bin/bash
set -e

echo "=== Updating apt and installing prerequisites ==="
sudo apt-get update -y
sudo apt-get install -y wget apt-transport-https sqlite3 ca-certificates curl gnupg lsb-release

echo "=== Installing .NET SDK ==="
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update -y
sudo apt-get install -y dotnet-sdk-9.0

echo "=== Creating NuGet.Config with allowInsecureConnections ==="
sudo -u vagrant mkdir -p /home/vagrant/.nuget/NuGet
cat > /home/vagrant/.nuget/NuGet/NuGet.Config <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="baget" value="http://192.168.56.10:5000/v3/index.json" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <config>
    <add key="allowInsecureConnections" value="true" />
  </config>
</configuration>
EOF
sudo chown -R vagrant:vagrant /home/vagrant/.nuget

echo "=== Building SmartGreenhouse.Web project ==="
cd /vagrant/SmartGreenhouse.Web
dotnet restore --configfile /home/vagrant/.nuget/NuGet/NuGet.Config
dotnet build -c Release

echo "=== Running SmartGreenhouse.Web on Linux VM ==="
nohup dotnet run --urls "http://0.0.0.0:6000" > /home/vagrant/greenhouse.log 2>&1 &

echo "=== SmartGreenhouse.Web is running at http://192.168.56.10:6000 ==="
echo "Log: /home/vagrant/greenhouse.log"
