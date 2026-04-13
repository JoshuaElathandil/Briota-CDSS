# Use the official Microsoft .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the csproj and restore any dependencies (via NuGet)
COPY ["Briota.CDSS.csproj", "./"]
RUN dotnet restore

# Copy the rest of the files and build the app
COPY . .
RUN dotnet publish -c Release -o out

# Use the official Microsoft .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the port the app will run on (standard is 8080)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Start the app
ENTRYPOINT ["dotnet", "Briota.CDSS.dll"]
