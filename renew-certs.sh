#! /usr/bin/env bash

#
# WIP script for renewing the api certificates
#

# Staging environment
# Certificate was created with
# `sudo certbot certonly -a manual -d api.stage.upmovement.org --email upprayer.staging@gmail.com`

# Combine the PEM files into a PFX with the password
sudo openssl pkcs12 -inkey /etc/letsencrypt/live/api.stage.upmovement.org/privkey.pem -in /etc/letsencrypt/live/api.stage.upmovement.org/fullchain.pem -export -out UPPrayerService/bin/Debug/netcoreapp3.0/cert.pfx

# Copy the PFX to the staging backend
scp -r UPPrayerService/bin/Debug/netcoreapp3.0/cert.pfx upd@167.99.162.86:/home/upd/bin/cert.pfx