# BSharp
For the time being, this document will contain instructions for developers.

## First Time Setup
Follow the steps below to run the application for the first time.

**Database Tier**
- Deploy the BSharp.Database.Admin and BSharp.Database.Identity sql projects in one database `[BSharp]` on the server "."
- Deploy the BSharp.Database.Application sql project in a separate database `[BSharp.101]` on the same server "."
- In the Admin database, seed the following tables (Id values are not important as long as referential integrity is maintained): 
	`AdminUsers: Id=1, Email='admin@bsharp.online', Name='Administrator', CreatedById=1, ModifiedById=1`
	`SqlServers: Id=1, ServerName='<AdminServer>, UserName='', CreatedById=1, ModifiedById=1`
	`SqlDatabases: Id=101, ServerId=1, DatabaseName=(Name of application database), CreatedById=1, ModifiedById=1`
	`GlobalUsers: Id=1, Email='admin@bsharp.online'`
	`GlobalUserMemberships: UserId=1, DatabaseId=101`
- Repeat the steps above for the integration tests databases: `[BSharp.IntegrationTests]` and `[BSharp.IntegrationTests.101]`, this time without deploying Identity

**Application Tier**
- Right click BSharp project -> Properties -> Debug, un-check "Launch browser" and check "Enable SSL", copy the SSL address into the App URL (making them identical), keep it for the next steps
- Right click BSharp project -> Manage User Secrets, and paste the following (replacing XXXXX with values from your environment):
```
{
  "Email": {
    "SendGrid": {
      "ApiKey": "XXXXX"
    }
  },
  "ApiAuthentication": {
    "AuthorityUri": "https://localhost:XXXXX"
  }
}
```

- Right click BSharp.IntegrationTests project -> Manage User Secrets, and paste the following:
```
{
  "AccessToken": "(To be specified)"
  "ApiAuthentication": {
    "AuthorityUri": "https://localhost:XXXXX"
  }
}
```

**Client App**
- Make sure SQL Server 2017 (or later) Developer Edition is installed and accessible on "." with Windows auth
- Install NodeJS (LTS edition) from the [official website](https://nodejs.org/en/)
- Install Angular CLI by running the following in cmd: `npm install -g @angular/cli`
- Install typescript by running the following in cmd: `npm install -g typescript`
- Install node_modules as follows: In the command line go inside "(SolutionDir)/BSharp/ClientApp/" and run: `npm install`
- Inside "(SolutionDir)/BSharp/ClientApp/src/assets/" create a file appsettings.development.json, and fill it with the following (replacing XXXXX with values from your environment):
```
{
    "apiAddress": "https://localhost:XXXXX/",
    "identityAddress": "https://localhost:XXXXX",
    "identityConfig": {
        "jwks": {
            "keys": [
                {
                    "kty": "RSA",
                    "use": "sig",
                    "kid": "2b8f9fe7747e07c0679d633c88d372c1",
                    "e": "AQAB",
                    "alg": "RS256",
                    "n": "rGVpLbPuUqscSDYG6X0oVfnBnH4oUugnHFMxg8s2xqMnjDZ32luEC67n9nwukknDEq4HBYAfyiGfa8oi0MSsCH1Etj7otaKuqStxU7rf-y-9yKz7RIDCNJ6IWkXMmNIs79CdWAtqtX6RXK0mgG48nmZmbNml7as-CvvKtTSwPDrlwrTtTYff8UIgKpA__zmP52UNAPZKmiXHeiZqM3W75NUzS2qrpRpoBcm1HZH5OiHPI8upOed8IogauiLXh-kY5eTc6b5qg2nBwphkVKZ3I5lJkrsGQkNkvH6pLQmw6O9FgbswM2fHaLKMhLOhPlAgDAVpfYnTF2OKFuswa3WUQQ"
                }
            ]
        },
        "loginUrl": "/connect/authorize",
        "sessionCheckIFrameUrl": "/connect/checksession",
        "logoutUrl": "/connect/endsession",
        "tokenRefreshPeriodInSeconds": 3600
    }
}
```

**Integration Tests - Final Steps**
1 - Open (SolutionDir)/BSharp/appsettings.json and change the value of WebClientAccessTokenLifetimeInDays to 3650
2 - Run the app (as per the below instructions)
3 - In the Chrome browser open developer tools (by hitting F12 on Windows), and go to Application >- Local Storage -> http://localhost:4200
4 - Find the value of access_token and copy it
5 - Right click BSharp.IntegrationTests project -> Manage User Secrets, and paste the value as the AccessToken. 
6 - Open (SolutionDir)/BSharp/appsettings.json and change the value of WebClientAccessTokenLifetimeInDays back to 3



## Running The App
**To Run The Application**
(1) Start the backend server: Debug -> Start without debugging (Make sure BSharp is the startup project)
(2) Start the frontend server: Open the command line inside "(SolutionDir)/BSharp/ClientApp/" and run: `ng serve -o`

**To Run The Tests**
(1) Start the backend server: Debug -> Start without debugging (Make sure BSharp is the startup project)
(2) Right Click BSharp.IntegrationTests => Run Tests
