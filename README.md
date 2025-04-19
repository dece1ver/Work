# Электронный журнал

Система для учета работы станков с ЧПУ, операторов и производственных показателей.
Включает в себя 2 приложения взаимодействующих с базой данных MSSQL Server:

- __eLog__ - для рабочего места оператора с ЧПУ (1 экземпляр на 1 станок) - тут заносятся данные операторами.
- __remeLog__ - для технологов/программиров/руководителей (анализ, формирование отчётов и прочее)

## Требования

### Требования к серверу

- Microsoft SQL Server
- SQL Server Management Studio (не обязательно на самом сервере)

### Требования к клиентским приложениям

- .NET 6 Runtime
- Windows 10 или выше

## Установка и настройка

### Настройка базы данных

1. Установите Microsoft SQL Server (достаточно Express версии)

> https://www.microsoft.com/ru-ru/sql-server/sql-server-downloads

2. Установите SQL Server Management Studio

> https://aka.ms/ssmsfullsetup?clcid=0x419

3. Подключитесь к установленному серверу

__Имя сервера__: localhost если SQL Server Management Studio и Microsoft SQL Server установлены на одном компьютере, если нет, то имя компьютера на котором установлен Microsoft SQL Server.

4. Создайте и настройте базу данных в Microsoft SQL Server.
   - разверните сервер щелкнув на `+` слева от названия;
   - ПКМ по `Базы данных`
   - `Создать`
   - указываем имя, остальное можно оставить по-умолчанию.
   - `Ctrl + N`
   - в открывшемся окне вставляем содержимое команд (отредактировав параметры пользователя и БД) ниже
   - жмем `F5`:

Создание пользователя БД:
```sql
DECLARE @UserName NVARCHAR(100) = 'имя_пользователя'
DECLARE @Password NVARCHAR(100) = 'пароль'
DECLARE @DatabaseName NVARCHAR(100) = 'имя_созданной_базы'

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @UserName)
BEGIN
    EXEC sp_addlogin @UserName, @Password;
END

DECLARE @SQL NVARCHAR(MAX) = N'
USE ' + QUOTENAME(@DatabaseName) + N';
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = ''' + @UserName + N''')
BEGIN
    CREATE USER ' + QUOTENAME(@UserName) + N' FOR LOGIN ' + QUOTENAME(@UserName) + N';
    ALTER ROLE db_owner ADD MEMBER ' + QUOTENAME(@UserName) + N';
END'

EXEC sp_executesql @SQL;
```

Разрешение авторизации таким пользователем:
```sql
EXEC xp_instance_regwrite
    N'HKEY_LOCAL_MACHINE',
    N'Software\Microsoft\MSSQLServer\MSSQLServer',
    N'LoginMode',
    REG_DWORD,
    2;
```

Создание таблиц в БД:
```sql
CREATE TABLE cnc_deviation_reasons (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Reason NVARCHAR(MAX) NOT NULL,
    Type NCHAR(9) NULL,
    RequireComment BIT NOT NULL
);

CREATE TABLE cnc_downtime_reasons (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Reason NVARCHAR(50) NOT NULL
);

CREATE TABLE cnc_elog_config (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SearchToolTypes NVARCHAR(50) NULL,
    UpdatePath NVARCHAR(MAX) NULL,
    LogPath NVARCHAR(MAX) NULL
);

CREATE TABLE cnc_machines (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    SetupLimit INT NOT NULL,
    SetupCoefficient FLOAT NOT NULL
);

CREATE TABLE cnc_operators (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Patronymic NVARCHAR(50) NULL,
    Qualification INT NOT NULL,
    IsActive BIT NOT NULL
);

CREATE TABLE cnc_remelog_config (
    max_setup_limit FLOAT NULL,
    long_setup_limit FLOAT NULL
);

CREATE TABLE cnc_serial_parts (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PartName NVARCHAR(255) NOT NULL UNIQUE,
    YearCount INT NOT NULL
);

CREATE TABLE cnc_shifts (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ShiftDate SMALLDATETIME NOT NULL,
    Shift NVARCHAR(4) NOT NULL,
    Machine NVARCHAR(MAX) NOT NULL,
    Master NVARCHAR(MAX) NOT NULL,
    UnspecifiedDowntimes FLOAT NOT NULL,
    DowntimesComment NVARCHAR(MAX) NOT NULL,
    CommonComment NVARCHAR(MAX) NOT NULL,
    IsChecked BIT NOT NULL,
    GiverWorkplaceCleaned BIT NULL,
    GiverFailures BIT NULL,
    GiverExtraneousNoises BIT NULL,
    GiverLiquidLeaks BIT NULL,
    GiverToolBreakage BIT NULL,
    GiverCoolantConcentration FLOAT NULL,
    RecieverWorkplaceCleaned BIT NULL,
    RecieverFailures BIT NULL,
    RecieverExtraneousNoises BIT NULL,
    RecieverLiquidLeaks BIT NULL,
    RecieverToolBreakage BIT NULL,
    RecieverCoolantConcentration FLOAT NULL
);

CREATE TABLE cnc_tool_search_cases (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PartGuid UNIQUEIDENTIFIER NOT NULL,
    ToolType NVARCHAR(50) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    StartTime SMALLDATETIME NOT NULL,
    EndTime SMALLDATETIME NOT NULL
    IsSuccess BIT NULL
);

CREATE TABLE cnc_wnc_cfg (
    Server NVARCHAR(50) NOT NULL,
    [User] NVARCHAR(50) NOT NULL,
    Pass NVARCHAR(50) NOT NULL,
    LocalType NVARCHAR(50) NOT NULL
);

CREATE TABLE masters (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName NVARCHAR(MAX) NOT NULL,
    IsActive BIT NOT NULL
);

CREATE TABLE parts (
    Guid UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Machine NVARCHAR(MAX) NOT NULL,
    Shift NCHAR(4) NOT NULL,
    ShiftDate SMALLDATETIME NOT NULL,
    Operator NVARCHAR(MAX) NOT NULL,
    PartName NVARCHAR(MAX) NOT NULL,
    [Order] NVARCHAR(MAX) NOT NULL,
    Setup INT NOT NULL,
    FinishedCount FLOAT NOT NULL,
    TotalCount INT NOT NULL,
    StartSetupTime SMALLDATETIME NOT NULL,
    StartMachiningTime SMALLDATETIME NOT NULL,
    SetupTimeFact FLOAT NOT NULL,
    EndMachiningTime SMALLDATETIME NOT NULL,
    SetupTimePlan FLOAT NOT NULL,
    SetupTimePlanForReport FLOAT NOT NULL,
    SingleProductionTimePlan FLOAT NOT NULL,
    ProductionTimeFact FLOAT NOT NULL,
    MachiningTime TIME NOT NULL,
    SetupDowntimes FLOAT NOT NULL,
    MachiningDowntimes FLOAT NOT NULL,
    PartialSetupTime FLOAT NOT NULL,
    CreateNcProgramTime FLOAT NOT NULL,
    MaintenanceTime FLOAT NOT NULL,
    ToolSearchingTime FLOAT NOT NULL,
    ToolChangingTime FLOAT NOT NULL,
    MentoringTime FLOAT NOT NULL,
    ContactingDepartmentsTime FLOAT NOT NULL,
    FixtureMakingTime FLOAT NOT NULL,
    HardwareFailureTime FLOAT NOT NULL,
    OperatorComment NVARCHAR(MAX) NOT NULL,
    MasterSetupComment VARCHAR(MAX) NULL,
    MasterMachiningComment VARCHAR(MAX) NULL,
    SpecifiedDowntimesComment VARCHAR(MAX) NULL,
    UnspecifiedDowntimeComment VARCHAR(MAX) NULL,
    MasterComment VARCHAR(MAX) NULL,
    FixedSetupTimePlan FLOAT NULL,
    FixedProductionTimePlan FLOAT NULL,
    EngineerComment VARCHAR(MAX) NULL,
    ExcludeFromReports BIT NULL,
    LongSetupReasonComment NVARCHAR(MAX) NULL,
    LongSetupFixComment NVARCHAR(MAX) NULL,
    LongSetupEngeneerComment NVARCHAR(MAX) NULL
);

CREATE TABLE cnc_serial_parts (
    Id INT IDENTITY(1,1) NOT NULL,
    PartName NVARCHAR(255) NOT NULL,
    CONSTRAINT PK_cnc_serial_parts PRIMARY KEY (Id),
    CONSTRAINT UQ_PartName UNIQUE (PartName)
);
```

### Настройка таблиц
Ниже приведено описание и примеры заполнения некоторых таблиц.  
_Созданные таблицы можно заполнять не только запросами как в примерах ниже, но и с помощью графического интерфейса MSSQL Management Studio, для этого: `Сервер` -> `Базы данных` -> `созданная БД` -> `Таблицы` -> `ПКМ по таблице` -> `Изменить первые 200 строк.`_

#### <ins>cnc_deviation_reasons</ins> (причины невыполнения нормативов)

- Reason: сама причина в текстовом виде
- Type: Setup - только для наладки, Machining - только для изготовления, NULL - для всего
- RequireComment: требуется ли обязательныый сопроводительный комментарий при указании этой причины

Для примера можно заполнить командой ниже:
```sql
INSERT INTO cnc_deviation_reasons (Reason, Type, RequireComment)
VALUES
    (N'', NULL, 0),
    (N'Другое', NULL, 1),
    (N'Отсутствие нормативов', NULL, 0),
    (N'Некорректные нормативы', NULL, 0),
    (N'Неопытный оператор', NULL, 0),
    (N'Работа ученика', NULL, 0),
    (N'Небрежное отношение к работе', NULL, 1),
    (N'Некорректное заполнение', NULL, 0),
    (N'Освоение', N'Setup', 0),
    (N'Особенности изготовления', NULL, 1),
    (N'Штучная/длительная работа', N'Machining', 0),
    (N'Изготовление не по техпроцессу', NULL, 1),
    (N'Изготовление типовой детали', N'Setup', 0),
    (N'Разовое изменение времени из-за проблем с инструментом/оборудованием', N'Machining', 1),
    (N'Несоответствующие заготовки', N'Machining', 0),
    (N'Доработка', NULL, 1);

```

#### <ins>cnc_downtime_reasons</ins> (причины простоев оборудования)
Подразумеваются случаи когда никто из операторов ничего на нем не отмечает.

- Reason: причина

Пример:
```sql
INSERT INTO cnc_downtime_reasons (Reason)
VALUES
    (N''),
    (N'Отсутствие оператора'),
    (N'Ремонт оборудования'),
    (N'Отсутствие электричества'),
    (N'Организационные потери'),
    (N'Другое');
```

#### <ins>cnc_elog_config</ins> (некоторые настройки программы eLog)

- SearchToolTypes: категории того, в поисках чего блуждают операторы
- UpdatePath: путь к директории, куда будет помещаться обновленная программа (не обязательно)
- LogPath: путь к директории, куда будут копироваться логи (не обязательно)

Пример:
```sql
INSERT INTO cnc_elog_config (SearchToolTypes, UpdatePath, LogPath)
VALUES
    (N'Сверло корпусное', N'\\server\eLog\release\update', NULL),
    (N'Сверло твёрдосплавное', NULL, NULL),
    (N'Сверло быстрорежущее (HSS и пр)', NULL, NULL),
    (N'Резец / Оправка (нар.)', NULL, NULL),
    (N'Резец / Оправка (внутр.)', NULL, NULL),
    (N'Пластина', NULL, NULL),
    (N'Фреза', NULL, NULL),
    (N'Станочная оснастка', NULL, NULL),
    (N'Измерительный инструмент', NULL, NULL);
```

#### <ins>cnc_machines</ins> (станки)

- Name: наименование станка
- IsActive: активен ли станок (будет отображаться в приложениях)
- Type: выбор из 4 вариантов:
   - Токарный
   - Токарно-фрезерный
   - Вертикально-фрезерный
   - Горизонтально-фрезерный
- SetupLimit: лимит наладки в минутах до отправки уведомлений (если нет норматива)
- SetupCoefficient: коэффициент превышения норматива для отправки уведомления, н-р. 1.25 значит что при нормативе в 60 минут уведомление будет отправлено при фактической наладке от 75 минут (60 * 1.25).

#### <ins>cnc_operators</ins> (операторы)
Список можно настроить как в MSSQL Management Studio, __так и в приложении remeLog__.

- FirstName: имя
- LastName: фамилия
- Patronymic: отчество (не обязательно)
- Qualification: квалификация (от 0 до 5, где 0 - ученик, а 5 максимальная квалификация)
- IsActive: активен ли оператор (будет отображаться в приложении eLog)

#### <ins>cnc_remelog_config</ins> (некоторые настройки программы remeLog)

- max_setup_limit: макимальный показатель выполнения норматива наладки - всё что выше будет приравниваться к нему
- long_setup_limit: лимит длительности наладки в минутах ~~не помню для чего~~
```sql
INSERT INTO cnc_remelog_config (max_setup_limit, long_setup_limit)
VALUES (1.5, 240);
```

#### <ins>cnc_wnc_cfg</ins> (настройки Windchill)
Если вы ~~вдруг~~ используете Windchill, то можно искать чертежи и модели из интерфейса remeLog, для этого нужны следующие настройки.

- Server: имя сервера Winchill
- User: пользователь Windchill с доступом контекстам
- Pass: пароль
- LocalType: локальный тип данных

#### <ins>masters</ins> (мастера)
- FullName: Фамилия Имя Отчество
- IsActive: активен ли (будет отображаться в приложении remeLog)

#### <ins>cnc_serial_parts</ins> (серийные детали)
- Id: ID
- PartName: Название детали

_Таблицы_ <ins>__parts__</ins>_,_ <ins>__cnc_shifts__</ins> _и_ <ins>__cnc_tool_search_cases__</ins> _наполняются данными автоматически в процессе работы_


### Установка приложений

Загрузите последние версии приложений из раздела [__Releases__](https://github.com/dece1ver/Work/releases)  
Приложения являются портативными и не требуют установки.

### Настройка eLog

1. После запуска приложения выберите в разделе "Параметры" пункт "Параметры".
2. Введите строку подключения, пример:

> Server=имя_сервера;Database=имя_созданной_базы;TrustServerCertificate=True;User Id=имя_пользователя;Password=пароль;  

_P.S. для того чтобы поле стало доступно для ввода нужно тыкнуть 5 раз по тексту "Строка подключения"_

3. После успешной проверки соединения, загрузятся станки и можно будет выбрать из указанных в БД.
4. Заказы: указываем путь к .xlsx файлу в котором должно быть 4 столбца:

| № Заказа | Наименование детали | - | Количество деталей |
|:---------|:--------------------|:-:|-------------------:|
| Заказ 1  | Деталь 1            | - | 100                |
| Заказ 2  | Деталь 2            | - | 200                |
| Заказ 3  | Деталь 3            | - | 300                |


Формат заказа: __ПР-00/11111.2.3__  
__ПР__: перфикс - всегда 2 буквы, список префиксов можно указать в пункте __Типы М/Л__  
__01__: номер месяца от 01 до 12  
__111111__: всегда 5 цифр  
__2__: любое количество цифр  
__3__: любое количество цифр  
_P.S. если суммарное количество цифр 2 и 3 будет превышать 4, то может не влезать в поле ввода_

5. Путь к директории, где будет лежать обновленная версия (не обязательно, делал в основном из-за собственных особенностей разворачивания)

6. ID Google таблицы в которой ведется планирование заданий на станки.  
ID - кусок ссылки на саму таблицу:
> https://docs.google.com/spreadsheets/d/__<u>ID</u>__/...

Структура таблицы:
|||||||||
|:-:|:---------|:---------------:|:---------------:|:-:|:-:|:-------------:|:---------------:|
| __Станок №1__                                                                       ||||||||
| Пустая строка                                                                              |
| - | __Деталь__   | __Заказ__   | __Количетство__ | - | - | __Приоритет__ | __Информация__  |
| - | Деталь 1 | ПР-00/00001.1.1 | 10              | - | - | 1             | <автоматически> |
| - | Деталь 2 | ПР-00/00666.1.2 | 100             | - | - | 5             | <автоматически> |
| - | Деталь 3 | ПР-00/00123.4.5 | 1000            | - | - | 3             | <автоматически> |
| - | Деталь 4 | ПР-00/11111.2.3 | 5               | - | - | 4             | <автоматически> |
| - | Деталь 5 | ПР-00/12345.6.7 | 4               | - | - | 2             | <автоматически> |
| __Станок №2__                                                                       ||||||||
| Пустая строка                                                                              |
| - | __Деталь__   | __Заказ__   | __Количетство__ | - | - | __Приоритет__ | __Информация__  |
| - | Деталь 6 | ПР-00/00009.1.1 | 20              | - | - | 1             | <автоматически> |
| - | Деталь 7 | ПР-00/00666.2.1 | 200             | - | - | 3             | <автоматически> |
| - | Деталь 8 | ПР-00/00321.5.4 | 1               | - | - | 4             | <автоматически> |
| - | Деталь 9 | ПР-00/11111.3.2 | 5               | - | - | 5             | <автоматически> |
| - | Деталь 5 | ПР-00/12345.6.7 | 4               | - | - | 2             | <автоматически> |

__Есть требования к ячейке со станком: ячейка должна содержать наименование станка и символ | после наименования. После символа | допускается любой текст, например:__  
> Mazak QTS200ML | токарно-приводной

__Программа будет сопоставлять станки из БД и искать ячейки с символом | тут, чтобы корректно находить заказы под станок.__  

_P.S. если не указать ID, то просто не будет дополнительного функционала просмотра заданий из Google-таблицы и автоматических отметок о ходе выполнения в ней же_

7. Google Credentials: путь к .JSON файлу с учетными данными сервисного Google аккаунта с которого будет производится взаимодействие с Google таблицей.  
[Подробнее](https://developers.google.com/workspace/guides/create-credentials?hl=ru#service-account)

8. Строка подключения: к этому моменту уже настроена.

9. Типы М/Л: Список префиксов для заказов.  
_Подробнее в п.4._

10. Данные для отправки уведомлений по электронной почте (SMTP).

11. Получатели: путь к файлу с получателями уведомлений по электронной почте.  
Текстовый файл в котором в квадратных скобках указываются категории получателей и в каждой категории перечислены сами получатели, пример:
```ini
[Long Setup]
tech1@example.com
tech2@example.com
master1@example.com
master2@example.com

[Tool Search]
tech1@example.com
tech2@example.com
master1@example.com
master2@example.com
warehouse@example.com

[Process Engineering Department]
tech1@example.com
tech2@example.com

[Production Supervisors]
master1@example.com
master2@example.com
supervisor1@example.com

[Tool Storage]
warehouse@example.com
```
__Long Setup__: получатели которым будет отправляться оперативное уведомление о превышении лимита наладки (при наличии норматива - коэффициент от него, при отсутствии просто назначаемый лимит для станка)

__Tool Search__: будут получать уведомления если у оператора простой по поиску инструмента более 5 минут

__Process Engineering Department__: будут получать сообщения целенаправленно отправляемые оператором группам "Технологи" и "Технологи и руководители цеха"

__Production Supervisors__: будут получать сообщения целенаправленно отправляемые оператором группам "Руководители цеха" и "Технологи и руководители цеха"

__Tool Storage__: будут получать сообщения целенаправленно отправляемые оператором группе "Инструментальный склад"


_P.S. Для корректной работы системы уведомлений на клиентских ПК с установленным приложением eLog необходимо установить системную переменную окружения `NOTIFY_SMTP_PWD`, содержащую пароль для SMTP-сервера._  
_Пример команды в командной строке:_  
```cmd
setx NOTIFY_SMTP_PWD "пароль"
```

12. Передача смены:  
Если включено, то оператор при запуске и завершеии смены будет заполнять небольшой чеклист, который будет видно в приложении remeLog.

13. Запись дополнительных отладочных логов: ну тут понятно.

14. В главном окне открыть "Параметры" -> "Операторы".  
В левом списке будут перечислены все операторы из БД, в правый переносим тех, кто будет работать конкретно на этом рабочем месте.  
_К этому моменту строка подключения должна быть настроена_

В самой программе в пункте "О программе" есть ссылка на ~~слегка~~ устаревшую видеоинструкцию по работе в программе.

### Настройка remeLog

1. После запуска приложения выберите в разделе "Параметры" пункт "Параметры";
2. Введите строку подключения по аналогии п.2 настройки приложения eLog;
3. Разрялы: <ins>не используется</ins>;
4. GS Credentials: путь к .JSON файлу с учетными данными сервисного Google аккаунта аналогично п.7 настройки приложения eLog;
5. ID Google таблицы со списком закрепленной номенклатуры.  
Сама таблица должна иметь структуру:

| Наименование | Станок   | - | - |
|:-------------|:---------|---|---|
| Деталь 1     | Станок 1 |---|---|
| Деталь 2     | Станок 2 |---|---|
| Деталь 3     | Станок 1 |---|---|
| Деталь 4     | Станок 4 |---|---|
6. Роль: влияет на отображение столбцов при подробном просмотре информации по станкам, является _первичным_ видом при открытии окна с подробностями, в самом окне "Подробно" наборы столбцов также переключаются в любой момент.
7. Мгновенное обновление информации на главной странице: если включено, то при изменении дат, закрытии окна "Подробно", информация будет обновляться сама.

В пункте "Дополнительно" -> "Операторы" можно заполнить операторов в БД.  
Это __левый__ список операторов в приложении __eLog__.
