# YouShallNotPass

## Backend

Instructions to run the backend:
1. First, install .Net 6 https://dotnet.microsoft.com/en-us/download
2. Navigate to `/Backend`
3. Run `dotnet publish -r [target architecture]` where target architecture can be `win-x64`, `win-x86`, `osx.12-arm64` (see https://learn.microsoft.com/en-us/dotnet/core/rid-catalog for all target architectures)
4. Run the generated executable in `/Backend/bin/Debug/net6.0/`
5. The root index is a swagger UI that shows all of the APIs and how to use them (see `/Backend/ContentType.cs` for the enum definitions)