ARG PARENT_VERSION=2.1.9-dotnet10.0
ARG PORT=8085

# Build stage image
FROM defradigital/dotnetcore-development:${PARENT_VERSION} AS build
ARG PARENT_VERSION
LABEL uk.gov.defra.parent-image=defradigital/dotnetcore-development:${PARENT_VERSION}

COPY --chown=dotnet:dotnet . .
RUN dotnet test Backend.Api.Test
RUN dotnet publish Backend.Api -c Release -o /home/dotnet/publish /p:UseAppHost=false

# Final production image
FROM defradigital/dotnetcore:${PARENT_VERSION} AS production
ARG PARENT_VERSION
LABEL uk.gov.defra.parent-image=defradigital/dotnetcore:${PARENT_VERSION}

# Add curl to template.
# CDP PLATFORM HEALTHCHECK REQUIREMENT
USER root
RUN apk add --no-cache curl

# App files stay root-owned so the dotnet user can't modify them. Not under /home/dotnet,
#   which must stay writable: TrustStore.cs imports TRUSTSTORE_* certs into ~/.dotnet.
WORKDIR /app
COPY --from=build /home/dotnet/publish .
USER dotnet

ARG PORT
EXPOSE ${PORT}
ENTRYPOINT ["dotnet", "Backend.Api.dll"]
