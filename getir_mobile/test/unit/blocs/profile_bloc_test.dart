import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/bloc/profile/profile_bloc.dart';
import 'package:getir_mobile/domain/services/profile_service.dart';
import 'package:getir_mobile/domain/entities/user_profile.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';

@GenerateMocks([ProfileService])
import 'profile_bloc_test.mocks.dart';

void main() {
  late ProfileBloc bloc;
  late MockProfileService mockService;

  setUp(() {
    mockService = MockProfileService();
    bloc = ProfileBloc(mockService);
  });

  const testProfile = UserProfile(
    id: 'user-123',
    email: 'test@test.com',
    firstName: 'John',
    lastName: 'Doe',
    phoneNumber: '+905551234567',
    avatarUrl: 'https://example.com/avatar.jpg',
  );

  group('ProfileBloc -', () {
    test('initial state is ProfileInitial', () {
      expect(bloc.state, ProfileInitial());
    });

    blocTest<ProfileBloc, ProfileState>(
      'LoadProfile emits [ProfileLoading, ProfileLoaded] when profile loaded',
      build: () {
        when(mockService.getUserProfile()).thenAnswer(
          (_) async => Result.success(testProfile),
        );
        return bloc;
      },
      act: (bloc) => bloc.add(LoadProfile()),
      expect: () => [
        ProfileLoading(),
        ProfileLoaded(testProfile),
      ],
    );

    blocTest<ProfileBloc, ProfileState>(
      'LoadProfile emits [ProfileLoading, ProfileError] when loading fails',
      build: () {
        when(mockService.getUserProfile()).thenAnswer(
          (_) async => Result.failure(const NetworkException(message: 'Network error')),
        );
        return bloc;
      },
      act: (bloc) => bloc.add(LoadProfile()),
      expect: () => [
        ProfileLoading(),
        ProfileError('Network error'),
      ],
    );

    blocTest<ProfileBloc, ProfileState>(
      'UpdateProfile emits [ProfileLoaded] with updated profile',
      build: () {
        const updatedProfile = UserProfile(
          id: 'user-123',
          email: 'test@test.com',
          firstName: 'John',
          lastName: 'Smith',
          phoneNumber: '+905551234567',
          avatarUrl: null,
        );
        when(mockService.updateUserProfile(
          firstName: anyNamed('firstName'),
          lastName: anyNamed('lastName'),
          phoneNumber: anyNamed('phoneNumber'),
          avatarUrl: anyNamed('avatarUrl'),
        )).thenAnswer(
          (_) async => Result.success(updatedProfile),
        );
        return bloc;
      },
      act: (bloc) => bloc.add(
        UpdateProfile(
          firstName: 'John',
          lastName: 'Smith',
        ),
      ),
      expect: () => [
        isA<ProfileLoaded>(),
      ],
    );

    blocTest<ProfileBloc, ProfileState>(
      'UpdateProfile emits [ProfileError] when update fails',
      build: () {
        when(mockService.updateUserProfile(
          firstName: anyNamed('firstName'),
          lastName: anyNamed('lastName'),
          phoneNumber: anyNamed('phoneNumber'),
          avatarUrl: anyNamed('avatarUrl'),
        )).thenAnswer(
          (_) async => Result.failure(const ValidationException(message: 'Invalid data')),
        );
        return bloc;
      },
      act: (bloc) => bloc.add(
        UpdateProfile(
          firstName: 'A',
          lastName: 'B',
        ),
      ),
      expect: () => [
        ProfileError('Invalid data'),
      ],
    );
  });
}
