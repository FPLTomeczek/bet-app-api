# apps/api — backend (ASP.NET Core)

Zasady ogólne projektu: patrz `CLAUDE.md` w roocie monorepo.

## Architektura warstw
- **Cienki kontroler** (thin controller) → `DbContext` bezpośrednio jest OK **dla czystego CRUD-u** (brak reguł biznesowych, walidacja to najwyżej „czy FK istnieje").
- **Gdy pojawia się logika biznesowa** (obliczenia, reguły domenowe, koordynacja wielu encji, np. liczenie kursów/wypłaty kuponu, uzgadnianie salda, hashowanie hasła) — wydziel ją do klasy w folderze `Services/` i wstrzykuj do kontrolera przez DI (`AddScoped`). Kontroler zostaje tłumaczem HTTP ↔ domena.
- **Serwis NIE zna HTTP** (żadnego `ModelState`/`ActionResult`). Błędy walidacji zwraca w typie domenowym `Result<T>` (`Services/Result.cs`); kontroler mapuje je na `ValidationProblem`.

- **Nie zakładaj serwisów „na zapas".** Dodawaj je tam, gdzie logika realnie istnieje — nie dla każdej encji z automatu (to zbędny balast). Pierwszy zrealizowany przykład: `CouponService`.

## Stack
- **Baza:** PostgreSQL 17 w Dockerze (docker-compose w roocie, named volume)
- **Backend:** .NET SDK 10.x, ASP.NET Core Web API (kontrolery), EF Core, Npgsql — podejście **code-first**
- **Projektowanie schematu:** draw.io (koncept)
- **Klient DB:** DBeaver
- **Sekrety:** .NET User Secrets

## Uruchomienie
```bash
docker compose up -d              # z roota monorepo
dotnet run --project apps/api/BetApp.Api
dotnet test  apps/api/bet-app.slnx
```

## Kontrakt z frontem
API jest **jedynym źródłem prawdy** dla kształtu danych. Front (`apps/web`) generuje swoje typy TS ze schematu OpenAPI — nie przepisuje ich ręcznie.
Zmieniasz DTO → w tym samym commicie zregeneruj typy frontu. Szczegóły: `CLAUDE.md` w roocie.

### Każda akcja deklaruje swoje odpowiedzi
Generator OpenAPI czyta **wyłącznie statyczne metadane** — sygnaturę i atrybuty. `return NotFound()` w ciele metody jest dla niego niewidzialny, a akcja bez deklaracji trafia do dokumentu jako gołe „200 OK". Efekt: dokument obiecywał 200 tam, gdzie kod zwracał 201 albo 204, i w ogóle nie wspominał o 404/400.

Dlatego **każda akcja ma komplet `[ProducesResponseType]`**:

```csharp
[HttpGet("{id:int}")]
[ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
```

Reguła generyka: **jest wtedy i tylko wtedy, gdy odpowiedź ma ciało.**

| Wynik | Deklaracja |
|---|---|
| `Ok(dto)` | `[ProducesResponseType<XResponse>(StatusCodes.Status200OK)]` |
| `CreatedAtAction(...)` | `[ProducesResponseType<XResponse>(StatusCodes.Status201Created)]` |
| `NoContent()` | `[ProducesResponseType(StatusCodes.Status204NoContent)]` — bez generyka |
| `NotFound()` | `[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]` |
| `ValidationProblem(ModelState)` | `[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]` |

Uwaga na dwie pułapki:
- **404 i 400 nie są puste.** Pod `[ApiController]` wyniki błędów klienckich są mapowane na `ProblemDetails` / `ValidationProblemDetails` i serwowane jako `application/problem+json`. Deklaracja bez generyka byłaby kłamstwem.
- **400 dotyczy każdej akcji z ciałem**, nawet bez ręcznej walidacji — DTO mają DataAnnotations, które `[ApiController]` sprawdza przed wejściem do metody.

Nic nie wymusza zgodności atrybutu z `return`-em — rozjechany atrybut po cichu okłamuje front. Zmieniasz zwracany wynik → popraw atrybut i zregeneruj typy.

**Nie używamy `[ApiConventionType(typeof(DefaultApiConventions))]`** — zgaduje kody z nazwy metody, więc przy nietypowej akcji wpisuje do kontraktu bzdurę bez ostrzeżenia.
