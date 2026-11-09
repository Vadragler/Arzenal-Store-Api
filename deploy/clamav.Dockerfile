FROM debian:bookworm-slim
ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y --no-install-recommends clamav-daemon clamav-freshclam clamdscan ca-certificates tini \
    && dpkg --compare-versions "$(dpkg-query -W -f='${Version}' clamav-daemon)" ge '1.4.6' \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /var/lib/clamav && chown clamav:clamav /var/lib/clamav
COPY deploy/clamd.conf /etc/clamav/clamd.conf
COPY deploy/freshclam.conf /etc/clamav/freshclam.conf
COPY --chmod=755 deploy/clamav-entrypoint.sh /usr/local/bin/arzenal-clamav
USER clamav
EXPOSE 3310
ENTRYPOINT ["/usr/bin/tini", "-g", "--", "/usr/local/bin/arzenal-clamav"]
