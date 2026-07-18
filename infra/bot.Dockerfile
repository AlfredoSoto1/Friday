FROM maven:3.9-eclipse-temurin-21 AS build

WORKDIR /src

ENV MAVEN_OPTS="-Xmx512m -XX:+UseSerialGC"

COPY bot/pom.xml ./
RUN mvn --batch-mode --no-transfer-progress dependency:go-offline

COPY bot/src ./src
RUN mvn --batch-mode --no-transfer-progress -DskipTests package

FROM eclipse-temurin:21-jre-noble AS runtime

WORKDIR /app

ENV JAVA_TOOL_OPTIONS="-Xms64m -Xmx320m -XX:+UseG1GC -XX:MaxGCPauseMillis=100"

COPY --from=build \
  /src/target/friday-bot-0.1.0-SNAPSHOT.jar \
  /app/friday-bot.jar

ENTRYPOINT ["java", "-jar", "/app/friday-bot.jar"]
