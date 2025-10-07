import 'dart:async';
import 'package:flutter/material.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/services/signalr_service.dart';

/// Real-time Order Tracking Page
/// Displays live courier location and order status updates
class OrderTrackingPage extends StatefulWidget {
  final String orderId;

  const OrderTrackingPage({super.key, required this.orderId});

  @override
  State<OrderTrackingPage> createState() => _OrderTrackingPageState();
}

class _OrderTrackingPageState extends State<OrderTrackingPage> {
  final SignalRService _signalRService = SignalRService();
  GoogleMapController? _mapController;

  // Tracking data
  TrackingData? _currentTracking;
  StreamSubscription<TrackingData>? _trackingSubscription;
  StreamSubscription<LocationUpdate>? _locationSubscription;

  // Map markers
  final Set<Marker> _markers = {};
  final Set<Polyline> _polylines = {};

  @override
  void initState() {
    super.initState();
    _initializeTracking();
  }

  @override
  void dispose() {
    _trackingSubscription?.cancel();
    _locationSubscription?.cancel();
    _signalRService.leaveOrderTrackingGroup(widget.orderId);
    super.dispose();
  }

  Future<void> _initializeTracking() async {
    // Subscribe to tracking updates
    await _signalRService.joinOrderTrackingGroup(widget.orderId);

    // Listen to tracking data stream
    _trackingSubscription = _signalRService.trackingDataStream.listen((
      tracking,
    ) {
      if (mounted) {
        setState(() {
          _currentTracking = tracking;
          _updateMapMarkers(tracking);
        });
      }
    });

    // Listen to location updates
    _locationSubscription = _signalRService.locationUpdateStream.listen((
      location,
    ) {
      if (mounted) {
        _updateCourierLocation(location);
      }
    });

    // Request initial tracking data
    await _signalRService.requestTrackingData(widget.orderId);
  }

  void _updateMapMarkers(TrackingData tracking) {
    _markers.clear();

    // Add courier marker
    if (tracking.courierLatitude != null && tracking.courierLongitude != null) {
      _markers.add(
        Marker(
          markerId: const MarkerId('courier'),
          position: LatLng(
            tracking.courierLatitude!,
            tracking.courierLongitude!,
          ),
          icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueBlue),
          infoWindow: const InfoWindow(title: 'Kurye'),
        ),
      );
    }

    // Add destination marker
    if (tracking.destinationLatitude != null &&
        tracking.destinationLongitude != null) {
      _markers.add(
        Marker(
          markerId: const MarkerId('destination'),
          position: LatLng(
            tracking.destinationLatitude!,
            tracking.destinationLongitude!,
          ),
          icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueRed),
          infoWindow: const InfoWindow(title: 'Teslimat Adresi'),
        ),
      );

      // Draw route line
      if (tracking.courierLatitude != null &&
          tracking.courierLongitude != null) {
        _polylines.add(
          Polyline(
            polylineId: const PolylineId('route'),
            points: [
              LatLng(tracking.courierLatitude!, tracking.courierLongitude!),
              LatLng(
                tracking.destinationLatitude!,
                tracking.destinationLongitude!,
              ),
            ],
            color: AppColors.primary,
            width: 4,
          ),
        );
      }

      // Update camera to show both markers
      _updateCamera(tracking);
    }
  }

  void _updateCourierLocation(LocationUpdate location) {
    // Update courier marker
    _markers.removeWhere((m) => m.markerId.value == 'courier');
    _markers.add(
      Marker(
        markerId: const MarkerId('courier'),
        position: LatLng(location.latitude, location.longitude),
        icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueBlue),
        infoWindow: InfoWindow(title: 'Kurye', snippet: location.address ?? ''),
      ),
    );

    if (mounted) {
      setState(() {});
    }
  }

  void _updateCamera(TrackingData tracking) {
    if (tracking.courierLatitude != null &&
        tracking.courierLongitude != null &&
        tracking.destinationLatitude != null &&
        tracking.destinationLongitude != null) {
      // Calculate bounds to show both markers
      final bounds = LatLngBounds(
        southwest: LatLng(
          tracking.courierLatitude! < tracking.destinationLatitude!
              ? tracking.courierLatitude!
              : tracking.destinationLatitude!,
          tracking.courierLongitude! < tracking.destinationLongitude!
              ? tracking.courierLongitude!
              : tracking.destinationLongitude!,
        ),
        northeast: LatLng(
          tracking.courierLatitude! > tracking.destinationLatitude!
              ? tracking.courierLatitude!
              : tracking.destinationLatitude!,
          tracking.courierLongitude! > tracking.destinationLongitude!
              ? tracking.courierLongitude!
              : tracking.destinationLongitude!,
        ),
      );

      _mapController?.animateCamera(CameraUpdate.newLatLngBounds(bounds, 100));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Sipariş Takibi'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: _currentTracking == null
          ? _buildLoadingState()
          : Column(
              children: [
                // Map
                Expanded(flex: 2, child: _buildMap()),

                // Tracking Info
                Expanded(flex: 1, child: _buildTrackingInfo()),
              ],
            ),
    );
  }

  Widget _buildLoadingState() {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          CircularProgressIndicator(
            valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
          ),
          SizedBox(height: 16),
          Text(
            'Sipariş bilgileri yükleniyor...',
            style: TextStyle(color: AppColors.textSecondary),
          ),
        ],
      ),
    );
  }

  Widget _buildMap() {
    final initialPosition =
        _currentTracking?.courierLatitude != null &&
            _currentTracking?.courierLongitude != null
        ? LatLng(
            _currentTracking!.courierLatitude!,
            _currentTracking!.courierLongitude!,
          )
        : const LatLng(41.0082, 28.9784); // Istanbul default

    return GoogleMap(
      initialCameraPosition: CameraPosition(target: initialPosition, zoom: 14),
      markers: _markers,
      polylines: _polylines,
      myLocationEnabled: true,
      myLocationButtonEnabled: true,
      zoomControlsEnabled: false,
      onMapCreated: (controller) {
        _mapController = controller;
        if (_currentTracking != null) {
          _updateCamera(_currentTracking!);
        }
      },
    );
  }

  Widget _buildTrackingInfo() {
    final tracking = _currentTracking!;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.1),
            blurRadius: 10,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Drag handle
            Center(
              child: Container(
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: Colors.grey[300],
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
            ),
            const SizedBox(height: 16),

            // Status
            _buildStatusSection(tracking.status),
            const SizedBox(height: 16),

            // ETA
            if (tracking.estimatedArrivalMinutes != null) ...[
              _buildETASection(tracking.estimatedArrivalMinutes!),
              const SizedBox(height: 16),
            ],

            // Distance
            if (tracking.distanceFromDestination != null) ...[
              _buildDistanceSection(tracking.distanceFromDestination!),
              const SizedBox(height: 16),
            ],

            // Last updated
            Text(
              'Son güncelleme: ${_formatTime(tracking.lastUpdated)}',
              style: AppTypography.bodySmall.copyWith(
                color: AppColors.textSecondary,
                fontStyle: FontStyle.italic,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatusSection(String status) {
    final statusInfo = _getStatusInfo(status);

    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: statusInfo['color'].withOpacity(0.1),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: statusInfo['color'], width: 2),
      ),
      child: Row(
        children: [
          Icon(statusInfo['icon'], color: statusInfo['color'], size: 32),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  statusInfo['title'],
                  style: AppTypography.bodyLarge.copyWith(
                    fontWeight: FontWeight.bold,
                    color: statusInfo['color'],
                  ),
                ),
                Text(
                  statusInfo['subtitle'],
                  style: AppTypography.bodySmall.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildETASection(int minutes) {
    return Row(
      children: [
        Icon(Icons.access_time, color: AppColors.primary, size: 24),
        const SizedBox(width: 12),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Tahmini Varış',
              style: AppTypography.bodySmall.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
            Text(
              '$minutes dakika',
              style: AppTypography.bodyLarge.copyWith(
                fontWeight: FontWeight.bold,
                color: AppColors.primary,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildDistanceSection(double distance) {
    return Row(
      children: [
        Icon(Icons.place, color: AppColors.error, size: 24),
        const SizedBox(width: 12),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Kalan Mesafe',
              style: AppTypography.bodySmall.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
            Text(
              '${distance.toStringAsFixed(2)} km',
              style: AppTypography.bodyLarge.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Map<String, dynamic> _getStatusInfo(String status) {
    switch (status.toLowerCase()) {
      case 'preparing':
        return {
          'icon': Icons.restaurant_menu,
          'color': Colors.orange,
          'title': 'Hazırlanıyor',
          'subtitle': 'Siparişiniz hazırlanıyor',
        };
      case 'ready':
        return {
          'icon': Icons.check_circle,
          'color': Colors.green,
          'title': 'Hazır',
          'subtitle': 'Kurye yolda',
        };
      case 'pickedup':
        return {
          'icon': Icons.delivery_dining,
          'color': Colors.blue,
          'title': 'Kuryede',
          'subtitle': 'Kurye siparişi aldı',
        };
      case 'ontheway':
        return {
          'icon': Icons.local_shipping,
          'color': AppColors.primary,
          'title': 'Yolda',
          'subtitle': 'Siparişiniz size geliyor',
        };
      case 'delivered':
        return {
          'icon': Icons.done_all,
          'color': Colors.green,
          'title': 'Teslim Edildi',
          'subtitle': 'Afiyet olsun!',
        };
      default:
        return {
          'icon': Icons.info,
          'color': Colors.grey,
          'title': 'Bilinmeyen Durum',
          'subtitle': '',
        };
    }
  }

  String _formatTime(DateTime dateTime) {
    final now = DateTime.now();
    final diff = now.difference(dateTime);

    if (diff.inMinutes < 1) {
      return 'Az önce';
    } else if (diff.inMinutes < 60) {
      return '${diff.inMinutes} dakika önce';
    } else if (diff.inHours < 24) {
      return '${diff.inHours} saat önce';
    } else {
      return '${dateTime.day}/${dateTime.month}/${dateTime.year} ${dateTime.hour}:${dateTime.minute.toString().padLeft(2, '0')}';
    }
  }
}
