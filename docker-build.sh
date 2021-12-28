#!/bin/bash

pushd ./web
docker build -t mattklup/aspnetcore-5:local .
popd

pushd ./backend
docker build -t mattklup/aspnetcorebackend-5:local .
popd
