# Tiny Admin

Tiny admin is a very simple and lightweight admin page to monitor a specific docker container.

## Features

This project is closer to a POC than anything else, but still usable.

Implemented : 

- Streaming logs to web UI
- Restart container

WIP (not working) : 

- Update container

## Usage

> ⚠ This container calls the docker API, so the container needs to be given privileged access, and the docker socket must be mounted.

> ⚠ There is no security implemented in this software, don't expose it to the internet, and use the level of caution you deem appropriate given that the containers has access to your server's docker socket.

```yaml
version: "3.3"
services:
  tinyadmin:
    image: ombrelin/tiny-admin:latest
    container_name: tinyadmin
    privileged: true
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
    environment:
      - TARGET_CONTAINER=name of the container you want to monitor
```

## Technologies

- C# 11
- .NET 7
- ASP .NET Core 7
- Typescript

Log streaming to the UI is performed asynchronously using Server-Sent Events.
