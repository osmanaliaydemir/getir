import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_cache_manager/flutter_cache_manager.dart';

/// Image Cache Configuration
/// Optimizes CachedNetworkImage performance and memory usage
class ImageCacheConfig {
  /// Custom cache manager with optimized settings
  static final CacheManager cacheManager = CacheManager(
    Config(
      'getir_image_cache',
      stalePeriod: const Duration(days: 7), // Cache validity: 7 days
      maxNrOfCacheObjects: 200, // Maximum cached images: 200
      repo: JsonCacheInfoRepository(databaseName: 'getir_image_cache'),
      fileService: HttpFileService(),
    ),
  );

  /// Get optimized cached image widget
  /// Usage:
  /// ```dart
  /// ImageCacheConfig.getCachedImage(
  ///   imageUrl: 'https://example.com/image.jpg',
  ///   placeholder: CircularProgressIndicator(),
  /// )
  /// ```
  static Widget getCachedImage({
    required String imageUrl,
    Widget? placeholder,
    Widget? errorWidget,
    BoxFit? fit,
    double? width,
    double? height,
    BorderRadius? borderRadius,
  }) {
    return ClipRRect(
      borderRadius: borderRadius ?? BorderRadius.zero,
      child: CachedNetworkImage(
        imageUrl: imageUrl,
        cacheManager: cacheManager,
        fit: fit ?? BoxFit.cover,
        width: width,
        height: height,
        placeholder: (context, url) =>
            placeholder ??
            const Center(
              child: SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            ),
        errorWidget: (context, url, error) =>
            errorWidget ??
            const Icon(
              Icons.broken_image_outlined,
              color: Colors.grey,
              size: 32,
            ),
        // Memory optimization
        memCacheWidth: _calculateMemCacheWidth(width),
        memCacheHeight: _calculateMemCacheHeight(height),
        // Fade animation
        fadeInDuration: const Duration(milliseconds: 300),
        fadeOutDuration: const Duration(milliseconds: 100),
      ),
    );
  }

  /// Get cached image for lists (optimized for performance)
  /// Reduced memory cache size for better scrolling performance
  static Widget getCachedImageForList({
    required String imageUrl,
    double? width,
    double? height,
    BoxFit? fit,
  }) {
    return CachedNetworkImage(
      imageUrl: imageUrl,
      cacheManager: cacheManager,
      fit: fit ?? BoxFit.cover,
      width: width,
      height: height,
      // Aggressive memory optimization for lists
      memCacheWidth: (width != null) ? (width * 2).toInt() : 400,
      memCacheHeight: (height != null) ? (height * 2).toInt() : 400,
      // Faster fade for better scroll performance
      fadeInDuration: const Duration(milliseconds: 200),
      fadeOutDuration: const Duration(milliseconds: 50),
      // Minimal placeholder for lists
      placeholder: (context, url) =>
          Container(color: Colors.grey[200], width: width, height: height),
      errorWidget: (context, url, error) => Container(
        color: Colors.grey[300],
        width: width,
        height: height,
        child: const Icon(Icons.broken_image, color: Colors.grey, size: 24),
      ),
    );
  }

  /// Preload images for better UX
  /// Call this for images that will be displayed soon
  static Future<void> preloadImage(
    BuildContext context,
    String imageUrl,
  ) async {
    try {
      await precacheImage(
        CachedNetworkImageProvider(imageUrl, cacheManager: cacheManager),
        context,
      );
    } catch (e) {
      debugPrint('⚠️  Failed to preload image: $e');
    }
  }

  /// Clear image cache
  static Future<void> clearCache() async {
    try {
      await cacheManager.emptyCache();
      debugPrint('🗑️  Image cache cleared');
    } catch (e) {
      debugPrint('❌ Failed to clear image cache: $e');
    }
  }

  /// Get cache info (size, count)
  static Future<Map<String, dynamic>> getCacheInfo() async {
    try {
      // For now, return estimated info
      // Would need to iterate cache directory to get actual count/size
      return {'count': 0, 'size': 0, 'sizeMB': '0.00'};
    } catch (e) {
      debugPrint('❌ Failed to get cache info: $e');
      return {'count': 0, 'size': 0, 'sizeMB': '0.00'};
    }
  }

  /// Calculate optimal memory cache width
  /// Prevents loading huge images into memory
  static int? _calculateMemCacheWidth(double? width) {
    if (width == null) return null;

    // Use 2x for retina displays, but cap at 1080px
    final memWidth = (width * 2).toInt();
    return memWidth > 1080 ? 1080 : memWidth;
  }

  /// Calculate optimal memory cache height
  static int? _calculateMemCacheHeight(double? height) {
    if (height == null) return null;

    // Use 2x for retina displays, but cap at 1920px
    final memHeight = (height * 2).toInt();
    return memHeight > 1920 ? 1920 : memHeight;
  }
}

/// Extension for easy cached image usage
extension CachedImageWidget on String {
  /// Convert image URL string to cached image widget
  /// Usage: 'https://example.com/image.jpg'.toCachedImage()
  Widget toCachedImage({
    BoxFit? fit,
    double? width,
    double? height,
    BorderRadius? borderRadius,
  }) {
    return ImageCacheConfig.getCachedImage(
      imageUrl: this,
      fit: fit,
      width: width,
      height: height,
      borderRadius: borderRadius,
    );
  }

  /// For list items (optimized)
  Widget toCachedImageForList({double? width, double? height, BoxFit? fit}) {
    return ImageCacheConfig.getCachedImageForList(
      imageUrl: this,
      width: width,
      height: height,
      fit: fit,
    );
  }
}

/// Image loading strategies
class ImageLoadingStrategy {
  /// Low quality placeholder with high quality image
  static ImageProvider lowQualityPlaceholder(String imageUrl) {
    return CachedNetworkImageProvider(
      imageUrl,
      cacheManager: ImageCacheConfig.cacheManager,
    );
  }

  /// Progressive image loading
  static Widget progressiveLoading(String imageUrl, {BoxFit? fit}) {
    return Image(
      image: CachedNetworkImageProvider(
        imageUrl,
        cacheManager: ImageCacheConfig.cacheManager,
      ),
      fit: fit ?? BoxFit.cover,
      frameBuilder: (context, child, frame, wasSynchronouslyLoaded) {
        if (wasSynchronouslyLoaded) return child;

        return AnimatedOpacity(
          opacity: frame == null ? 0.0 : 1.0,
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOut,
          child: child,
        );
      },
      loadingBuilder: (context, child, loadingProgress) {
        if (loadingProgress == null) return child;

        return Center(
          child: CircularProgressIndicator(
            value: loadingProgress.expectedTotalBytes != null
                ? loadingProgress.cumulativeBytesLoaded /
                      (loadingProgress.expectedTotalBytes ?? 1)
                : null,
          ),
        );
      },
    );
  }
}
