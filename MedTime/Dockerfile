# ===== STAGE 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY . ./

# Restore packages
RUN dotnet restore "MedTime.csproj"

# Build and publish
RUN dotnet publish "MedTime.csproj" -c Release -o /app/publish

# ===== STAGE 2: Run =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Render thường yêu cầu $PORT)
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "MedTime.dll"]
