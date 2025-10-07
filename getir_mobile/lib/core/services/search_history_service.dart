import 'package:shared_preferences/shared_preferences.dart';

class SearchHistoryService {
  static const String _searchHistoryKey = 'search_history';
  static const int _maxHistoryItems = 10;

  final SharedPreferences _prefs;

  SearchHistoryService(this._prefs);

  /// Get search history list
  Future<List<String>> getSearchHistory() async {
    final history = _prefs.getStringList(_searchHistoryKey) ?? [];
    return history;
  }

  /// Add a new search query to history
  Future<void> addSearchQuery(String query) async {
    if (query.trim().isEmpty) return;

    final history = await getSearchHistory();
    
    // Remove if already exists (to move it to top)
    history.remove(query);
    
    // Add to beginning
    history.insert(0, query);
    
    // Keep only last N items
    if (history.length > _maxHistoryItems) {
      history.removeRange(_maxHistoryItems, history.length);
    }
    
    await _prefs.setStringList(_searchHistoryKey, history);
  }

  /// Remove a specific search query from history
  Future<void> removeSearchQuery(String query) async {
    final history = await getSearchHistory();
    history.remove(query);
    await _prefs.setStringList(_searchHistoryKey, history);
  }

  /// Clear all search history
  Future<void> clearSearchHistory() async {
    await _prefs.remove(_searchHistoryKey);
  }

  /// Get popular searches (mock data for now)
  Future<List<String>> getPopularSearches() async {
    // TODO: Bu veriler backend'den gelebilir
    return [
      'Pizza',
      'Burger',
      'Su',
      'Makarna',
      'Süt',
      'Ekmek',
      'Kahve',
      'Çay',
    ];
  }
}

