# nbomber-webtours-demo

NBomber-версия WebTours — рядом с [gatling-webtours-demo](https://github.com/GeorgeKalyaev/gatling-webtours-demo) и [k6-webtours-demo](https://github.com/GeorgeKalyaev/k6-webtours-demo).

Сервис: `http://127.0.0.1:1080` (Docker из k6-репо или XAMPP).

## Структура

```text
src/NBomber.WebToursDemo/
├── Program.cs
├── Data/                           UserRecord, PaymentData, CityPair
├── Resources/
│   ├── Users.csv, City.csv
│   └── profiles/                   JSON-профили нагрузки
└── NewScripts/
    ├── Debug.cs                    entry point, выбор UC + LoadConfig
    ├── Protocols.cs                baseUrl, headers
    ├── FeederGlobe.cs              имена CSV
    ├── VariablesOfCycles.cs        think time
    └── WebTours/
        ├── HttpSupport.cs
        ├── CsvResource.cs
        ├── UC_01_UcPurchaseTicket/   покупка билета
        │   ├── UcPurchaseTicketAction.cs
        │   ├── UcPurchaseTicketFeeder.cs
        │   ├── UcPurchaseTicketForms.cs
        │   ├── UcPurchaseTicketSessionKeys.cs
        │   └── UcPurchaseTicketScenario.cs
        └── UC_02_UcCancelTicket/     удаление билета
            ├── UcCancelTicketAction.cs
            ├── UcCancelTicketFeeder.cs
            ├── UcCancelTicketForms.cs
            ├── UcCancelTicketSessionKeys.cs
            └── UcCancelTicketScenario.cs
```

Глобальное — в `NewScripts/`, бизнес-логика UC — в своей папке (`Action`, `Feeder`, `Forms`, `Scenario`).

## UC

| | Смысл | NBomber scenario |
|---|-------|------------------|
| UC01 | покупка билета | `BuyWithoutPromocodeAppLight` |
| UC02 | удаление билета | `UC02_Cancel_One_Ticket` |

## Запуск

```powershell
dotnet run --project src/NBomber.WebToursDemo

$env:SCENARIO="cancel"
$env:NBOMBER_CONFIG="smoke.json"
dotnet run --project src/NBomber.WebToursDemo
```

`SCENARIO`: `purchase` (default), `cancel`, `both`. Работают и старые `uc01` / `uc02`.

Профили в `Resources/profiles/`:
- `smoke.json` — 1 копия, 2 мин
- `quick-smoke.json` — быстрая проверка, 30 сек
- `stepped-profile.json` — ступени нагрузки UC01
- `stepped-profile-both.json` — UC01 + UC02

Think time: `THINK_MS_MIN`, `THINK_MS_MAX`. Отчёты пишутся в `reports/` (в git не попадают).

## Стек

.NET 8 · NBomber 6.2 · NBomber.Http 6.2
