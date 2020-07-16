How to build docker image:

docker build -t upmovement/up-service:production .


Create container and run from image **on development machine**:

docker run --restart always -v upd-dev-db:/upd/db/ -p 8043:443 -p 8080:80 --name upd-dev upd


Push created image to Docker Hub:

Make sure you are signed in to your Docker Hub account

`docker push upmovement/up-service:latest`


Updating the Container On The Server

 - Stop and delete the container
   `docker stop upd`
   `docker rm upd`
 - Pull the latest version of the image
   `docker pull upmovement/up-service:latest`
 - Recreate the container