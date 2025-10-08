import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/user_profile.dart';
import '../../../domain/usecases/profile_usecases.dart';

part 'profile_event.dart';
part 'profile_state.dart';

class ProfileBloc extends Bloc<ProfileEvent, ProfileState> {
  final GetUserProfileUseCase getUserProfileUseCase;
  final UpdateUserProfileUseCase updateUserProfileUseCase;

  ProfileBloc({
    required this.getUserProfileUseCase,
    required this.updateUserProfileUseCase,
  }) : super(ProfileInitial()) {
    on<LoadProfile>((event, emit) async {
      emit(ProfileLoading());

      final result = await getUserProfileUseCase();

      result.when(
        success: (profile) => emit(ProfileLoaded(profile)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(ProfileError(message));
        },
      );
    });

    on<UpdateProfile>((event, emit) async {
      final result = await updateUserProfileUseCase(
        firstName: event.firstName,
        lastName: event.lastName,
        phoneNumber: event.phoneNumber,
        avatarUrl: event.avatarUrl,
      );

      result.when(
        success: (updated) => emit(ProfileLoaded(updated)),
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
