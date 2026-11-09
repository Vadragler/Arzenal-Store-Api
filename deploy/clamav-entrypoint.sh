#!/bin/sh
set -eu

if [ ! -f /var/lib/clamav/main.cvd ] && [ ! -f /var/lib/clamav/main.cld ]; then
    freshclam --config-file=/etc/clamav/freshclam.conf --stdout
fi
freshclam --config-file=/etc/clamav/freshclam.conf --daemon --foreground --stdout &
exec clamd --config-file=/etc/clamav/clamd.conf --foreground
