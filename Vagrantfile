Vagrant.configure("2") do |config|
  # Базовий образ
  config.vm.box = "ubuntu/focal64"

  # ВМ з приватним IP (підходить для локальної мережі)
  config.vm.network "private_network", ip: "192.168.56.10"

  # Назва для зручності
  config.vm.provider "virtualbox" do |vb|
    vb.name = "baget-vm"
    vb.memory = "2048"
    vb.cpus = 2
  end

  # Provisioning: запускаємо shell-скрипт
  config.vm.provision "shell", path: "Vagrant/provision/setup-baget.sh"
  
end
