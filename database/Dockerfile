FROM postgres:alpine

ENV POSTGRES_USER=jstg
ENV POSTGRES_PASSWORD=password
ENV POSTGRES_DB=jsot

COPY ./scripts/init.sql /docker-entrypoint-initdb.d/

EXPOSE 5432
