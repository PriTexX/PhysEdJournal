# PhysEdJournal

Журнал для физры, который предоставляет возможность преподавателям вести учет активности студента в электронном формате, 
а студентам просматривать свои заработанные баллы.

## Начало работы

1. Поставить [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) и [git](https://git-scm.com)
2. Склонировать репозиторий
```bash
git clone https://github.com/PriTexX/PhysEdJournal.git
```
3. Перейти в папку проекта
```bash
cd PhysEdJournal/PhysEdJournal.Api
```
4. Запустить приложение
```bash
dotnet run
```
5. Перейти по адресу http://localhost:5017/graphql

> [!NOTE]
> Для работы приложения требуется указать строку подключения к базе данных. 
> Для локальной разработки это может быть тестовая бд в докере, либо стейджовая версия бд на сервере. 
> Указать строку подключения к бд можно в файле `PhysEdJournal.Api/appSettings.Development.json`,
> в `"ConnectionString": {"DefaultConnection": "{CONNECTION STRING HERE}"}`

## Тесты

В проекте используется фреймворк `xUnit` для написания автотестов. Для запуска всех тестов напишите в корне проекта

```bash
dotnet test
```

> [!NOTE]
> Тесты используют Testcontainers для запуска тестовой базы данных в контейнере. 
> Чтобы они работали нужен запущенный докер, рекомендую сразу установить [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Документация

За более подробной документацией по проекту можно обращаться [сюда](https://pritexx.github.io/PhysEdJournal/)



