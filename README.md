# CDP C# ASP.NET Backend Template

Core delivery C# ASP.NET backend template.

### Install MongoDB
- Install [MongoDB](https://www.mongodb.com/docs/manual/tutorial/#installation) on your local machine
- Start MongoDB:
```bash
sudo mongod --dbpath ~/mongodb-cdp
```

### Inspect MongoDB

To inspect the Database and Collections locally:
```bash
mongosh
```

### Testing

Run the tests with:

Tests run by running a full `WebApplication` backed by [Ephemeral MongoDB](https://github.com/asimmon/ephemeral-mongo).
Tests do not use mocking of any sort and read and write from the in-memory database.

```bash
dotnet test
````

### Running

Run CDP-Deployments application:
```bash
dotnet run --project Backend.Api --launch-profile Development
```
