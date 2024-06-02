---
sidebar_position: 2
---

# Права доступа (Разрешения)

Для выполнения каких-либо действий с журналом преподавателю необходимо авторизоваться 
и иметь достаточно прав на это действие.

## Какие права бывают?

Всего у нас 5 разных прав доступа

- **DefaultAccess** - Доступ по умолчанию, дает право на выполнение базовых действий (иными словами обычный преподаватель)
- **SecretaryAccess** - Доступ к функционала секретаря. Его выдают секретарю кафедры, который может выдавать баллы за особые
активности (проставлять баллы за сборную и т.д.)
- **OnlineCourseAccess** - Доступ к выставлению баллов за ЛМС
- **AdminAccess** - Админский доступ к журналу, открывает дополнительные возможности по управлению журналом
- **SuperUser** - Супер пользователь, от админа отличается только тем, что может выдавать другим админские права

## Комбинирование прав

В реализации системы прав было предусмотрена возможность комбинировать права (выдать несколько прав одному человеку).
Хоть на практике это оказалось и не нужным, т.к. в данный момент все эти обязанности четко разделены между разными людьми.

## Реализация

### TeacherPermissions

Это специальный `enum`, в котором перечислены все возможные права, он помечен атрибутом `Flags`, чтобы сделать из энама
битовую маску и позволить хранить там сразу несколько прав, а не 1 значение, как с обычным энамом.

### PermissionsValidator

Класс, в котором инкапсулирована логика по проверке прав доступа.

Он использует встроенный в .NET кэш `MemoryCache`, чтобы кэшировать права преподавателей и не ходить при каждом запросе
за ними в базу. 

Алгоритм при проверке прав:

1. Если необходимые права `DefaultAccess` - вернуть `true`
2. Если у преподавателя есть права `SuperUser` - вернуть `true`
3. Если в необходимых правах есть `SuperUser` - вернуть `false` (потому что из прошлого шага мы уже знаем, что у пользователя их нет)
4. Если у преподавателя есть права `AdminAccess` - вернуть `true` (потому что у админа есть права на все, кроме того, что требует
прав `SuperUser`, а они тут не требуется, что следует из шага `3`)
5. Сделать логическое умножение между требуемыми правами и правами преподавателя, если это значение != 0, то вернуть `true`

### Как работает шаг 5?

В битовой маске права пользователя представлены в виде двоичного числа, например пользователь с правами на ЛМС и секретаря
будет иметь `TeacherPermissions` = `1100`, а если добавить к ним еще права админа, то `1110`. Здесь `DefaultAccess`
это `0000`.

Теперь, для того, чтобы проверить, что у пользователя есть либо права секретаря, либо права админа, то нам нужно 
сделать логическое умножение, для этого мы попарно умножим каждый бит в 2 числах друг на друга. \
`0110` (права пользователя) \
`0110` (необходимые права) \
Получим `0110` - это число `!= 0` поэтому вернем `true`.

Если у пользователя есть только права секретаря то \
`0100` (права пользователя) \
`0110` (необходимые права) \
Получим `0100` - это число `!= 0` поэтому вернем true.

В случае если у пользователя нет ни одного из необходимых прав (допустим есть доступ к ЛМС), то \
`1000` (права пользователя) \
`0110` (необходимые права) \
Получим `0000` - это `== 0` поэтому вернем `false`

:::tip
Проверка прав пользователя проходит только на API уровне
:::