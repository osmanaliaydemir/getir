import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';

import 'package:getir_mobile/core/cubits/notification_badge/notification_badge_cubit.dart';
import 'package:getir_mobile/core/cubits/notification_badge/notification_badge_state.dart';

void main() {
  group('NotificationBadgeCubit', () {
    late NotificationBadgeCubit cubit;

    setUp(() {
      cubit = NotificationBadgeCubit();
    });

    tearDown(() {
      cubit.close();
    });

    // ==================== Initial State Tests ====================
    group('Initial State', () {
      test('initial state should have unreadCount of 0', () {
        expect(cubit.state.unreadCount, equals(0));
        expect(cubit.state.hasUnread, isFalse);
      });

      test('initial state should have empty badgeText', () {
        expect(cubit.state.badgeText, equals(''));
      });
    });

    // ==================== updateUnreadCount Tests ====================
    group('updateUnreadCount', () {
      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should update unread count to specific value',
        build: () => cubit,
        act: (cubit) => cubit.updateUnreadCount(5),
        expect: () => [const NotificationBadgeState(unreadCount: 5)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should update from 0 to 10',
        build: () => cubit,
        act: (cubit) => cubit.updateUnreadCount(10),
        expect: () => [const NotificationBadgeState(unreadCount: 10)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should update from 5 to 0',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 5),
        act: (cubit) => cubit.updateUnreadCount(0),
        expect: () => [const NotificationBadgeState(unreadCount: 0)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should not emit state if count is same',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 5),
        act: (cubit) => cubit.updateUnreadCount(5),
        expect: () => [],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle large numbers',
        build: () => cubit,
        act: (cubit) => cubit.updateUnreadCount(999),
        expect: () => [const NotificationBadgeState(unreadCount: 999)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle updating to same value multiple times',
        build: () => cubit,
        act: (cubit) {
          cubit.updateUnreadCount(3);
          cubit.updateUnreadCount(3);
          cubit.updateUnreadCount(3);
        },
        expect: () => [const NotificationBadgeState(unreadCount: 3)],
      );
    });

    // ==================== increment Tests ====================
    group('increment', () {
      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should increment count from 0 to 1',
        build: () => cubit,
        act: (cubit) => cubit.increment(),
        expect: () => [const NotificationBadgeState(unreadCount: 1)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should increment count from 5 to 6',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 5),
        act: (cubit) => cubit.increment(),
        expect: () => [const NotificationBadgeState(unreadCount: 6)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should increment multiple times',
        build: () => cubit,
        act: (cubit) {
          cubit.increment();
          cubit.increment();
          cubit.increment();
        },
        expect: () => [
          const NotificationBadgeState(unreadCount: 1),
          const NotificationBadgeState(unreadCount: 2),
          const NotificationBadgeState(unreadCount: 3),
        ],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle incrementing from large number',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 99),
        act: (cubit) => cubit.increment(),
        expect: () => [const NotificationBadgeState(unreadCount: 100)],
      );
    });

    // ==================== decrement Tests ====================
    group('decrement', () {
      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should decrement count from 5 to 4',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 5),
        act: (cubit) => cubit.decrement(),
        expect: () => [const NotificationBadgeState(unreadCount: 4)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should decrement count from 1 to 0',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 1),
        act: (cubit) => cubit.decrement(),
        expect: () => [const NotificationBadgeState(unreadCount: 0)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should not decrement below 0',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 0),
        act: (cubit) => cubit.decrement(),
        expect: () => [],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should not emit when already at 0',
        build: () => cubit,
        act: (cubit) {
          cubit.decrement();
          cubit.decrement();
          cubit.decrement();
        },
        expect: () => [],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should decrement multiple times correctly',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 3),
        act: (cubit) {
          cubit.decrement();
          cubit.decrement();
          cubit.decrement();
        },
        expect: () => [
          const NotificationBadgeState(unreadCount: 2),
          const NotificationBadgeState(unreadCount: 1),
          const NotificationBadgeState(unreadCount: 0),
        ],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle decrementing to exactly 0',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 1),
        act: (cubit) {
          cubit.decrement();
          cubit.decrement(); // Should not emit again
        },
        expect: () => [const NotificationBadgeState(unreadCount: 0)],
      );
    });

    // ==================== reset Tests ====================
    group('reset', () {
      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should reset count to 0 from 5',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 5),
        act: (cubit) => cubit.reset(),
        expect: () => [const NotificationBadgeState(unreadCount: 0)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should reset count to 0 from 100',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 100),
        act: (cubit) => cubit.reset(),
        expect: () => [const NotificationBadgeState(unreadCount: 0)],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should not emit when already at 0',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 0),
        act: (cubit) => cubit.reset(),
        expect: () => [],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should not emit multiple times when called repeatedly on 0',
        build: () => cubit,
        act: (cubit) {
          cubit.reset();
          cubit.reset();
          cubit.reset();
        },
        expect: () => [],
      );
    });

    // ==================== NotificationBadgeState Tests ====================
    group('NotificationBadgeState', () {
      test('hasUnread should return false when count is 0', () {
        const state = NotificationBadgeState(unreadCount: 0);
        expect(state.hasUnread, isFalse);
      });

      test('hasUnread should return true when count is greater than 0', () {
        const state = NotificationBadgeState(unreadCount: 1);
        expect(state.hasUnread, isTrue);
      });

      test('badgeText should return empty string when count is 0', () {
        const state = NotificationBadgeState(unreadCount: 0);
        expect(state.badgeText, equals(''));
      });

      test('badgeText should return count as string when count is 1-99', () {
        expect(
          const NotificationBadgeState(unreadCount: 1).badgeText,
          equals('1'),
        );
        expect(
          const NotificationBadgeState(unreadCount: 50).badgeText,
          equals('50'),
        );
        expect(
          const NotificationBadgeState(unreadCount: 99).badgeText,
          equals('99'),
        );
      });

      test('badgeText should return "99+" when count is greater than 99', () {
        expect(
          const NotificationBadgeState(unreadCount: 100).badgeText,
          equals('99+'),
        );
        expect(
          const NotificationBadgeState(unreadCount: 150).badgeText,
          equals('99+'),
        );
        expect(
          const NotificationBadgeState(unreadCount: 999).badgeText,
          equals('99+'),
        );
      });

      test('equality should work correctly', () {
        const state1 = NotificationBadgeState(unreadCount: 5);
        const state2 = NotificationBadgeState(unreadCount: 5);
        const state3 = NotificationBadgeState(unreadCount: 10);

        expect(state1, equals(state2));
        expect(state1, isNot(equals(state3)));
      });

      test('props should contain unreadCount', () {
        const state = NotificationBadgeState(unreadCount: 7);
        expect(state.props, contains(7));
        expect(state.props.length, equals(1));
      });

      test('toString should return correct string', () {
        const state = NotificationBadgeState(unreadCount: 5);
        expect(
          state.toString(),
          equals('NotificationBadgeState(unreadCount: 5)'),
        );
      });
    });

    // ==================== Integration Tests ====================
    group('Integration Tests', () {
      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle full workflow: increment, update, decrement, reset',
        build: () => cubit,
        act: (cubit) {
          cubit.increment(); // 0 -> 1
          cubit.increment(); // 1 -> 2
          cubit.updateUnreadCount(10); // 2 -> 10
          cubit.decrement(); // 10 -> 9
          cubit.reset(); // 9 -> 0
        },
        expect: () => [
          const NotificationBadgeState(unreadCount: 1),
          const NotificationBadgeState(unreadCount: 2),
          const NotificationBadgeState(unreadCount: 10),
          const NotificationBadgeState(unreadCount: 9),
          const NotificationBadgeState(unreadCount: 0),
        ],
      );

      test('should maintain correct state through complex operations', () {
        cubit.increment();
        expect(cubit.state.unreadCount, equals(1));

        cubit.increment();
        expect(cubit.state.unreadCount, equals(2));

        cubit.updateUnreadCount(50);
        expect(cubit.state.unreadCount, equals(50));
        expect(cubit.state.badgeText, equals('50'));

        cubit.decrement();
        expect(cubit.state.unreadCount, equals(49));

        cubit.reset();
        expect(cubit.state.unreadCount, equals(0));
        expect(cubit.state.hasUnread, isFalse);
      });
    });

    // ==================== Edge Cases ====================
    group('Edge Cases', () {
      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle rapid increments',
        build: () => cubit,
        act: (cubit) {
          for (int i = 0; i < 10; i++) {
            cubit.increment();
          }
        },
        expect: () => List.generate(
          10,
          (index) => NotificationBadgeState(unreadCount: index + 1),
        ),
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle increment then decrement to same value',
        build: () => cubit,
        seed: () => const NotificationBadgeState(unreadCount: 5),
        act: (cubit) {
          cubit.increment(); // 5 -> 6
          cubit.decrement(); // 6 -> 5
        },
        expect: () => [
          const NotificationBadgeState(unreadCount: 6),
          const NotificationBadgeState(unreadCount: 5),
        ],
      );

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle mixed operations',
        build: () => cubit,
        act: (cubit) {
          cubit.increment(); // 0 -> 1
          cubit.increment(); // 1 -> 2
          cubit.decrement(); // 2 -> 1
          cubit.increment(); // 1 -> 2
          cubit.updateUnreadCount(5); // 2 -> 5
          cubit.decrement(); // 5 -> 4
          cubit.decrement(); // 4 -> 3
        },
        expect: () => [
          const NotificationBadgeState(unreadCount: 1),
          const NotificationBadgeState(unreadCount: 2),
          const NotificationBadgeState(unreadCount: 1),
          const NotificationBadgeState(unreadCount: 2),
          const NotificationBadgeState(unreadCount: 5),
          const NotificationBadgeState(unreadCount: 4),
          const NotificationBadgeState(unreadCount: 3),
        ],
      );

      test('should handle boundary values correctly', () {
        cubit.updateUnreadCount(0);
        expect(cubit.state.unreadCount, equals(0));
        expect(cubit.state.badgeText, equals(''));

        cubit.updateUnreadCount(99);
        expect(cubit.state.unreadCount, equals(99));
        expect(cubit.state.badgeText, equals('99'));

        cubit.updateUnreadCount(100);
        expect(cubit.state.unreadCount, equals(100));
        expect(cubit.state.badgeText, equals('99+'));
      });

      blocTest<NotificationBadgeCubit, NotificationBadgeState>(
        'should handle update to very large number',
        build: () => cubit,
        act: (cubit) => cubit.updateUnreadCount(99999),
        expect: () => [const NotificationBadgeState(unreadCount: 99999)],
        verify: (cubit) {
          expect(cubit.state.badgeText, equals('99+'));
        },
      );
    });
  });
}
