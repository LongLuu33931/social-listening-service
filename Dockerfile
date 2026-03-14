# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy the entire solution structure
COPY . .

# Restore dependencies
RUN dotnet restore src/Coka.Social.Listening.API/Coka.Social.Listening.API.csproj

# Build the application
RUN dotnet publish src/Coka.Social.Listening.API/Coka.Social.Listening.API.csproj -c Release -o /app/build

# Use the official .NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime

# Cài đặt timezone cho container
RUN apt-get update && \
    apt-get install -y tzdata && \
    ln -fs /usr/share/zoneinfo/Asia/Ho_Chi_Minh /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/build .

# Expose the port and set the entry point
EXPOSE 8080
ENTRYPOINT ["dotnet", "Coka.Social.Listening.API.dll"]
