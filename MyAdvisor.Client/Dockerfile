FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apt-get update && apt-get install -y nodejs npm && rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY . .
RUN cd MyAdvisor.Client && npm install
RUN dotnet publish MyAdvisor.Client/MyAdvisor.Client.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY MyAdvisor.Client/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
