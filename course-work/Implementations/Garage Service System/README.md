
# Garage Management System

**Факултетен номер: 2301321046   
## Описание  
Системата "Garage Management System" е предназначена за управление на клиенти, автомобили и ремонтни дейности в автосервиз. Проектът включва две основни части — уеб услуги (back-end) и клиентски интерфейс (front-end), реализирани с .NET технологии.

## Технологии
 ASP.NET Core Web API (RESTful)
 ASP.NET MVC (клиентски интерфейс)
-Entity Framework Core
-JWT (JSON Web Token) за автентикация

## Структура на проекта
```
/TermProject      - Back-end REST API
/WebClient        - Front-end клиент (.NET MVC)
```



### Back-end (TermProject):
1. Отворете папката `TermProject/TermProject` в Visual Studio.
2. Уверете се, че базата данни `garage.db` съществува (или стартирайте миграциите).

### Front-end (WebClient):
1. Отворете `WebClient/WebClient` в Visual Studio.


## Данни за достъп (примерни)
Можете да създадете потребител чрез формата **Register**, след което да влезете чрез **Login**. След успешен вход ще получите JWT токен, използван за достъп до защитените ресурси.

## Функционалности
- CRUD операции за клиенти, автомобили и ремонти.
- Сортиране и странициране.
- JWT базирана защита на back-end и front-end.
- Валидация чрез `DataAnnotations` и `asp-validation`.
