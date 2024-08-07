---
sidebar_position: 1
---

# Структура репозитория

Первое что мы видим в проекте - это структуру самого проекта (как расположены папки, файлы и т.д.)

## Структура проектов

Основу приложения составляют 3 проекта
- [Api](api-project) - Апи проекта
- [Core](core-project) - Здесь содержится вся бизнес-логика и немного инфраструктурной логики (Логирование, конфигурация)
- [DB](db-project) - Модели бд

А также вспомогательные проекты
- [Admin.Api](admin-api-project) - Апи для админки
- [Admin.UI](admin-ui-project) - UI админки
- **Tests** - проект с тестами приложения

## Инфраструктура

Т.к. проект лежит на гитхабе мы активно используем предлагаемые им фичи, такие как GitHub Pages и GitHub Actions.

Здесь можно отметить несколько папок

- **.github/workflows** - yml файлы для GitHub Actions, здесь содержатся все скрипты для CI/CD
- **deploy** - Здесь лежат все конфиги необходимые для работы приложения в проде
- **docs** - Проект с документацией

## Дополнительно

Еще можно заметить в репозитории файл `CodeStyle.ruleset` - это набор правил, которые задают ряд ограничений
на то какой код можно писать, а какой нельзя (например название классов всегда должно быть с большой буквы).
Подробнее про это можно почитать [тут](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)