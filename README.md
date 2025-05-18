# SeerrFixarr

This is a small project to automatically redownload media that has been reported as broken in Overseerr.

## Deployment

Look at the [docker-compose.exampe.yml](docker-compose.exampe.yml) file for an example of how to deploy this project.


## Configuration

For ease of use, put SeerrFixarr, Overseerr, Sonarr and Radarr in the same docker network.

Overseerr must be configured to use SeerrFixarr as a webhook. This can be done by going to the settings page in Overseerr and adding a new webhook with the following settings:
- **URL**: `http://<your-seerrfixarr-container>:8080/webhook`
- Authorization Header: `Not required`

- JSON Payload:
    ```json
    {
      "issue_id": "{{issue_id}}",
      "reportedBy_username": "{{reportedBy_username}}"
    } 
    ```
- Notification Typed: `[x] Issue Reported`

| Variable Name                    | Description                                                  | Example Value                  | Valid Values                                                                                 |
|----------------------------------|--------------------------------------------------------------|--------------------------------|----------------------------------------------------------------------------------------------|
| `OVERSEERR__APIURL`              | URL to the Overseerr Api                                     | `http://overseerr:5055/api/v1` | Any path                                                                                     |
| `OVERSEERR__APIKEY`              | Api Key of your the Overseerr instance | `YOUR_KEY`                     | Any path                                                                                     |
| `RADARR__APIURL`                 | URL to the Radarr Api                                        | `http://radarr:7878/api/v3`    | Any string                                                                                   |   |
| `RADARR__APIKEY`                 | Api Key of your the Radarr instance                                            | `YOUR_KEY`                     | Any string                                                                                   |   |
| `SONARR__APIURL`                 | URL to the Sonarr Api                                        | `http://sonarr:8989/api/v3`    | Any string                                                                                   |   |
| `SONARR__APIKEY`                 | Api Key of your the Sonarr instance                          | `YOUR_KEY`                     | Any string                                                                                   |
| `SERILOG__MINIMUMLEVEL__Default` | The minimum level of messages to log                         | `Information`                  | [Valid values](https://github.com/serilog/serilog/wiki/configuration-basics#Minimum%20level) |
