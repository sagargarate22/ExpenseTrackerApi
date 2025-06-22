# STEP 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy and restore all the packages
COPY *.csproj ./
RUN dotnet restore

# copy all files and publish it
COPY . ./
RUN dotnet publish -c Release -o /app/out


# STEP 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 80

ENTRYPOINT ["dotnet", "ExpenseTrackerApi.dll"]