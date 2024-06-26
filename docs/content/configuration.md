---
sidebar_position: 2
---

# Конфигурация

В проекте используется стандартная конфигурация ASP.Net Core приложений. Она задается через файл `appsettings.json` 
(или `appsettings.Development.json` для дев окружения), а также через переменные окружения.

## ApplicationOptions

В качестве конфига у нас используется класс [ApplicationOptions](https://github.com/PriTexX/PhysEdJournal/blob/main/PhysEdJournal.Infrastructure/ApplicationOptions.cs)
в нем содержатся все поля конфигурации. 

При старте приложения автоматически проверяется наличие всей необходимых (`Required`) полей конфигурации заданных в 
`ApplicationOptions` и если чего-то не хватает, либо указан неверный тип (Требуется `int` передали `string`), то 
выкидывается исключение.

## Использование конфигурации

В месте, где нужно использовать конфиг достаточно передать через внедрение зависимостей интерфейс `IOptions<ApplicationOptions>`
и получить конфиг через `options.Value`.

## Секреты

Не все значения конфига могут быть публичными, например - строка подключения к базе данных, она обязательно должна
храниться в секрете. Мы используем `GitHub Secrets` значения хранящиеся там при деплое будут переданы
в переменные окружения сервера, а оттуда в конфиг самого приложения. Подробнее об этом [тут](other/infrastructure#секреты-приложения)

:::tip
Для локальной разработки можно выставить строку подключения к бд в файле `appsettings.Development.json` 
и не коммитить эти изменения
:::