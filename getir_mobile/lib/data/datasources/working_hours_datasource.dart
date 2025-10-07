import 'package:dio/dio.dart';
import '../../domain/entities/working_hours.dart';

abstract class WorkingHoursDataSource {
  /// Merchant'ın tüm çalışma saatlerini getirir (7 gün)
  Future<List<WorkingHours>> getWorkingHoursByMerchant(String merchantId);

  /// Merchant'ın şu an açık olup olmadığını kontrol eder
  Future<bool> isMerchantOpen(String merchantId, {DateTime? checkTime});

  /// Belirli bir çalışma saatini getirir
  Future<WorkingHours> getWorkingHoursById(String id);
}

class WorkingHoursDataSourceImpl implements WorkingHoursDataSource {
  final Dio _dio;

  WorkingHoursDataSourceImpl({required Dio dio}) : _dio = dio;

  @override
  Future<List<WorkingHours>> getWorkingHoursByMerchant(
    String merchantId,
  ) async {
    try {
      final response = await _dio.get(
        '/api/v1/WorkingHours/merchant/$merchantId',
      );

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        if (data is List) {
          return data.map((json) => _workingHoursFromJson(json)).toList();
        }
        return [];
      } else {
        throw Exception(
          'Failed to fetch working hours: ${response.statusCode}',
        );
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        // Merchant için çalışma saatleri henüz tanımlanmamış
        return [];
      }
      throw Exception('Failed to fetch working hours: ${e.message}');
    } catch (e) {
      throw Exception('Failed to fetch working hours: $e');
    }
  }

  @override
  Future<bool> isMerchantOpen(String merchantId, {DateTime? checkTime}) async {
    try {
      final queryParams = checkTime != null
          ? {'checkTime': checkTime.toIso8601String()}
          : <String, dynamic>{};

      final response = await _dio.get(
        '/api/v1/WorkingHours/merchant/$merchantId/is-open',
        queryParameters: queryParams,
      );

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        if (data is bool) {
          return data;
        }
        // Backend bazen direkt boolean döner, bazen wrapped
        return data == true || data.toString().toLowerCase() == 'true';
      } else {
        throw Exception(
          'Failed to check merchant status: ${response.statusCode}',
        );
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        // Merchant bulunamadı, kapalı kabul et
        return false;
      }
      throw Exception('Failed to check merchant status: ${e.message}');
    } catch (e) {
      throw Exception('Failed to check merchant status: $e');
    }
  }

  @override
  Future<WorkingHours> getWorkingHoursById(String id) async {
    try {
      final response = await _dio.get('/api/v1/WorkingHours/$id');

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return _workingHoursFromJson(data);
      } else {
        throw Exception(
          'Failed to fetch working hours: ${response.statusCode}',
        );
      }
    } on DioException catch (e) {
      throw Exception('Failed to fetch working hours: ${e.message}');
    } catch (e) {
      throw Exception('Failed to fetch working hours: $e');
    }
  }

  /// JSON'dan WorkingHours entity'sine dönüştürür
  WorkingHours _workingHoursFromJson(Map<String, dynamic> json) {
    return WorkingHours(
      id: json['id']?.toString() ?? '',
      merchantId: json['merchantId']?.toString() ?? '',
      dayOfWeek:
          json['dayOfWeek'] ?? 0, // C# DayOfWeek enum (0=Sunday, 1=Monday, ...)
      openTime: WorkingHoursHelper.parseTimeSpan(json['openTime']),
      closeTime: WorkingHoursHelper.parseTimeSpan(json['closeTime']),
      isClosed: json['isClosed'] ?? false,
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : DateTime.now(),
    );
  }
}
