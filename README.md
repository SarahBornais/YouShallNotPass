# YouShallNotPass

## Backend
Backend is running on Azure at https://l.messenger.com/l.php?u=https%3A%2F%2Fyoushallnotpassbackend.azurewebsites.net%2F&h=AT1WGoO_nLa8Mv21u3fJzJ0510NY0cCwH6jesSxW5IFSv5C5_EkIUKs65aYXHlkXszfU3--uCqgD6Q5DzA8eOKewtq6Cs9LqnQq3sVDTDsJYxF9S5TSFp3EVs_1js_LxgqJ3oA

To run locally,
1. First, install [.Net 6](https://dotnet.microsoft.com/en-us/download)
2. Navigate to `/Backend/Api/`
3. Define a server key
    1. Run `dotnet user-secrets init`
    2. Go to this [site](https://www.allkeysgenerator.com/Random/Security-Encryption-Key-Generator.aspx) and generate a 128 bit key with hex format (for example, `217A25432A462D4A614E645266556A58`)
    3. dotnet user-secrets set "ServerKey" "[key]"
4. Run `dotnet run`, the url will show in the terminal
5. The root index is a swagger UI that shows all of the APIs and how to use them (see `/Backend/ContentType.cs` for the enum definitions)