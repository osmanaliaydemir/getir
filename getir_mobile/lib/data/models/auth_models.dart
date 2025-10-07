import '../../domain/entities/user_entity.dart';

// Login Request Model
class LoginRequest {
  final String email;
  final String password;
  
  const LoginRequest({
    required this.email,
    required this.password,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'email': email,
      'password': password,
    };
  }
  
  factory LoginRequest.fromJson(Map<String, dynamic> json) {
    return LoginRequest(
      email: json['email'] as String,
      password: json['password'] as String,
    );
  }
}

// Register Request Model
class RegisterRequest {
  final String email;
  final String password;
  final String firstName;
  final String lastName;
  final String? phoneNumber;
  
  const RegisterRequest({
    required this.email,
    required this.password,
    required this.firstName,
    required this.lastName,
    this.phoneNumber,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'email': email,
      'password': password,
      'firstName': firstName,
      'lastName': lastName,
      if (phoneNumber != null) 'phoneNumber': phoneNumber,
    };
  }
  
  factory RegisterRequest.fromJson(Map<String, dynamic> json) {
    return RegisterRequest(
      email: json['email'] as String,
      password: json['password'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      phoneNumber: json['phoneNumber'] as String?,
    );
  }
}

// Auth Response Model (Backend'den gelen response'a uygun)
class AuthResponse {
  final String accessToken;
  final String refreshToken;
  final DateTime expiresAt;
  final String role;
  final String userId;
  final String email;
  final String fullName;
  
  const AuthResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresAt,
    required this.role,
    required this.userId,
    required this.email,
    required this.fullName,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'accessToken': accessToken,
      'refreshToken': refreshToken,
      'expiresAt': expiresAt.toIso8601String(),
      'role': role,
      'userId': userId,
      'email': email,
      'fullName': fullName,
    };
  }
  
  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String,
      expiresAt: DateTime.parse(json['expiresAt'] as String),
      role: json['role'] as String,
      userId: json['userId'] as String,
      email: json['email'] as String,
      fullName: json['fullName'] as String,
    );
  }
  
  // UserModel'e çevir (geriye dönük uyumluluk için)
  UserModel toUserModel() {
    final names = fullName.split(' ');
    return UserModel(
      id: userId,
      email: email,
      firstName: names.isNotEmpty ? names[0] : '',
      lastName: names.length > 1 ? names.sublist(1).join(' ') : '',
      phoneNumber: null,
      role: role,
      isEmailVerified: true,
      isActive: true,
      createdAt: DateTime.now(),
      updatedAt: null,
      lastLoginAt: DateTime.now(),
    );
  }
}

// User Model (Data Layer)
class UserModel {
  final String id;
  final String email;
  final String firstName;
  final String lastName;
  final String? phoneNumber;
  final String role;
  final bool isEmailVerified;
  final bool isActive;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final DateTime? lastLoginAt;
  
  const UserModel({
    required this.id,
    required this.email,
    required this.firstName,
    required this.lastName,
    this.phoneNumber,
    required this.role,
    required this.isEmailVerified,
    required this.isActive,
    required this.createdAt,
    this.updatedAt,
    this.lastLoginAt,
  });
  
  String get fullName => '$firstName $lastName';
  
  bool get isCustomer => role == 'Customer';
  bool get isMerchant => role == 'Merchant';
  bool get isCourier => role == 'Courier';
  bool get isAdmin => role == 'Admin';
  
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'email': email,
      'firstName': firstName,
      'lastName': lastName,
      if (phoneNumber != null) 'phoneNumber': phoneNumber,
      'role': role,
      'isEmailVerified': isEmailVerified,
      'isActive': isActive,
      'createdAt': createdAt.toIso8601String(),
      if (updatedAt != null) 'updatedAt': updatedAt!.toIso8601String(),
      if (lastLoginAt != null) 'lastLoginAt': lastLoginAt!.toIso8601String(),
    };
  }
  
  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      id: json['id'] as String,
      email: json['email'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      phoneNumber: json['phoneNumber'] as String?,
      role: json['role'] as String,
      isEmailVerified: json['isEmailVerified'] as bool,
      isActive: json['isActive'] as bool,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: json['updatedAt'] != null 
          ? DateTime.parse(json['updatedAt'] as String) 
          : null,
      lastLoginAt: json['lastLoginAt'] != null 
          ? DateTime.parse(json['lastLoginAt'] as String) 
          : null,
    );
  }
  
  // Convert to Domain Entity
  UserEntity toEntity() {
    return UserEntity(
      id: id,
      email: email,
      firstName: firstName,
      lastName: lastName,
      phoneNumber: phoneNumber,
      role: role,
      isEmailVerified: isEmailVerified,
      isActive: isActive,
      createdAt: createdAt,
      updatedAt: updatedAt,
      lastLoginAt: lastLoginAt,
    );
  }
  
  // Convert from Domain Entity
  factory UserModel.fromEntity(UserEntity entity) {
    return UserModel(
      id: entity.id,
      email: entity.email,
      firstName: entity.firstName,
      lastName: entity.lastName,
      phoneNumber: entity.phoneNumber,
      role: entity.role,
      isEmailVerified: entity.isEmailVerified,
      isActive: entity.isActive,
      createdAt: entity.createdAt,
      updatedAt: entity.updatedAt,
      lastLoginAt: entity.lastLoginAt,
    );
  }
}

// Token Refresh Request Model
class RefreshTokenRequest {
  final String refreshToken;
  
  const RefreshTokenRequest({
    required this.refreshToken,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'refreshToken': refreshToken,
    };
  }
  
  factory RefreshTokenRequest.fromJson(Map<String, dynamic> json) {
    return RefreshTokenRequest(
      refreshToken: json['refreshToken'] as String,
    );
  }
}

// Token Refresh Response Model
class RefreshTokenResponse {
  final String accessToken;
  final String refreshToken;
  final DateTime expiresAt;
  
  const RefreshTokenResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresAt,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'accessToken': accessToken,
      'refreshToken': refreshToken,
      'expiresAt': expiresAt.toIso8601String(),
    };
  }
  
  factory RefreshTokenResponse.fromJson(Map<String, dynamic> json) {
    return RefreshTokenResponse(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String,
      expiresAt: DateTime.parse(json['expiresAt'] as String),
    );
  }
}

// Forgot Password Request Model
class ForgotPasswordRequest {
  final String email;
  
  const ForgotPasswordRequest({
    required this.email,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'email': email,
    };
  }
  
  factory ForgotPasswordRequest.fromJson(Map<String, dynamic> json) {
    return ForgotPasswordRequest(
      email: json['email'] as String,
    );
  }
}

// Reset Password Request Model
class ResetPasswordRequest {
  final String token;
  final String newPassword;
  
  const ResetPasswordRequest({
    required this.token,
    required this.newPassword,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'token': token,
      'newPassword': newPassword,
    };
  }
  
  factory ResetPasswordRequest.fromJson(Map<String, dynamic> json) {
    return ResetPasswordRequest(
      token: json['token'] as String,
      newPassword: json['newPassword'] as String,
    );
  }
}
