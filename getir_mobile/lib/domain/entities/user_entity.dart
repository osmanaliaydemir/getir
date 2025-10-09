import 'package:equatable/equatable.dart';

/// Represents a user in the system.
///
/// This is a pure domain entity with no framework dependencies.
/// Supports multiple user roles: customer, merchant, courier, admin.
///
/// Example:
/// ```dart
/// final user = UserEntity(
///   id: 'user-123',
///   email: 'user@getir.com',
///   firstName: 'John',
///   lastName: 'Doe',
///   role: 'customer',
///   // ...
/// );
/// ```
class UserEntity extends Equatable {
  /// Unique identifier for the user
  final String id;

  /// User's email address (used for login)
  final String email;

  /// User's first name
  final String firstName;

  /// User's last name
  final String lastName;

  /// User's phone number (optional, E.164 format recommended)
  final String? phoneNumber;

  /// User role: 'customer', 'merchant', 'courier', or 'admin'
  final String role;

  /// Whether the user's email has been verified
  final bool isEmailVerified;

  /// Whether the user account is active (not suspended/deleted)
  final bool isActive;

  /// When the user account was created
  final DateTime createdAt;

  /// When the user account was last updated
  final DateTime? updatedAt;

  /// When the user last logged in
  final DateTime? lastLoginAt;

  const UserEntity({
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

  /// Returns the user's full name (first name + last name)
  String get fullName => '$firstName $lastName';

  /// Returns true if the user is a customer
  bool get isCustomer => role == 'Customer';

  /// Returns true if the user is a merchant
  bool get isMerchant => role == 'Merchant';

  /// Returns true if the user is a courier
  bool get isCourier => role == 'Courier';

  /// Returns true if the user is an admin
  bool get isAdmin => role == 'Admin';

  /// Creates a copy of this user with the given fields replaced with new values.
  ///
  /// All parameters are optional. If a parameter is not provided, the current
  /// value will be kept.
  UserEntity copyWith({
    String? id,
    String? email,
    String? firstName,
    String? lastName,
    String? phoneNumber,
    String? role,
    bool? isEmailVerified,
    bool? isActive,
    DateTime? createdAt,
    DateTime? updatedAt,
    DateTime? lastLoginAt,
  }) {
    return UserEntity(
      id: id ?? this.id,
      email: email ?? this.email,
      firstName: firstName ?? this.firstName,
      lastName: lastName ?? this.lastName,
      phoneNumber: phoneNumber ?? this.phoneNumber,
      role: role ?? this.role,
      isEmailVerified: isEmailVerified ?? this.isEmailVerified,
      isActive: isActive ?? this.isActive,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
      lastLoginAt: lastLoginAt ?? this.lastLoginAt,
    );
  }

  @override
  List<Object?> get props => [
    id,
    email,
    firstName,
    lastName,
    phoneNumber,
    role,
    isEmailVerified,
    isActive,
    createdAt,
    updatedAt,
    lastLoginAt,
  ];

  @override
  String toString() {
    return 'UserEntity(id: $id, email: $email, firstName: $firstName, lastName: $lastName, phoneNumber: $phoneNumber, role: $role, isEmailVerified: $isEmailVerified, isActive: $isActive, createdAt: $createdAt, updatedAt: $updatedAt, lastLoginAt: $lastLoginAt)';
  }
}
