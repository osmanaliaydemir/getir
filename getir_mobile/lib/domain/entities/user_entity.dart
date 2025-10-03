class UserEntity {
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
  
  String get fullName => '$firstName $lastName';
  
  bool get isCustomer => role == 'Customer';
  bool get isMerchant => role == 'Merchant';
  bool get isCourier => role == 'Courier';
  bool get isAdmin => role == 'Admin';
  
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
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    
    return other is UserEntity &&
        other.id == id &&
        other.email == email &&
        other.firstName == firstName &&
        other.lastName == lastName &&
        other.phoneNumber == phoneNumber &&
        other.role == role &&
        other.isEmailVerified == isEmailVerified &&
        other.isActive == isActive &&
        other.createdAt == createdAt &&
        other.updatedAt == updatedAt &&
        other.lastLoginAt == lastLoginAt;
  }
  
  @override
  int get hashCode {
    return id.hashCode ^
        email.hashCode ^
        firstName.hashCode ^
        lastName.hashCode ^
        phoneNumber.hashCode ^
        role.hashCode ^
        isEmailVerified.hashCode ^
        isActive.hashCode ^
        createdAt.hashCode ^
        updatedAt.hashCode ^
        lastLoginAt.hashCode;
  }
  
  @override
  String toString() {
    return 'UserEntity(id: $id, email: $email, firstName: $firstName, lastName: $lastName, phoneNumber: $phoneNumber, role: $role, isEmailVerified: $isEmailVerified, isActive: $isActive, createdAt: $createdAt, updatedAt: $updatedAt, lastLoginAt: $lastLoginAt)';
  }
}
