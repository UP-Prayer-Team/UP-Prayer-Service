Setting Up a New Droplet
========================

1. Create / rebuild droplet with Ubuntu 20.04 (LTS) x64 image

2. Install Docker
    `apt-get update`
    `apt-get install apt-transport-https ca-certificates curl gnupg-agent software-properties-common`
    `curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -`
    `add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"`
    `apt-get update`
    `apt-get install docker-ce docker-ce-cli`
    `docker login`

3. Create new non-root user called `upd` with home directory `/home/upd` in the `docker` group
    `adduser upd`
    `usermod -aG docker upd`

4. Create database volume in Docker
    `docker volume create upd-db`

5. Create container
    `docker run --restart always -d -v upd-db:/upd/db/ -p 443:443 -p 80:80 --name upd upmovement/up-service:latest`