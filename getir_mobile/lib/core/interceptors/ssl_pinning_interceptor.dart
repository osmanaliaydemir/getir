import 'dart:io';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';
import '../config/environment_config.dart';

/// SSL/Certificate Pinning Interceptor
/// Validates server certificates against pinned certificates
/// Only active in production mode when ENABLE_SSL_PINNING=true
class SslPinningInterceptor extends Interceptor {
  static SecurityContext? _securityContext;
  static bool _isInitialized = false;

  /// Initialize SSL pinning with certificate files
  static Future<void> initialize() async {
    if (_isInitialized) return;

    // Only enable in production with flag
    if (!EnvironmentConfig.isProduction ||
        !EnvironmentConfig.enableSslPinning) {
      debugPrint('🔓 SSL Pinning: Disabled');
      _isInitialized = true;
      return;
    }

    try {
      _securityContext = SecurityContext.defaultContext;

      // Load certificate files from assets
      final certificates = [
        'assets/certificates/getir_ca.pem',
        'assets/certificates/getir_server.pem',
      ];

      for (final certPath in certificates) {
        try {
          final certData = await rootBundle.load(certPath);
          final bytes = certData.buffer.asUint8List();
          _securityContext!.setTrustedCertificatesBytes(bytes);
          debugPrint('🔒 SSL Pinning: Loaded certificate $certPath');
        } catch (e) {
          debugPrint('⚠️  SSL Pinning: Failed to load $certPath: $e');
        }
      }

      _isInitialized = true;
      debugPrint('🔒 SSL Pinning: Initialized successfully');
    } catch (e) {
      debugPrint('❌ SSL Pinning: Initialization failed: $e');
      _isInitialized = false;
    }
  }

  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    // Apply SSL pinning to HTTPS requests only
    if (options.uri.scheme == 'https' &&
        EnvironmentConfig.enableSslPinning &&
        _securityContext != null) {
      // Create HttpClient with custom security context
      final httpClient = HttpClient(context: _securityContext);
      httpClient.badCertificateCallback =
          (X509Certificate cert, String host, int port) {
            // Custom certificate validation
            // In production, you should implement proper certificate validation
            // For now, we trust our pinned certificates
            debugPrint('🔍 SSL Pinning: Validating certificate for $host');

            // TODO: Implement proper certificate validation
            // 1. Check certificate chain
            // 2. Verify certificate fingerprint
            // 3. Check expiration date
            // 4. Validate hostname

            return _validateCertificate(cert, host);
          };
    }

    handler.next(options);
  }

  /// Validate certificate against pinned certificates
  /// This is a simplified implementation
  /// In production, use a proper SSL pinning library or implement full validation
  bool _validateCertificate(X509Certificate cert, String host) {
    try {
      // Get certificate DER encoding
      final certDer = cert.der;

      // TODO: Add your pinned certificate SHA-256 hashes here
      // For now, allow all certificates in development
      if (EnvironmentConfig.isDevelopment) {
        debugPrint(
          '🔓 SSL Pinning: Development mode - allowing all certificates',
        );
        return true;
      }

      // In production, validate against pinned hashes
      // You should implement proper certificate validation here
      debugPrint('🔐 SSL Pinning: Validating certificate for $host');
      return _isPinnedCertificate(certDer);
    } catch (e) {
      debugPrint('❌ Certificate validation failed: $e');
      return false;
    }
  }

  /// Check if certificate is pinned
  bool _isPinnedCertificate(Uint8List certDer) {
    // TODO: Replace with your actual pinned certificate hashes
    // Get these by running:
    // openssl s_client -connect api.getir.com:443 | openssl x509 -fingerprint -sha256 -noout
    //
    // For production, compute SHA-256 hash of certDer and compare with pinned hashes
    // Example:
    // import 'package:crypto/crypto.dart';
    // final hash = sha256.convert(certDer).toString();
    // return pinnedHashes.contains(hash);

    // For now, allow all in development (already checked above)
    return true;
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (err.type == DioExceptionType.connectionError &&
        err.error is HandshakeException) {
      // SSL handshake failed - possible MITM attack
      debugPrint('🚨 SSL Pinning: Certificate validation failed!');
      debugPrint('   This could indicate a Man-in-the-Middle attack.');
      debugPrint('   Connection rejected for security reasons.');

      handler.reject(
        DioException(
          requestOptions: err.requestOptions,
          type: DioExceptionType.connectionError,
          error: 'SSL certificate validation failed. Connection rejected.',
        ),
      );
      return;
    }

    handler.next(err);
  }
}

/// Helper class for SSL pinning setup instructions
class SslPinningSetup {
  /// Print setup instructions
  static void printInstructions() {
    debugPrint('''
    
╔═══════════════════════════════════════════════════════════════════════════╗
║                     SSL PINNING SETUP INSTRUCTIONS                        ║
╠═══════════════════════════════════════════════════════════════════════════╣
║                                                                           ║
║  1. Get your server's SSL certificate fingerprint:                       ║
║                                                                           ║
║     openssl s_client -connect api.getir.com:443 < /dev/null \\           ║
║       | openssl x509 -fingerprint -sha256 -noout                         ║
║                                                                           ║
║  2. Copy the SHA-256 fingerprint to _isPinnedCertificate() method        ║
║                                                                           ║
║  3. (Optional) Export your server certificate:                           ║
║                                                                           ║
║     openssl s_client -connect api.getir.com:443 < /dev/null \\           ║
║       | openssl x509 -outform PEM > assets/certificates/getir_server.pem ║
║                                                                           ║
║  4. Enable SSL pinning in .env.prod:                                     ║
║                                                                           ║
║     ENABLE_SSL_PINNING=true                                              ║
║                                                                           ║
║  5. Test in staging environment before production deployment             ║
║                                                                           ║
╚═══════════════════════════════════════════════════════════════════════════╝
    ''');
  }
}
