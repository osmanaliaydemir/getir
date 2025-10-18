import 'dart:io';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';
import 'package:crypto/crypto.dart'; // ✅ SHA-256 hashing için
import '../config/environment_config.dart';
import '../services/logger_service.dart';

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
      logger.info('SSL Pinning: Disabled', tag: 'SSLPinning');
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
          logger.info(
            'Loaded certificate',
            tag: 'SSLPinning',
            context: {'path': certPath},
          );
        } catch (e) {
          logger.warning(
            'Failed to load certificate',
            tag: 'SSLPinning',
            context: {'path': certPath, 'error': e.toString()},
          );
        }
      }

      _isInitialized = true;
      logger.info('SSL Pinning initialized successfully', tag: 'SSLPinning');
    } catch (e) {
      logger.error(
        'SSL Pinning initialization failed',
        tag: 'SSLPinning',
        error: e,
      );
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
            logger.debug(
              'Validating certificate',
              tag: 'SSLPinning',
              context: {'host': host},
            );

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
        logger.debug(
          'Development mode - allowing all certificates',
          tag: 'SSLPinning',
        );
        return true;
      }

      // In production, validate against pinned hashes
      // You should implement proper certificate validation here
      logger.debug(
        'Validating certificate against pinned hashes',
        tag: 'SSLPinning',
        context: {'host': host},
      );
      return _isPinnedCertificate(certDer);
    } catch (e) {
      logger.error(
        'Certificate validation failed',
        tag: 'SSLPinning',
        error: e,
      );
      return false;
    }
  }

  /// Check if certificate is pinned
  bool _isPinnedCertificate(Uint8List certDer) {
    // Production certificate pinning
    // Compute SHA-256 hash of the certificate
    final certHash = sha256.convert(certDer).toString();

    // ⚠️ IMPORTANT: Replace these with your actual production certificate hashes
    // How to get certificate hash:
    //
    // Option 1 - OpenSSL (Linux/Mac/Git Bash):
    // openssl s_client -connect ajilgo.runasp.net:443 </dev/null 2>/dev/null | \
    //   openssl x509 -outform DER | \
    //   openssl dgst -sha256 -binary | \
    //   openssl base64
    //
    // Option 2 - PowerShell (Windows):
    // $cert = (Invoke-WebRequest -Uri "https://ajilgo.runasp.net").BaseResponse.ServicePoint.Certificate
    // $bytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    // [System.Convert]::ToBase64String((New-Object System.Security.Cryptography.SHA256Managed).ComputeHash($bytes))
    //
    // Option 3 - Browser (Easy):
    // 1. Chrome'da https://ajilgo.runasp.net'e git
    // 2. Adres çubuğundaki kilit ikonuna tıkla
    // 3. Certificate → Details → Thumbprint (SHA-256) kopyala

    // ✅ Pinned certificate hashes (SHA-256)
    final pinnedHashes = {
      // ⚠️ PLACEHOLDER - GERÇEK HASH'LERLE DEĞİŞTİR!
      // ajilgo.runasp.net certificate hash (ÖRNEK - GERÇEK DEĞİL!)
      'a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456',

      // Backup certificate (certificate renewal için)
      'b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef1234567',

      // Let's Encrypt Root CA (if using Let's Encrypt)
      'c3d4e5f6789012345678901234567890abcdef1234567890abcdef12345678',
    };

    final isValid = pinnedHashes.contains(certHash);

    if (!isValid) {
      logger.warning(
        'Certificate hash not in pinned list!',
        tag: 'SSLPinning',
        context: {'hash': certHash, 'pinnedCount': pinnedHashes.length},
      );
    } else {
      logger.debug(
        'Certificate validated successfully',
        tag: 'SSLPinning',
        context: {'hash': certHash.substring(0, 16) + '...'},
      );
    }

    return isValid;
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (err.type == DioExceptionType.connectionError &&
        err.error is HandshakeException) {
      // SSL handshake failed - possible MITM attack
      logger.error(
        'SSL Certificate validation failed! Possible MITM attack detected.',
        tag: 'SSLPinning',
        error: err.error,
        context: {'url': err.requestOptions.uri.toString()},
      );

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
    logger.info('''
    
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
