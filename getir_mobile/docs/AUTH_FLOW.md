# 🔐 Authentication Flow

**Tarih:** 2 Kasım 2025  
**Konu:** Getir Mobile - Kimlik Doğrulama Akışı

---

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Login Flow](#login-flow)
- [Register Flow](#register-flow)
- [Token Management](#token-management)
- [Security](#security)

---

## 🎯 Genel Bakış

Authentication flow 3 katmanlı Clean Architecture üzerinde çalışır:

```
UI (LoginPage) → BLoC (AuthBloc) → Service (AuthService) → Repository → API
```

---

## 🔑 Login Flow

### 1. UI Layer

**LoginPage** (`lib/presentation/pages/auth/login_page.dart`)

```dart
// User submits form
void _login() {
  if (_formKey.currentState!.validate()) {
    context.read<AuthBloc>().add(
      AuthLoginRequested(
        email: _emailController.text.trim(),
        password: _passwordController.text,
      ),
    );
  }
}
```

### 2. BLoC Layer

**AuthBloc** (`lib/presentation/bloc/auth/auth_bloc.dart`)

```dart
Future<void> _onLoginRequested(
  AuthLoginRequested event,
  Emitter<AuthState> emit,
) async {
  emit(AuthLoading());  // UI'da loading göster
  
  final result = await _authService.login(
    event.email, 
    event.password
  );
  
  result.when(
    success: (user) async {
      // Analytics tracking
      await _analytics.logLogin(method: 'email');
      emit(AuthAuthenticated(user));  // Success state
    },
    failure: (error) {
      emit(AuthError(error.message));  // Error state
    },
  );
}
```

### 3. Service Layer

**AuthService** (`lib/domain/services/auth_service.dart`)

```dart
Future<Result<UserEntity>> login(String email, String password) async {
  // Business rule: Validate
  final validationError = _validateCredentials(email, password);
  if (validationError != null) {
    return Result.failure(validationError);
  }
  
  // Delegate to repository
  return await _repository.login(
    email.trim().toLowerCase(), 
    password
  );
}

String? _validateCredentials(String email, String password) {
  if (!isValidEmail(email)) {
    return ValidationException('Invalid email format');
  }
  if (password.length < 6) {
    return ValidationException('Password must be at least 6 characters');
  }
  return null;
}
```

### 4. Repository Layer

**AuthRepositoryImpl** (`lib/data/repositories/auth_repository_impl.dart`)

```dart
Future<Result<UserEntity>> login(String email, String password) async {
  try {
    // API call
    final response = await _dataSource.login(email, password);
    
    // Save encrypted tokens
    await _encryption.saveAccessToken(response.accessToken);
    await _encryption.saveRefreshToken(response.refreshToken);
    await _storage.saveUser(response.user);
    
    return Result.success(response.user);
  } on NetworkException catch (e) {
    return Result.failure(e);
  }
}
```

### 5. Data Layer

**AuthDataSource** (`lib/data/datasources/auth_datasource_impl.dart`)

```dart
Future<LoginResponse> login(String email, String password) async {
  final response = await _dio.post(
    '/auth/login',
    data: {'email': email, 'password': password},
  );
  return LoginResponse.fromJson(response.data);
}
```

---

## 📊 State Flow Diagram

```
LoginPage
    ↓ [User enters credentials]
AuthBloc.add(AuthLoginRequested)
    ↓
AuthState → AuthLoading (UI shows spinner)
    ↓ [Service call]
AuthService.login()
    ↓ [Validate]
    ↓ [Repository]
AuthRepositoryImpl.login()
    ↓ [Encrypt & Save]
SecureEncryptionService.saveAccessToken()
    ↓ [API Call]
AuthDataSource.login()
    ↓ [Backend responds]
AuthBloc.emit(AuthAuthenticated(user))
    ↓
BlocListener catches state
    ↓
LoginPage → Merge cart → Navigate to Home
```

---

## 🔄 Register Flow

### Steps

1. **Form Validation** (UI)
   - Email format
   - Password strength
   - Name required fields

2. **Service Layer**
   - Phone number validation
   - Duplicate check preparation

3. **Repository**
   - API call
   - Token storage
   - User data persistence

4. **Success**
   - Auto-login
   - Navigate to onboarding/home

---

## 🎫 Token Management

### Storage

```dart
// SecureEncryptionService
await _secureStorage.write(
  key: _accessTokenKey,
  value: encryptedToken,  // AES-256-GCM encrypted
);
```

### Token Refresh

```dart
// Automatic refresh on 401
class TokenRefreshInterceptor extends Interceptor {
  @override
  void onError(DioError err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401) {
      final newToken = await refreshToken();
      if (newToken != null) {
        // Retry original request with new token
        handler.resolve(retryRequest);
      }
    }
  }
}
```

### Auto-login

```dart
// App start
context.read<AuthBloc>().add(AuthCheckAuthenticationRequested());

// Check if valid token exists
if (token != null && !isExpired(token)) {
  emit(AuthAuthenticated(user));
} else {
  emit(AuthUnauthenticated());
}
```

---

## 🔒 Security

### 1. Encryption

**Algorithm:** AES-256-GCM

```dart
// AES-256-GCM encryption
final encrypter = Encrypter(
  AES(key, mode: AESMode.gcm)
);
final encrypted = encrypter.encrypt(plaintext, iv: iv);
```

### 2. Secure Storage

**Android:** `EncryptedSharedPreferences`  
**iOS:** `Keychain (first_unlock)`

### 3. Token Validation

- Expiration check
- Signature verification
- Refresh before expiry

---

## 🎨 UI States

### Loading State

```dart
if (state is AuthLoading) {
  return CircularProgressIndicator();
}
```

### Success State

```dart
if (state is AuthAuthenticated) {
  BlocListener:
    - Merge local cart
    - Navigate to home
    - Track analytics
}
```

### Error State

```dart
if (state is AuthError) {
  ScaffoldMessenger.showSnackBar(
    SnackBar(content: Text(state.message))
  );
}
```

---

**Hazırlayan:** Senior Backend Developer  
**Tarih:** 2 Kasım 2025

