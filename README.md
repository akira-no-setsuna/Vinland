# Vinland

Минималистичная 2D Top-Down игра на C# (.NET 10) с многопоточной архитектурой.

> [!IMPORTANT]
> **Текущий статус**: Реализовано базовое перемещение игрока. Проект находится в активной разработке.

---

## ⚙️ Стек технологий

- **Ядро**: Godot + .NET 10
- **Физика**: Godot Physics2D
- **Логика**: Stateless (State Machine), Microsoft.Extensions.DependencyInjection, ReGoap
- **Данные**: LiteDB (NoSQL), System.Text.Json (конфиги сущностей)
- **Логирование**: Serilog (Async, File, Thread Enricher)
- **Тесты и бенчмарки**: xUnit, FluentAssertions, Moq, BenchmarkDotNet

---

## 🏗 Архитектура потоков

Проект использует строгое разделение ответственности с обменом данными через `System.Threading.Channels`.

| Поток | Ответственность | Синхронизация |
| :--- | :--- | :--- |
| **🟦 Main** | Рендер, ввод, чтение очередей | Читает `Channel` в начале `Update()` |
| **🟨 Logic** | AI (`Stateless`), бой, спавн, Фиксированный шаг (1/60). Пишет в `Channel` |
| **🟪 Physics** | `Godot Physics2D` работает в выделеном потоке. Результаты позиций → `Channel` |
| **🟫 Data** | Сохранение (`LiteDB`), асинхронное логирование (`Serilog`) | Фоновые `Task` с `CancellationToken` |

---

> [!NOTE]
> Проект кроссплатформенный.  
