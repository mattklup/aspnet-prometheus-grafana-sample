#!/bin/bash

docker-compose \
  -f ./docker-compose.yml \
  -f ./docker-compose-rabbitmq.yml \
  up --remove-orphans

# 
# docker-compose -f ./docker-compose-fortio.yml up
