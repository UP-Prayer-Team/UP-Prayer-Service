#! /usr/bin/env bash

# Delete the old binaries
ssh root@api.stage.upmovement.org "rm -r /home/upd/bin"

# Upload the binaries
scp -r UPPrayerService/bin/Release/netcoreapp3.0/publish/ root@167.99.162.86:/home/upd/bin/

# Restart the service
ssh root@api.stage.upmovement.org "systemctl restart upd"