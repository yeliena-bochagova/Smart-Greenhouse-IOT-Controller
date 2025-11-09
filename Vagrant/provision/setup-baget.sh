#!/bin/bash
set -e

echo "=== Updating apt ==="
sudo apt-get update -y

echo "=== Installing Docker and docker-compose ==="
sudo apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
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
sudo chown -R vagrant:vagrant /home/vagrant/baget_data

echo "=== Creating docker-compose.yml for BaGet ==="
cat > /home/vagrant/baget_data/docker-compose.yml <<'EOF'
version: '3.8'
services:
  baget:
    image: loicsharma/baget:latest
    container_name: baget
    environment:
      - DATABASE__TYPE=filesystem
      - STORAGE__TYPE=filesystem
      - BAGET__BASE_URL=http://0.0.0.0:80
    ports:
      - "5000:80"
    volumes:
      - ./storage:/var/baget
EOF

echo "=== Starting BaGet container ==="
cd /home/vagrant/baget_data
# start container in detached mode
sudo docker compose up -d

echo "=== BaGet should be running at http://0.0.0.0:5000 (on VM). ==="
echo "If you need the web UI, open http://192.168.56.10:5000 from host (or use port forwarding)."

# ensure docker container is started
sleep 3
sudo docker ps --filter "name=baget" --format "table {{.Names}}\t{{.Status}}"
