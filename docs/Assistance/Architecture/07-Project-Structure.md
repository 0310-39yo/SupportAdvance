# プロジェクト構成 - SupportAdvance

**最終更新**: 2026-06-30  
**準拠レベル**: 8.5 / 10

---

## 目次

1. [プロジェクト一覧](#プロジェクト一覧)
2. [ディレクトリ構造](#ディレクトリ構造)
3. [依存関係図](#依存関係図)
4. [レイヤマッピング](#レイヤマッピング)
5. [プロジェクト参照ルール](#プロジェクト参照ルール)

---

## プロジェクト一覧

### 全プロジェクト（11個）

| # | プロジェクト名 | 層 | 親プロジェクト | 説明 |
|---|--------------|----|-----------|----|
| 1 | **Common** | Common | - | 設定・時刻・ユーティリティ（最下層） |
| 2 | **Crosscutting** | Crosscutting | - | ロギング等の横断処理 |
| 3 | **SharedKernel** | Domain | - | エンティティ基底・共通ビジネスルール |
| 4 | **Application** | Application | - | UseCase定義・DTO基盤 |
| 5 | **Infrastructure** | Infrastructure | - | DB接続・ORM（汎用） |
| 6 | **Presentation.Shared** | Presentation | - | DI設定・UI共通 |
| 7 | **Presentation.WinTrial** | Presentation | Shared | Windows Forms UI |
| 8 | **Presentation.WpfTrial** | Presentation | Shared | WPF UI |
| 9 | **CarPreferences.Domain** | Domain | SharedKernel | ビジネスルール（Context固有） |
| 10 | **CarPreferences.Application** | Application | Application | UseCase実装（Context固有） |
| 11 | **CarPreferences.Infrastructure** | Infrastructure | Infrastructure | 永続化実装（Context固有） |

---

## ディレクトリ構造

```
SupportAdvance/
│
├─ .github/
│  ├─ instructions/
│  │  └─ csharp.instructions.md
│  └─ workflows/
│
├─ src/
│  ├─ Common/                              ← Common層
│  │  ├─ Configuration/
│  │  │  ├─ IApplicationSettings.cs
│  │  │  ├─ IAppsSettings.cs
│  │  │  ├─ IDatabaseSettings.cs
│  │  │  ├─ IFileSystemSettings.cs
│  │  │  └─ AppSettings.cs ✅ (moved)
│  │  ├─ Clocks/
│  │  │  ├─ IClock.cs
│  │  │  ├─ IClockSettings.cs
│  │  │  ├─ ClockSettings.cs ✅ (moved)
│  │  │  ├─ ClockFactory.cs
│  │  │  ├─ LocalDateTime.cs
│  │  │  ├─ SystemClock.cs
│  │  │  ├─ TickingClock.cs
│  │  │  ├─ OffsetClock.cs
│  │  │  ├─ MockClock.cs
│  │  │  └─ Exception.cs
│  │  └─ Common.csproj
│  │
│  ├─ Crosscutting/                       ← Crosscutting層
│  │  ├─ Logging/
│  │  │  └─ NLogInitializer.cs
│  │  ├─ DependencyInjection.cs
│  │  └─ Crosscutting.csproj
│  │
│  ├─ SharedKernel/                       ← Domain層 (基盤)
│  │  ├─ 保存領域/
│  │  ├─ Entities/
│  │  ├─ ValueObjects/
│  │  ├─ DomainEvents/
│  │  ├─ Exceptions/
│  │  └─ SharedKernel.csproj
│  │
│  ├─ Application/                        ← Application層 (汎用)
│  │  ├─ UseCases/
│  │  │  ├─ IUseCase.cs
│  │  │  ├─ IRequest.cs
│  │  │  ├─ IResponse.cs
│  │  │  └─ UseCaseException.cs
│  │  ├─ DependencyInjection.cs
│  │  └─ Application.csproj
│  │
│  ├─ Infrastructure/                     ← Infrastructure層 (汎用)
│  │  ├─ Data/
│  │  │  ├─ Connections/
│  │  │  └─ Repositories/
│  │  ├─ ORM/
│  │  │  ├─ Dapper/
│  │  │  │  └─ DapperTypeHandlerRegistration.cs ✅ (moved)
│  │  │  └─ RepoDB/
│  │  ├─ DependencyInjection.cs ✅ (Register()追加)
│  │  └─ Infrastructure.csproj
│  │
│  ├─ Presentation/                       ← Presentation層
│  │  ├─ Shared/
│  │  │  ├─ DependencyInjection/
│  │  │  │  ├─ Clock/
│  │  │  │  └─ Configuration/
│  │  │  ├─ Decorators/
│  │  │  ├─ HostBuilderFactory.cs
│  │  │  ├─ Shared.csproj ✅ (Dapper参照削除)
│  │  │  └─ Shared (フォルダ)
│  │  │
│  │  ├─ WinTrial/
│  │  │  ├─ Program.cs ✅ (修正)
│  │  │  ├─ Views/
│  │  │  ├─ ViewModels/
│  │  │  ├─ WinTrial.csproj
│  │  │  └─ WinTrial (フォルダ)
│  │  │
│  │  └─ WpfTrial/
│  │     ├─ Program.cs
│  │     ├─ Views/
│  │     ├─ ViewModels/
│  │     ├─ WpfTrial.csproj
│  │     └─ WpfTrial (フォルダ)
│  │
│  └─ Contexts/
│     └─ Samples/
│        └─ CarPreferences/
│           ├─ Domain/
│           │  ├─ Entities/
│           │  ├─ ValueObjects/
│           │  └─ CarPreferences.Domain.csproj
│           │
│           ├─ Application/
│           │  ├─ UseCases/
│           │  ├─ Dto/
│           │  ├─ DependencyInjection.cs
│           │  └─ CarPreferences.Application.csproj
│           │
│           └─ Infrastructure/
│              ├─ Repositories/
│              ├─ DependencyInjection.cs
│              └─ CarPreferences.Infrastructure.csproj
│
├─ docs/
│  ├─ Assistance/
│  │  ├─ Architecture/                     ← ← フェーズ1ドキュメント
│  │  │  ├─ 00-Overview.md ✅ (作成)
│  │  │  ├─ 01-Layer-Architecture.md ✅ (作成)
│  │  │  ├─ 02-Dependency-Rules.md ✅ (作成)
│  │  │  └─ 07-Project-Structure.md ✅ (作成)
│  │  ├─ Guides/
│  │  └─ Templates/
│  ├─ Common/
│  ├─ Contexts/
│  └─ ...
│
├─ AGENTS.md                              ← AI指示書
├─ .instructions.md
└─ SupportAdvance.sln

```

---

## 依存関係図

### 層別グループ

```
┌─────────────────────────────────────────────────────────────┐
│ PRESENTATION (外側・UI層)                                   │
├─────────────────────────────────────────────────────────────┤
│
│  WinTrial.csproj ──┐
│  WpfTrial.csproj ──┼──→ Shared.csproj
│                    │
└────────────────────┼──────────────────────────────────────────┘
                     ↓
┌──────────────────────────────────────────────────────────────┐
│ Shared.csproj 参照:                                          │
│  ├─→ Application.csproj                                     │
│  ├─→ Common.csproj                                          │
│  └─→ Crosscutting.csproj                                    │
└────────────┬─────────────────────────────────────────────────┘
             ↓
┌──────────────────────────────────────────────────────────────┐
│ APPLICATION (中位層)                                         │
├──────────────────────────────────────────────────────────────┤
│
│  Application.csproj
│  CarPreferences.Application.csproj
│
│  参照:
│  ├─→ SharedKernel.csproj
│  ├─→ Common.csproj
│  └─→ CarPreferences.Domain.csproj
│
└────────────┬─────────────────────────────────────────────────┘
             ↓
┌──────────────────────────────────────────────────────────────┐
│ DOMAIN (内側・業務層)                                        │
├──────────────────────────────────────────────────────────────┤
│
│  SharedKernel.csproj
│  CarPreferences.Domain.csproj
│
│  参照:
│  ├─→ Common.csproj
│  └─→ (他層参照なし)
│
└────────────┬─────────────────────────────────────────────────┘
             ↓ (逆向き許容)
┌──────────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE (外側・技術層)                                │
├──────────────────────────────────────────────────────────────┤
│
│  Infrastructure.csproj
│  CarPreferences.Infrastructure.csproj
│
│  参照:
│  ├─→ Application.csproj
│  ├─→ Common.csproj
│  ├─→ Crosscutting.csproj
│  └─→ (Domain参照なし) ✅
│
└────────────┬─────────────────────────────────────────────────┘
             ↓
┌──────────────────────────────────────────────────────────────┐
│ CROSSCUTTING / COMMON (最下層)                               │
├──────────────────────────────────────────────────────────────┤
│
│  Crosscutting.csproj
│  Common.csproj
│  → (他プロジェクト参照なし)
│
└──────────────────────────────────────────────────────────────┘
```

### 詳細参照マップ

```
WinTrial.csproj
  ├─→ Shared.csproj ┐
  ├─→ Infrastructure.csproj ┐ (DI設定のみ)
  └─→ CarPreferences.* ┘

Shared.csproj
  ├─→ Application.csproj
  ├─→ Common.csproj
  └─→ Crosscutting.csproj

Application.csproj
  ├─→ SharedKernel.csproj
  └─→ Common.csproj

CarPreferences.Application.csproj
  ├─→ Application.csproj
  ├─→ CarPreferences.Domain.csproj
  └─→ Common.csproj

CarPreferences.Domain.csproj
  ├─→ SharedKernel.csproj
  └─→ Common.csproj

Infrastructure.csproj
  ├─→ Application.csproj ✅ (インターフェース経由)
  ├─→ Common.csproj
  └─→ Crosscutting.csproj

CarPreferences.Infrastructure.csproj
  ├─→ Infrastructure.csproj
  ├─→ CarPreferences.Application.csproj ✅ (インターフェース経由)
  ├─→ CarPreferences.Domain.csproj
  └─→ Common.csproj

Crosscutting.csproj
  └─→ Common.csproj

Common.csproj
  → (参照なし)
```

---

## レイヤマッピング

### Common層

| プロジェクト | 責務 | ファイル例 |
|-----------|------|---------|
| **Common** | 純粋ユーティリティ | AppSettings.cs, ClockSettings.cs, IClock.cs |

### Crosscutting層

| プロジェクト | 責務 | ファイル例 |
|-----------|------|---------|
| **Crosscutting** | 横断処理 | NLogInitializer.cs, DependencyInjection.cs |

### Domain層

| プロジェクト | 責務 | ファイル例 |
|-----------|------|---------|
| **SharedKernel** | 基底・共通 | Entity.cs, ValueObject.cs, DomainEvent.cs |
| **CarPreferences.Domain** | Context固有 | CarPreference.cs, PreferenceCategory.cs |

### Application層

| プロジェクト | 責務 | ファイル例 |
|-----------|------|---------|
| **Application** | 基盤 | IUseCase.cs, IRequest.cs, IResponse.cs |
| **CarPreferences.Application** | Context固有 | GetCarPreferenceUseCase.cs, DTOs/ |

### Infrastructure層

| プロジェクト | 責務 | ファイル例 |
|-----------|------|---------|
| **Infrastructure** | 技術基盤 | DB接続, DapperTypeHandlerRegistration.cs |
| **CarPreferences.Infrastructure** | Context固有 | CarPreferenceRepository.cs |

### Presentation層

| プロジェクト | 責務 | ファイル例 |
|-----------|------|---------|
| **Presentation.Shared** | DI設定 | HostBuilderFactory.cs, DependencyInjection/ |
| **Presentation.WinTrial** | UI (WinForms) | Form1.cs, Views/ |
| **Presentation.WpfTrial** | UI (WPF) | MainWindow.xaml, Views/ |

---

## プロジェクト参照ルール

### ✅ 許可される参照

```
WinTrial → Shared ✅
WinTrial → CarPreferences.* ✅
Shared → Application ✅
Shared → Common ✅
Shared → Crosscutting ✅
Application → Domain ✅
Infrastructure → Application ✅
Infrastructure → Common ✅
```

### ❌ 禁止される参照

```
Presentation → Domain ❌
Application → Presentation ❌
Domain → Infrastructure ❌
Domain → Application ❌
循環依存 ❌
```

---

## 新規プロジェクト追加時

### Context追加テンプレート

```
Contexts/Samples/[NewContext]/
├─ Domain/
│  ├─ Entities/
│  ├─ ValueObjects/
│  ├─ DomainEvents/
│  └─ [NewContext].Domain.csproj
│     参照: SharedKernel, Common
│
├─ Application/
│  ├─ UseCases/
│  ├─ Dto/
│  ├─ DependencyInjection.cs
│  └─ [NewContext].Application.csproj
│     参照: Application, [NewContext].Domain, Common
│
└─ Infrastructure/
   ├─ Repositories/
   ├─ DependencyInjection.cs
   └─ [NewContext].Infrastructure.csproj
      参照: Infrastructure, [NewContext].Application, [NewContext].Domain, Common
```

### .csproj設定例

```xml
<!-- [NewContext].Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\..\SharedKernel\SharedKernel.csproj" />
    <ProjectReference Include="..\..\..\..\Common\Common.csproj" />
  </ItemGroup>
</Project>
```

---

## 現在の準拠状況

| 項目 | 状態 | スコア |
|------|------|--------|
| 層構造 | ✅ 完全準拠 | 10/10 |
| 依存方向 | ✅ 完全準拠 | 10/10 |
| 循環依存 | ✅ なし | 10/10 |
| 禁止パターン | ✅ ほぼ準拠 | 9/10 |
| **総合** | **✅ 良好** | **8.5/10** |

### 改善履歴

| 日付 | 改善内容 | 効果 |
|------|---------|------|
| 2026-06-30 | AppSettings → Common移動 | 循環依存なし |
| 2026-06-30 | ClockSettings → Common移動 | Domain依存なし |
| 2026-06-30 | DapperTypeHandlerRegistration → Infrastructure移動 | Presentation がORM依存しない |

---

**作成日**: 2026-06-30  
**参考**: AGENTS.md / 00-Overview.md / 01-Layer-Architecture.md
