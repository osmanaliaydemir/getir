🟣 MASTER PROMPT — .NET 9 API Base (DB-First, UoW, JWT, SQL Server) + Temel Kalite Eklentileri

ROL: Senior .NET Software Architect & Code Generator
HEDEF: Aşağıdaki gereksinimlerle tek repoda, derlenebilir ve çalışır bir iskelet API üret. Gereksiz açıklama yazma; belirtilen dosyaları tam içerikle ver. Endpoint listesi ekleme.

1) Çerçeve

Runtime: .NET 9 (C# 13)

Mimari: Clean-ish Layered → WebApi, Application, Domain, Infrastructure

Yaklaşım: EF Core 9 DB-First, Generic Repository + Unit of Work, Service Layer

UI: API (Minimal API veya Controller — birini seç ve tutarlı kal), Swagger/OpenAPI

DB: SQL Server (Microsoft.EntityFrameworkCore.SqlServer)

Authentication: JWT (Access + Refresh temel iskelet)

2) Temel Kalite Eklentileri (zorunlu)

Validation (FluentValidation):

Request DTO’ları için validator sınıfları.

Hataları tekil bir problem detay formatında dönen pipeline/middleware.

Logging (Serilog):

Console sink.

Her isteğe X-Request-Id korelasyonu (request id üreten ve log’lara ekleyen middleware).

Health Checks:

/health uç noktası; en azından DB bağlantısı kontrolü.

API Versioning:

v1 şeması; Swagger’da versiyon seçimi.

Pagination/Sorting Standardı:

PagedResult<T> modeli.

Query parametre sözleşmesi: page, pageSize, sortBy, sortDir.

Error Codes:

Result/Result<T> içine ErrorCode alanı ekle (string).

Hata sözlüğü: kısa, stabil kodlar (örn. AUTH_INVALID_CREDENTIALS, ORDER_NOT_FOUND).

3) Klasör Yapısı (tam)
/src
  /WebApi
    Program.cs
    appsettings.json
    appsettings.Development.json
    /Middleware
      ErrorHandlingMiddleware.cs          # JSON hata + ErrorCode
      RequestIdMiddleware.cs              # X-Request-Id üret/korelasyon
    /Configuration
      SwaggerConfig.cs
      AuthConfig.cs
      VersioningConfig.cs
      HealthChecksConfig.cs
  /Application
    /Abstractions
      IGenericRepository.cs
      IReadOnlyRepository.cs
      IUnitOfWork.cs
    /DTO
      AuthDtos.cs
      MerchantDtos.cs
      ProductDtos.cs
      OrderDtos.cs
      PaginationQuery.cs                  # page/pageSize/sortBy/sortDir
    /Validators
      AuthValidators.cs
      MerchantValidators.cs
      ProductValidators.cs
      OrderValidators.cs
    /Services
      Auth/
        IAuthService.cs
        AuthService.cs
      Merchants/
        IMerchantService.cs
        MerchantService.cs
      Products/
        IProductService.cs
        ProductService.cs
      Orders/
        IOrderService.cs
        OrderService.cs
    Result.cs
    ResultExtensions.cs                   # ToIResult(), ProblemDetails eşlemesi
    PagedResult.cs
  /Domain
    /Entities                             # EF Scaffold (DB-First) POCO’lar
      Merchant.cs
      Product.cs
      Order.cs
      OrderLine.cs
  /Infrastructure
    /Persistence
      AppDbContext.cs                     # EF Scaffold (DB-First)
      /Repositories
        GenericRepository.cs
        ReadOnlyRepository.cs
        UnitOfWork.cs
    /Security
      JwtOptions.cs
      JwtTokenService.cs
/.github/workflows/ci.yml                 # (tek job: restore/build)
README.md

4) Paketler (README’de komutları ver)

Microsoft.EntityFrameworkCore.SqlServer

Microsoft.EntityFrameworkCore.Design

Swashbuckle.AspNetCore

FluentValidation.AspNetCore

Serilog.AspNetCore (veya Serilog.Sinks.Console)

Microsoft.AspNetCore.Diagnostics.HealthChecks

Microsoft.AspNetCore.Mvc.Versioning

System.IdentityModel.Tokens.Jwt

Microsoft.AspNetCore.Authentication.JwtBearer

5) EF Core 9 — DB-First Scaffold (README’ye ekle ve konumları belirt)
dotnet ef dbcontext scaffold \
 "Server=localhost,1433;Database=AppDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True" \
 Microsoft.EntityFrameworkCore.SqlServer \
 --context AppDbContext \
 --context-dir src/Infrastructure/Persistence \
 --output-dir src/Domain/Entities \
 --use-database-names \
 --no-pluralize


Özelleştirmeler için partial sınıfları kullan.

6) Abstraction & Altyapı (tam içerik)

IGenericRepository<T>: AddAsync, AddRangeAsync, Update, Remove, GetByIdAsync,
GetPagedAsync(filter, orderBy, page, pageSize, include)

IReadOnlyRepository<T>: AnyAsync, CountAsync, ListAsync(filter, orderBy, include, take) (AsNoTracking)

IUnitOfWork: Repository<T>, ReadRepository<T>, SaveChangesAsync, BeginTransactionAsync, CommitAsync, RollbackAsync

UnitOfWork: IDbContextTransaction yönetimi + ConcurrentDictionary ile repo cache.

7) Service Layer (örnek akışlar)

AuthService: RegisterAsync, LoginAsync, RefreshAsync

Şifre hash’i için .NET yerleşik yöntem (ör. Rfc2898DeriveBytes veya PasswordHasher).

JwtTokenService ile Access+Refresh üretimi.

Merchant/Product/Order Services:

CRUD ve örnek bir transaction boundary (CreateOrder → Begin/Commit).

Tüm servisler Result<T> döndürsün; başarısızlıklarda ErrorCode set edilsin.

8) JWT & Güvenlik

JwtOptions { Issuer, Audience, Secret, AccessTokenMinutes, RefreshTokenMinutes }

JwtTokenService.CreateAccessToken(userId, claims) + CreateRefreshToken()

AddAuthentication().AddJwtBearer(...) ve uygun TokenValidationParameters

appsettings.json içinde Jwt örnek konfig.

Swagger’a Bearer Auth şeması ekle (Authorize butonu ile test edilebilir).

9) Health Checks

AddHealthChecks().AddSqlServer(connectionString)

MapHealthChecks("/health") (JSON dönen minimal yanıt yeterli)

README’de kısa test talimatı.

10) API Versioning

AddApiVersioning (default v1, assume default true)

AddVersionedApiExplorer (Swagger için).

Controller/Minimal API tarafında v1 route/metadata; Swagger’da versiyon sekmesi.

11) Pagination/Sorting Standardı

PaginationQuery { int page=1, int pageSize=20, string? sortBy, string sortDir="desc" }

Servis/Repo’da sıralama için güvenli property eşlemesi (beyaz liste yaklaşımı).

PagedResult<T> { Items, Total, Page, PageSize }

12) Error Codes & Hata İşleme

Result(bool Success, string? Error, string? ErrorCode)

Result<T> aynı şekilde.

ErrorHandlingMiddleware:

Yakalanmamış istisnaları 500 olarak ProblemDetails’la döndür.

Validation hatalarını 400 olarak döndür; ErrorCode map’le.

ResultExtensions.ToIResult() → HTTP kodu + gövde standartlaştırma.

13) Program.cs (yüklenmesi gerekenler)

DbContext: UseSqlServer(...) + EnableSensitiveDataLogging(false)

DI: IUnitOfWork, IGenericRepository<>, IReadOnlyRepository<>, Service’ler

Auth: AddAuthentication(JwtBearerDefaults.AuthenticationScheme) + AddAuthorization()

HealthChecks, Versioning, FluentValidation, Swagger kayıtları

Middleware sırası: RequestIdMiddleware → ErrorHandlingMiddleware → UseHttpsRedirection → UseAuthentication → UseAuthorization → rotalar

Swagger: AddEndpointsApiExplorer + AddSwaggerGen (Bearer şeması + versiyonlar)

14) README — Hızlı Başlangıç

dotnet restore, dotnet build

EF DB-First scaffold komutu

dotnet run

Swagger’da Authorize → Bearer ile test akışı (register → login → token ile korumalı uçlara erişim)

Health check testi: GET /health

15) Kabul Kriterleri

dotnet build hatasız.

SQL Server erişilebilirken dotnet run ile API ayakta, Swagger açılıyor.

JWT akışı (register → login → refresh) örnek servisle çalışıyor.

Health check /health 200 döndürüyor, DB sağlığı işleniyor.

Versiyonlama v1 altında Swagger’da görünüyor.

Validation hataları 400, istisnalar 500 ve ErrorCode içeren JSON ile dönüyor.

Log’larda X-Request-Id korelasyonu mevcut.

16) Çıktı Formatı

Tüm dosyaları kod bloklarıyla, üstünde dosya yolu başlığıyla sırayla üret.

Boş iskelet değil, derlenebilir içerik ver.

Endpoint listesi ekleme.