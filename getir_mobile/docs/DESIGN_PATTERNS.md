# 🎨 Design Patterns Guide

**Tarih:** 2 Kasım 2025  
**Konu:** Getir Mobile - Kullanılan Design Pattern'ler

---

## 📋 İçindekiler

- [Clean Architecture](#clean-architecture)
- [BLoC Pattern](#bloc-pattern)
- [Dependency Injection](#dependency-injection)
- [Repository Pattern](#repository-pattern)
- [Service Layer Pattern](#service-layer-pattern)
- [Error Handling](#error-handling)

---

## 🏗️ Clean Architecture

**Amaç:** Katmanlar arası bağımlılıkları tersine çevirmek

### Katmanlar

```
┌─────────────────────────────────────┐
│   Presentation Layer (UI)           │
│   - Pages, Widgets                  │
│   - BLoCs                           │
└──────────────┬──────────────────────┘
               │ depends on
┌──────────────▼──────────────────────┐
│   Domain Layer (Business Logic)     │
│   - Entities                        │
│   - Services                        │
│   - Repository Interfaces           │
└──────────────┬──────────────────────┘
               │ depends on
┌──────────────▼──────────────────────┐
│   Data Layer (Implementation)       │
│   - Repository Implementations      │
│   - Data Sources                    │
│   - DTOs                            │
└─────────────────────────────────────┘
```

### Prensipler

1. **Dependency Rule:** Dış katmanlar içe bağımlı
2. **Separation of Concerns:** Her katmanın tek sorumluluğu
3. **Independence:** Framework'lerden bağımsız domain logic
4. **Testability:** Her katman izole test edilebilir

---

## 🎭 BLoC Pattern

**Kütüphane:** `flutter_bloc ^8.1.3`

### Akış

```
User Action → Event → BLoC → Use Case → Repository → API
                                         ↓
User Update ← State ← BLoC ← Result ← ────
```

### Örnek

```dart
// Event
class AuthLoginRequested extends AuthEvent {
  final String email;
  final String password;
}

// State
class AuthAuthenticated extends AuthState {
  final UserEntity user;
}

// BLoC
class AuthBloc extends Bloc<AuthEvent, AuthState> {
  final AuthService _authService;
  
  Future<void> _onLoginRequested(
    AuthLoginRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());
    final result = await _authService.login(event.email, event.password);
    
    result.when(
      success: (user) => emit(AuthAuthenticated(user)),
      failure: (error) => emit(AuthError(error.message)),
    );
  }
}
```

### Faydalar

- **Separation:** UI ve business logic ayrı
- **Testability:** BLoCs kolayca test edilebilir
- **Reusability:** Business logic birden fazla widget'ta kullanılabilir
- **Predictability:** State flow tahmin edilebilir

---

## 💉 Dependency Injection

**Kütüphane:** `get_it ^7.6.4`

### Registration

```dart
// lib/core/di/injection.dart
final getIt = GetIt.instance;

Future<void> configureDependencies() async {
  // Core services
  getIt.registerSingleton<LoggerService>(LoggerService());
  getIt.registerSingleton<NetworkService>(NetworkService());
  
  // Repositories
  getIt.registerLazySingleton<IAuthRepository>(
    () => AuthRepositoryImpl(getIt()),
  );
  
  // Services
  getIt.registerLazySingleton<AuthService>(
    () => AuthService(getIt()),
  );
  
  // BLoCs
  getIt.registerFactory<AuthBloc>(
    () => AuthBloc(getIt()),
  );
}
```

### Kullanım

```dart
// Widget'ta
BlocProvider<AuthBloc>(
  create: (_) => getIt<AuthBloc>(),
  child: LoginPage(),
)

// Service'te
final authService = getIt<AuthService>();
```

### Faydalar

- **Testability:** Mock'lar kolayca inject edilebilir
- **Loose Coupling:** Sınıflar birbirine bağımlı değil
- **Singleton Management:** Tek noktadan instance yönetimi

---

## 🗄️ Repository Pattern

**Amaç:** Data access logic'i encapsulate etmek

### Interface (Domain)

```dart
abstract class IAuthRepository {
  Future<Result<UserEntity>> login(String email, String password);
  Future<Result<UserEntity>> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
  });
  Future<void> logout();
  Future<String?> getAccessToken();
}
```

### Implementation (Data)

```dart
class AuthRepositoryImpl implements IAuthRepository {
  final AuthDataSource _dataSource;
  final SecureEncryptionService _encryption;
  
  AuthRepositoryImpl(this._dataSource, this._encryption);
  
  @override
  Future<Result<UserEntity>> login(String email, String password) async {
    try {
      final response = await _dataSource.login(email, password);
      // Token'ları şifreli olarak kaydet
      await _encryption.saveAccessToken(response.accessToken);
      return Result.success(response.user);
    } on NetworkException catch (e) {
      return Result.failure(e);
    }
  }
}
```

### Faydalar

- **Abstraction:** Data source değişikliği domain'den etkilenmez
- **Cache Support:** Caching logic kolayca eklenebilir
- **Testing:** Mock data sources ile test edilebilir

---

## 🔧 Service Layer Pattern

**Amaç:** UseCase pattern yerine Service layer kullanımı

### Örnek

```dart
class AuthService {
  final IAuthRepository _repository;
  
  Future<Result<UserEntity>> login(String email, String password) async {
    // Business logic
    final validationError = _validateCredentials(email, password);
    if (validationError != null) {
      return Result.failure(validationError);
    }
    
    // Delegate to repository
    return await _repository.login(email.trim().toLowerCase(), password);
  }
  
  String? _validateCredentials(String email, String password) {
    if (!isValidEmail(email)) return ValidationException('Invalid email');
    if (password.length < 6) return ValidationException('Password too short');
    return null;
  }
}
```

### UseCase vs Service

| Aspect | UseCase | Service |
|--------|---------|---------|
| **Boilerplate** | Her use case için class | Tek sınıf |
| **Discoverability** | Dağınık | Merkezi |
| **Maintenance** | Çok dosya | Tek dosya |
| **DI Complexity** | 9 dependency | 1 dependency |

---

## 🚨 Error Handling

**Pattern:** Result<T> type

### Result Type

```dart
sealed class Result<T> {
  const Result();
  
  R when<R>({
    required R Function(T data) success,
    required R Function(Exception error) failure,
  });
}

class Success<T> extends Result<T> {
  final T data;
  const Success(this.data);
}

class Failure<T> extends Result<T> {
  final Exception error;
  const Failure(this.error);
}
```

### Kullanım

```dart
final result = await authService.login(email, password);

result.when(
  success: (user) {
    // Handle success
    navigateToHome();
  },
  failure: (error) {
    // Handle error
    showError(error.message);
  },
);
```

### Exception Hierarchy

```
AppException
├── ValidationException
├── NetworkException
│   ├── ConnectivityException
│   └── TimeoutException
├── AuthenticationException
│   ├── UnauthorizedException
│   └── TokenExpiredException
├── NotFoundException
├── ConflictException
└── ServerException
```

---

## 📚 Diger Pattern'ler

### Singleton Pattern

```dart
class LoggerService {
  static final LoggerService _instance = LoggerService._internal();
  factory LoggerService() => _instance;
  LoggerService._internal();
}
```

### Factory Pattern

```dart
class ApiClientFactory {
  static Dio createDio() {
    final dio = Dio();
    // Configure interceptors, timeout, etc.
    return dio;
  }
}
```

### Observer Pattern

```dart
// BLoC: Stream-based state management
class ThemeBloc extends Bloc<ThemeEvent, ThemeState> {
  @override
  Stream<ThemeState> mapEventToState(ThemeEvent event) async* {
    if (event is ToggleTheme) {
      yield event.isDark ? ThemeDark() : ThemeLight();
    }
  }
}
```

---

## ✅ Best Practices

### 1. Interface Segregation

❌ **Kötü:**
```dart
abstract class IRepository {
  Future<User> getUsers();
  Future<Product> getProducts();
  Future<Order> getOrders();
}
```

✅ **İyi:**
```dart
abstract class IUserRepository { Future<User> getUsers(); }
abstract class IProductRepository { Future<Product> getProducts(); }
abstract class IOrderRepository { Future<Order> getOrders(); }
```

### 2. Single Responsibility

❌ **Kötü:**
```dart
class UserService {
  Future<User> getUsers() { }
  Future<void> sendEmail() { }
  Future<void> processPayment() { }
}
```

✅ **İyi:**
```dart
class UserService { Future<User> getUsers() { } }
class EmailService { Future<void> sendEmail() { } }
class PaymentService { Future<void> processPayment() { } }
```

### 3. Dependency Inversion

❌ **Kötü:**
```dart
class AuthBloc {
  final AuthRepositoryImpl repository; // Concrete type
}
```

✅ **İyi:**
```dart
class AuthBloc {
  final IAuthRepository repository; // Interface
}
```

---

**Hazırlayan:** Senior Architect  
**Tarih:** 2 Kasım 2025

