# Tellma
For the time being, this document will contain instructions for developers.

## First Time Setup
Follow the steps below to setup the solution for the first time.

### Database Tier
- Make sure SQL Server 2017 (or later) Developer Edition is installed and accessible on "." with Windows auth
- Deploy the Tellma.Database.Admin and BSharp.Database.Identity sql projects in one database `[Tellma]` on the server "."
- Deploy the Tellma.Database.Application sql project in a separate database `[Tellma.101]` on the same server "."
- In the Admin database, seed the following tables (Id values are not important as long as referential integrity is maintained): 
	`SqlDatabases: Id=101, ServerId=1, DatabaseName=(Name of application database), CreatedById=1, ModifiedById=1`
	`GlobalUsers: Id=1, Email='admin@tellma.com'`
	`GlobalUserMemberships: UserId=1, DatabaseId=101`
- Repeat the steps above for the integration tests databases: `[Tellma.IntegrationTests]` and `[Tellma.IntegrationTests.101]`, this time without deploying Tellma.Database.Identity

### Application Tier
- Make sure Tellma project is your startup project
- Change your debug profile from IIS Express to Tellma
- Right click Tellma project -> Manage User Secrets, and paste the following (replacing XXXXX with values from your environment):
```
{
  "Email": {
    "SendGrid": {
      "ApiKey": "XXXXX"
    }
  }
}
```

### Client App
- Install NodeJS (LTS edition) from the [official website](https://nodejs.org/en/)
- Install Angular CLI by running the following in cmd: `npm install -g @angular/cli`
- Install typescript by running the following in cmd: `npm install -g typescript`
- Install node_modules as follows: In the command line go inside "(SolutionDir)/Tellma/ClientApp/" and run: `npm install`
- Inside "(SolutionDir)/Tellma/ClientApp/src/assets/" create a file appsettings.development.json, and fill it with the following:
```
{
    "apiAddress": null,
    "identityAddress": null
}
```

### Integration Tests - Final Steps
- Open (SolutionDir)/Tellma/appsettings.json and change the value of WebClientAccessTokenLifetimeInDays to 3650
- Run the app (as per the below instructions)
- In the Chrome browser open developer tools (by hitting F12 on Windows), and go to Application >- Local Storage -> http://localhost:5001
- Find the value of access_token and copy it
- Right click Tellma.IntegrationTests project -> Manage User Secrets, and paste the following, replacing XXX with the access token:
```
{
  "AccessToken": "XXXXX"
}
```
- Open (SolutionDir)/Tellma/appsettings.json and change the value of WebClientAccessTokenLifetimeInDays back to 3



## Running The App
### To Run The Application
- Make sure you pull the latest version of the code from GitHub
- Make sure the latest version of the database is deployed to `[Tellma.101]`
- Start the app: Debug -> Start without debugging (Make sure Tellma is the startup project)
- Sign in with username: `admin@tellma.com`, default password: `Admin@123`

### To Run The Tests
- Make sure you pull the latest version of the code from GitHub
- Make sure the latest version of the database is deployed to `[Tellma.IntegrationTests.101]`
- Start the app: Debug -> Start without debugging (Make sure Tellma is the startup project)
- Right Click BSharp.IntegrationTests -> Run Tests
