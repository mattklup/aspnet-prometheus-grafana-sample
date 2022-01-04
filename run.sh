#!/bin/bash

docker-compose \
  -f ./docker-compose.yml \
  -f ./docker-compose-rabbitmq.yml \
  -f ./docker-compose-fortio.yml \
  up --remove-orphans
