part of 'profile_bloc.dart';

abstract class ProfileEvent extends Equatable {
  @override
  List<Object?> get props => [];
}

class LoadProfile extends ProfileEvent {}

class UpdateProfile extends ProfileEvent {
  final String firstName;
  final String lastName;
  final String? phoneNumber;
  final String? avatarUrl;

  UpdateProfile({
    required this.firstName,
    required this.lastName,
    this.phoneNumber,
    this.avatarUrl,
  });

  @override
  List<Object?> get props => [firstName, lastName, phoneNumber, avatarUrl];
}
