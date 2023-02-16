FROM mongo
RUN apt-get update && apt-get install -y mongodb-clients
ENV PATH="/usr/bin:${PATH}"
COPY init.js /docker-entrypoint-initdb.d/
