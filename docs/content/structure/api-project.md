---
sidebar_position: 2
---

# Api

Здесь описывается структура `Api` проекта

## Middlewares

Здесь описаны все мидлвари, которые есть в приложении

### LogUserGuidMiddleware

Добавляет гуид пользователя, который сделал запрос во все логи.

### RequestIdMiddleware

Присвает каждому запросу гуид, по которому можно отфильтровать все логи в рамках 1 запроса.

## PermissionsValidator

Валидатор разрешений преподавателей. Проверяет что у преподавателя, который делает запрос, достаточно прав на совершение этого действия.

## Rest

Тут лежит вся логика работы REST api.

### Common

Общие компоненты для всего API.

#### Filters

Фильтры необходимые для Minimal API.

- **PermissionsFilter** - Проверяет разрешения преподавателя
- **ValidationFilter** - Валидирует входные данные запроса

:::tip
Minimal API не поддерживает валидацию запросов через `Data Annotations`, поэтому мы используем `FluentValidation`
и кастомные валидаторы для каждого запроса.
:::

### ErrorResponse, ErrorHandler

Все ошибки приложения клиенту возвращаются в формате `{ success: false, detail: string, type: string }` \
В проекте есть глобальный маппер ошибок - `ErrorHandler`. Внутри него хранится словарь с типом ошибки к ее 
api ответу. В контроллере же вызывается статик метод `ErrorHandler.HandleErrorResult` куда передается `Exception`,
после чего он возвращает нужный объект ответа, который вернется клиенту.

### Response

Все ответы придерживаются определенного формата. Если запрос завершается успешно, то клиенту возвращается структура
формата `{ success: true, data: T }`

В случае ошибки же вызывается обработчик `ErrorHandler` и возвращается ошибка в виде `{ success: false, detail: string, type: string }`

### OrderByAddons

Добавляет динамическую сортировку для таблиц в БД.

### PaginationParameters

Параметры для пагинации.

## Other

Все остальное - это уже конкретные эндпоинты с их конроллерами, контрактами и ошибками.

### ...Controller

Сам контроллер. Т.к. мы используем `Minimal API`, то наши контроллеры это статик классы, которые определяют приватные
методы (обработчики запросов) и потом через метод `MapEndpoints` проставляют руты для них в инстансе приложения (**app**).

### Contracts

Здесь находятся контракты для каждого запроса. В том числе здесь объявляются мапперы ошибок в их api ответы.

