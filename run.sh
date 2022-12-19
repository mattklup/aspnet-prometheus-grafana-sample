#!/bin/bash

docker-compose \
  -f ./docker-compose.yml \
  -f ./docker-compose-rabbitmq.yml \
  up --remove-orphans

# -f ./docker-compose-fluentbit.yml \
# docker-compose -f ./docker-compose-fortio.yml up
