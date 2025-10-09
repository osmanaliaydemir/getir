import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/bloc/working_hours/working_hours_bloc.dart';
import 'package:getir_mobile/presentation/bloc/working_hours/working_hours_event.dart';
import 'package:getir_mobile/presentation/bloc/working_hours/working_hours_state.dart';
import 'package:getir_mobile/domain/services/working_hours_service.dart';
import 'package:getir_mobile/domain/entities/working_hours.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:flutter/material.dart';

@GenerateMocks([WorkingHoursService])
import 'working_hours_bloc_test.mocks.dart';

void main() {
  late WorkingHoursBloc bloc;
  late MockWorkingHoursService mockService;

  setUp(() {
    mockService = MockWorkingHoursService();
    bloc = WorkingHoursBloc(mockService);
  });

  final testWorkingHours = WorkingHours(
    id: 'wh-123',
    merchantId: 'merchant-123',
    dayOfWeek: 1,
    openTime: const TimeOfDay(hour: 9, minute: 0),
    closeTime: const TimeOfDay(hour: 22, minute: 0),
    isClosed: false,
    createdAt: DateTime(2024, 1, 1),
  );

  group('WorkingHoursBloc -', () {
    test('initial state is WorkingHoursInitial', () {
      expect(bloc.state, WorkingHoursInitial());
    });

    blocTest<WorkingHoursBloc, WorkingHoursState>(
      'LoadWorkingHours emits [WorkingHoursLoading, WorkingHoursLoaded] when loaded',
      build: () {
        when(
          mockService.getWorkingHours(any),
        ).thenAnswer((_) async => Result.success([testWorkingHours]));
        return bloc;
      },
      act: (bloc) => bloc.add(const LoadWorkingHours('merchant-123')),
      expect: () => [WorkingHoursLoading(), isA<WorkingHoursLoaded>()],
    );

    blocTest<WorkingHoursBloc, WorkingHoursState>(
      'LoadWorkingHours emits [WorkingHoursLoading, WorkingHoursError] when fails',
      build: () {
        when(mockService.getWorkingHours(any)).thenAnswer(
          (_) async =>
              Result.failure(const NetworkException(message: 'Failed')),
        );
        return bloc;
      },
      act: (bloc) => bloc.add(const LoadWorkingHours('merchant-123')),
      expect: () => [WorkingHoursLoading(), isA<WorkingHoursError>()],
    );

    blocTest<WorkingHoursBloc, WorkingHoursState>(
      'LoadWorkingHours emits [WorkingHoursLoading, WorkingHoursNotFound] when empty',
      build: () {
        when(
          mockService.getWorkingHours(any),
        ).thenAnswer((_) async => Result.success([]));
        return bloc;
      },
      act: (bloc) => bloc.add(const LoadWorkingHours('merchant-123')),
      expect: () => [WorkingHoursLoading(), WorkingHoursNotFound()],
    );

    blocTest<WorkingHoursBloc, WorkingHoursState>(
      'CheckMerchantOpen emits [WorkingHoursLoading, MerchantOpenStatusChecked] when checked',
      build: () {
        when(
          mockService.checkIfMerchantOpen(
            any,
            checkTime: anyNamed('checkTime'),
          ),
        ).thenAnswer((_) async => Result.success(true));
        return bloc;
      },
      act: (bloc) => bloc.add(const CheckMerchantOpen('merchant-123')),
      expect: () => [WorkingHoursLoading(), isA<MerchantOpenStatusChecked>()],
    );
  });
}
