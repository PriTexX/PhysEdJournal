---
sidebar_position: 1
---

# Введение

## Что такое команды?

Команда - это определенная бизнес-фича, код которой инкапсулирован в классе. 
Она принимает определенные параметры, совершает необходимые операции и возвращает [Result\<T\>](https://github.com/PriTexX/presult).

Например: 

Если приложение поддерживает функционал регистрации нового пользователя, то мы создадим команду `RegisterNewUserCommand`

```cs
public class RegisterNewUserPayload
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public class RegisterNewUserResponse
{
    public required string UserId { get; init; }
}

public class RegisterNewUserCommand : ICommand<RegisterNewUserPayload, RegisterNewUserResponse>
{
    public async Task<Result<RegisterNewUserResponse>> ExecuteAsync(RegisterNewUserPayload commandPayload)
    {
        /* all logic here */
        
        return new RegisterNewUserPayload { UserId = userId };
    }
}
```

Данная команда получает на вход необходимые данные для работы - `Payload`, 
выполняет всю логику в методе `ExecuteAsync` и возвращает ответ.

В дальнейшем эта команда будет вызвана в API слое.

## Что такое ICommand\<TPayload, TOutput\>?

Это общий интерфейс для комманд, который помогает их стандартизировать 
(Например чтобы не было такого, что в одной команде вызов логику происходит через метод `DoAsync`, в другой `ExecuteAsync`, 
а в третьей `PerformAsync`)

Он принимает 2 дженерик параметра

- **TPayload** - данные, которые принимает команда
- **TOutput** - данные, которые отдает команда

Интерфейс определяет единственный метод - `public Task<Result<TOutput>> ExecuteAsync(TPayload commandPayload)`

Стоит отметить, что т.к. команды представляют собой бизнес-операции, они могут и будут завершаться с некоторыми ошибками
(В примере с регистрацией нового пользователя ошибками могут быть `UserAlreadyExists`, `WeakPassword` и т.д.). 
Для этого команды возвращают не `TOutput` напрямую, а оборачивают его в [Result\<T\>](https://github.com/PriTexX/presult).

## Result

В проекте для работы с ошибками мы используем [Result pattern](https://mbarkt3sto.hashnode.dev/the-result-design-pattern)
и его реализацию на C# - библиотека [PResult](https://github.com/PriTexX/presult)

## Валидация бизнес-правил для входных данных команд

Обычно для бизнес-логики есть разные ограничения, в случае с `RegisterNewUser` 
это могут быть ограничения на уже существующего пользователя (`UserAlreadyExists`) и на сложность пароля (`WeakPassword`).
Оба этих ограничения должны быть проверены в самой команде, т.к. являются бизнес-правилами.

:::tip
Не путайте Валидаторы API слоя с валидаторами бизнес-правил, они выполняют разные функции.

Валидатор API слоя проверяет входные данные на соответствие ожидаемомому payload, например проверяет,
что переданное число > 0 или что строка соответствует определенному требуемому паттерну, длины строк и т.д.
Важно, что валидатор API слоя никогда не может обращаться к базе данных и другим сервисам.

Валидатор бизнес-правил, напротив, чаще всего будет обращаться к бд, чтобы проверить соответствие нужных правил
:::

Для того, чтобы разделить проверки всех бизнес-правил и непосредственно выполнение команды был создан вспомогательный
интерфейс `ICommandValidator<TPayload>`. Он объявляет единственный метод - 
`public ValueTask<ValidationResult> ValidateCommandInputAsync(TPayload commandInput)`.

Пример для `RegisterNewUserCommand`

```cs
internal class RegisterNewUserValidator 
    : ICommandValidator<RegisterNewUserPayload>
{
    public async ValueTask<ValidationResult> ValidateCommandInputAsync(
        RegisterNewUserPayload commandInput
    )
    {
        /* validation logic here */
        
        /* 
            if validation failed simply
            return an Exception here
            
            return new Exception("Failed");   
         */ 
        
        /* 
            if all good
            
            return ValidationResult.Success;   
         */
    }
}
```

:::tip
Если для валидации нужны какие-либо зависимости, то их можно подтянуть через внедрение зависимостей
:::

:::danger
Валидатор бизнес-правил это часть самой команды, поэтому интерфейс и объявлен как `internal`.
Не нужно пытаться передать его в контейнер зависимостей и получать в конструкторе команды. Валидатор
должен создаваться и управляться самой командой.
:::

## Что возвращают команды

В большинстве наших случаев команды ничего не возвращают. Но т.к. нельзя передать в качестве дженерик параметра
`void`, то используется специальный тип из библиотеки `PResult` - `Unit`. Это тип с единственным значением, 
получить его можно через статик свойство - `Unit.Default`. В том случае, если команда ничего не возвращает, а только
выполняет бизнес-операцию она будет объявлена как `RegisterNewUserCommand : ICommand<RegisterNewUserPayload, Unit>`, 
а вернет она в конце сам юнит - `return Unit.Default;`

## Пустые входные данные команд

Некоторые команды не принимают никаких входных аргументов. В таком случае команда будет наследоваться от интерфейса
`ICommand<EmptyPayload, {output type here}>`, и вызов команды будет таким - `command.ExecuteAsync(EmptyPayload.Empty)`.