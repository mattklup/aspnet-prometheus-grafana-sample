# Basic build and deploy of the app

```bash
$ docker build -t aspnetapp .
$ docker run -it --rm -p 8080:80 --name myapp aspnetapp

$ docker image tag aspnetapp mattklup/aspnetcore-5:1.0
$ docker push mattklup/aspnetcore-5:1.0
```

# Run app with Prometheus/Grafana

```bash
$ docker-compose up
```