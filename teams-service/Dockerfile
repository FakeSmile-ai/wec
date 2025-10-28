# ===== build =====
FROM maven:3.9.8-eclipse-temurin-17 AS build
WORKDIR /app
COPY pom.xml .
RUN mvn -q -DskipTests dependency:go-offline
COPY src ./src
# importante: que ejecute el repackage
RUN mvn -q -DskipTests clean package

# ===== runtime =====
FROM eclipse-temurin:17-jre-alpine
WORKDIR /app
EXPOSE 8082
ENV JAVA_OPTS=""

# copia SOLO el jar reempacado (no el .original)
COPY --from=build /app/target/teams-service-*.jar /app/app.jar
ENTRYPOINT ["sh", "-c", "java $JAVA_OPTS -jar /app/app.jar"]
