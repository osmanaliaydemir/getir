import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:getir_mobile/core/services/search_history_service.dart';

@GenerateMocks([SharedPreferences])
import 'search_history_service_test.mocks.dart';

void main() {
  late SearchHistoryService service;
  late MockSharedPreferences mockPrefs;

  setUp(() {
    mockPrefs = MockSharedPreferences();
    service = SearchHistoryService(mockPrefs);
  });

  group('SearchHistoryService - Get Search History', () {
    test('should return empty list when no history exists', () async {
      // Arrange
      when(mockPrefs.getStringList(any)).thenReturn(null);

      // Act
      final history = await service.getSearchHistory();

      // Assert
      expect(history, isEmpty);
    });

    test('should return existing search history', () async {
      // Arrange
      final existingHistory = ['pizza', 'burger', 'sushi'];
      when(mockPrefs.getStringList(any)).thenReturn(existingHistory);

      // Act
      final history = await service.getSearchHistory();

      // Assert
      expect(history, equals(existingHistory));
    });

    test('should return list in correct order', () async {
      // Arrange
      final existingHistory = ['latest', 'middle', 'oldest'];
      when(mockPrefs.getStringList(any)).thenReturn(existingHistory);

      // Act
      final history = await service.getSearchHistory();

      // Assert
      expect(history[0], equals('latest'));
      expect(history[2], equals('oldest'));
    });
  });

  group('SearchHistoryService - Add Search Query', () {
    test('should add new search query to history', () async {
      // Arrange
      when(mockPrefs.getStringList(any)).thenReturn([]);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.addSearchQuery('pizza');

      // Assert
      verify(mockPrefs.setStringList(any, ['pizza'])).called(1);
    });

    test('should add query to beginning of list', () async {
      // Arrange
      when(mockPrefs.getStringList(any)).thenReturn(['burger']);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.addSearchQuery('pizza');

      // Assert
      verify(mockPrefs.setStringList(any, ['pizza', 'burger'])).called(1);
    });

    test('should not add empty or whitespace queries', () async {
      // Arrange
      when(mockPrefs.getStringList(any)).thenReturn([]);

      // Act
      await service.addSearchQuery('   ');

      // Assert
      verifyNever(mockPrefs.setStringList(any, any));
    });

    test('should move existing query to top', () async {
      // Arrange
      when(
        mockPrefs.getStringList(any),
      ).thenReturn(['pizza', 'burger', 'sushi']);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.addSearchQuery('burger');

      // Assert
      verify(
        mockPrefs.setStringList(any, ['burger', 'pizza', 'sushi']),
      ).called(1);
    });

    test('should limit history to 10 items', () async {
      // Arrange
      final fullHistory = List.generate(10, (i) => 'query$i');
      when(mockPrefs.getStringList(any)).thenReturn(fullHistory);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.addSearchQuery('new_query');

      // Assert - Should have max 10 items, oldest should be removed
      final captured = verify(
        mockPrefs.setStringList(any, captureAny),
      ).captured;
      final savedList = captured.last as List<String>;
      expect(savedList.length, equals(10));
      expect(savedList[0], equals('new_query'));
    });
  });

  group('SearchHistoryService - Remove Search Query', () {
    test('should remove specific query from history', () async {
      // Arrange
      when(
        mockPrefs.getStringList(any),
      ).thenReturn(['pizza', 'burger', 'sushi']);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.removeSearchQuery('burger');

      // Assert
      verify(mockPrefs.setStringList(any, ['pizza', 'sushi'])).called(1);
    });

    test('should handle removing non-existent query', () async {
      // Arrange
      when(mockPrefs.getStringList(any)).thenReturn(['pizza', 'burger']);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.removeSearchQuery('sushi');

      // Assert - Should save same list
      verify(mockPrefs.setStringList(any, ['pizza', 'burger'])).called(1);
    });

    test('should handle removing from empty history', () async {
      // Arrange
      when(mockPrefs.getStringList(any)).thenReturn([]);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.removeSearchQuery('pizza');

      // Assert
      verify(mockPrefs.setStringList(any, [])).called(1);
    });
  });

  group('SearchHistoryService - Clear History', () {
    test('should clear all search history', () async {
      // Arrange
      when(mockPrefs.remove(any)).thenAnswer((_) async => true);

      // Act
      await service.clearSearchHistory();

      // Assert
      verify(mockPrefs.remove('search_history')).called(1);
    });

    test('should handle clearing empty history', () async {
      // Arrange
      when(mockPrefs.remove(any)).thenAnswer((_) async => true);

      // Act & Assert
      expect(() => service.clearSearchHistory(), returnsNormally);
    });
  });

  group('SearchHistoryService - Popular Searches', () {
    test('should return popular searches', () async {
      // Act
      final popular = await service.getPopularSearches();

      // Assert
      expect(popular, isNotEmpty);
      expect(popular, isA<List<String>>());
    });

    test('should return consistent popular searches', () async {
      // Act
      final popular1 = await service.getPopularSearches();
      final popular2 = await service.getPopularSearches();

      // Assert
      expect(popular1, equals(popular2));
    });
  });

  group('SearchHistoryService - Edge Cases', () {
    test('should handle special characters in queries', () async {
      // Arrange
      when(mockPrefs.getStringList(any)).thenReturn([]);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.addSearchQuery('pizza & burger!');

      // Assert
      verify(mockPrefs.setStringList(any, ['pizza & burger!'])).called(1);
    });

    test('should handle unicode characters', () async {
      // Arrange
      when(mockPrefs.getStringList(any)).thenReturn([]);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.addSearchQuery('寿司 🍣');

      // Assert
      verify(mockPrefs.setStringList(any, ['寿司 🍣'])).called(1);
    });

    test('should handle very long query strings', () async {
      // Arrange
      final longQuery = 'a' * 1000;
      when(mockPrefs.getStringList(any)).thenReturn([]);
      when(mockPrefs.setStringList(any, any)).thenAnswer((_) async => true);

      // Act
      await service.addSearchQuery(longQuery);

      // Assert
      verify(mockPrefs.setStringList(any, [longQuery])).called(1);
    });
  });
}
