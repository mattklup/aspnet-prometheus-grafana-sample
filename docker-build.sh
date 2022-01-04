#!/bin/bash

pushd ./src/web
docker build -t mattklup/aspnetcore-5:local .
popd

pushd ./src/backend
docker build -t mattklup/aspnetcorebackend-5:local .
popd
