import 'dart:io';
import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:image_picker/image_picker.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/user_profile.dart';
import '../../../domain/services/profile_service.dart';

part 'profile_event.dart';
part 'profile_state.dart';

class ProfileBloc extends Bloc<ProfileEvent, ProfileState> {
  final ProfileService _profileService;

  ProfileBloc(this._profileService) : super(ProfileInitial()) {
    on<LoadProfile>((event, emit) async {
      emit(ProfileLoading());

      final result = await _profileService.getUserProfile();

      result.when(
        success: (profile) => emit(ProfileLoaded(profile)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(ProfileError(message));
        },
      );
    });

    on<UpdateProfile>((event, emit) async {
      emit(ProfileLoading());

      final result = await _profileService.updateUserProfile(
        firstName: event.firstName,
        lastName: event.lastName,
        phoneNumber: event.phoneNumber,
        avatarUrl: event.avatarUrl,
        avatarImage: event.avatarImage != null
            ? File(event.avatarImage!.path)
            : null,
      );

      result.when(
        success: (updated) => emit(ProfileUpdated(updated)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(ProfileError(message));
        },
      );
    });
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
