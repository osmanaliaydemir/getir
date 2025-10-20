import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../../core/animations/loading_animations.dart';
import '../../../core/constants/route_constants.dart';
import '../../../core/services/local_storage_service.dart';
import '../../../core/services/secure_encryption_service.dart';
import '../../../core/di/injection.dart';

/// Splash screen with Getir branding and animations
class SplashPage extends StatefulWidget {
  const SplashPage({super.key});

  @override
  State<SplashPage> createState() => _SplashPageState();
}

class _SplashPageState extends State<SplashPage> with TickerProviderStateMixin {
  late AnimationController _logoController;
  late AnimationController _textController;
  late AnimationController _progressController;

  late Animation<double> _logoScale;
  late Animation<double> _logoOpacity;
  late Animation<double> _textOpacity;
  late Animation<double> _progressOpacity;
  late Animation<double> _progressValue;

  @override
  void initState() {
    super.initState();
    _initializeAnimations();
    _startSplashSequence();
  }

  void _initializeAnimations() {
    // Logo animation - more dynamic
    _logoController = AnimationController(
      duration: const Duration(milliseconds: 2000),
      vsync: this,
    );

    _logoScale = Tween<double>(begin: 0.3, end: 1.0).animate(
      CurvedAnimation(parent: _logoController, curve: Curves.elasticOut),
    );

    _logoOpacity = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _logoController,
        curve: const Interval(0.0, 0.7, curve: Curves.easeIn),
      ),
    );

    // Text animation - staggered
    _textController = AnimationController(
      duration: const Duration(milliseconds: 1200),
      vsync: this,
    );

    _textOpacity = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(parent: _textController, curve: Curves.easeOut));

    // Progress animation - continuous
    _progressController = AnimationController(
      duration: const Duration(milliseconds: 2500),
      vsync: this,
    );

    _progressOpacity = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _progressController, curve: Curves.easeIn),
    );

    _progressValue = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _progressController, curve: Curves.easeInOut),
    );
  }

  void _startSplashSequence() async {
    // Start logo animation with delay
    await Future.delayed(const Duration(milliseconds: 300));
    _logoController.forward();

    // Start text animation with overlap
    await Future.delayed(const Duration(milliseconds: 800));
    _textController.forward();

    // Start progress animation with overlap
    await Future.delayed(const Duration(milliseconds: 1200));
    _progressController.forward();

    // Wait for progress to complete + extra time for smooth transition
    await Future.delayed(const Duration(milliseconds: 1500));

    // Check if user has seen onboarding
    final storage = LocalStorageService();
    final hasSeenOnboarding =
        storage.getUserData('has_seen_onboarding') == 'true';

    // Token kontrolü - SecureEncryptionService'den (LocalStorage değil!)
    final encryptionService = getIt<SecureEncryptionService>();
    final hasToken = await encryptionService.hasAccessToken();

    // Navigate to appropriate screen
    if (mounted) {
      if (hasToken) {
        // User is logged in → Go to home
        debugPrint('✅ [SplashPage] Token found, navigating to home');
        context.go(RouteConstants.home);
      } else if (hasSeenOnboarding) {
        // User has seen onboarding → Go to login
        debugPrint('⚠️ [SplashPage] No token, navigating to login');
        context.go(RouteConstants.login);
      } else {
        // First time user → Go to onboarding
        debugPrint('ℹ️ [SplashPage] First time user, navigating to onboarding');
        context.go(RouteConstants.onboarding);
      }
    }
  }

  @override
  void dispose() {
    _logoController.dispose();
    _textController.dispose();
    _progressController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.primary,
      body: AnimatedBuilder(
        animation: Listenable.merge([
          _logoController,
          _textController,
          _progressController,
        ]),
        builder: (context, child) {
          return Container(
            decoration: BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: [
                  Theme.of(context).colorScheme.primary,
                  Theme.of(context).colorScheme.primary.withOpacity(0.9),
                  Theme.of(context).colorScheme.primary.withOpacity(0.7),
                  Theme.of(context).colorScheme.primary,
                ],
                stops: const [0.0, 0.3, 0.7, 1.0],
              ),
            ),
            child: SafeArea(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  const Spacer(flex: 3),

                  // Logo Section - Perfectly centered
                  Center(
                    child: Transform.scale(
                      scale: _logoScale.value,
                      child: Opacity(
                        opacity: _logoOpacity.value,
                        child: _buildLogo(),
                      ),
                    ),
                  ),

                  const SizedBox(height: 50),

                  // Text Section - Perfectly centered
                  Center(
                    child: Opacity(
                      opacity: _textOpacity.value,
                      child: _buildText(),
                    ),
                  ),

                  const Spacer(flex: 4),

                  // Progress Section - Perfectly centered at bottom
                  Center(
                    child: Opacity(
                      opacity: _progressOpacity.value,
                      child: _buildProgress(),
                    ),
                  ),

                  const SizedBox(height: 80),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildLogo() {
    return AnimatedBuilder(
      animation: _logoController,
      builder: (context, child) {
        return Container(
          width: 140,
          height: 140,
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(25),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.15),
                blurRadius: 30,
                offset: const Offset(0, 15),
                spreadRadius: 2,
              ),
              BoxShadow(
                color: Colors.white.withOpacity(0.2),
                blurRadius: 10,
                offset: const Offset(0, -5),
                spreadRadius: 1,
              ),
            ],
          ),
          child: Stack(
            alignment: Alignment.center,
            children: [
              // Rotating background circle
              Transform.rotate(
                angle: _logoController.value * 2 * 3.14159,
                child: Container(
                  width: 100,
                  height: 100,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    gradient: LinearGradient(
                      colors: [
                        const Color(0xFF5D3EBC).withOpacity(0.1),
                        const Color(0xFF5D3EBC).withOpacity(0.3),
                        const Color(0xFF5D3EBC).withOpacity(0.1),
                      ],
                      stops: const [0.0, 0.5, 1.0],
                    ),
                  ),
                ),
              ),
              // Main icon with pulse animation
              Transform.scale(
                scale: 1.0 + (0.05 * (1 - _logoController.value).abs()),
                child: const Icon(
                  Icons.local_shipping,
                  size: 70,
                  color: Color(0xFF5D3EBC), // Getir purple
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildText() {
    return AnimatedBuilder(
      animation: _textController,
      builder: (context, child) {
        return Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            // Main title with bounce animation
            Transform.translate(
              offset: Offset(0, 20 * (1 - _textController.value)),
              child: Text(
                'Getir',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.headlineLarge?.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 2,
                  shadows: [
                    Shadow(
                      color: Colors.black.withOpacity(0.3),
                      offset: const Offset(0, 2),
                      blurRadius: 4,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 8),
            // Subtitle with fade-in animation
            Opacity(
              opacity: _textController.value,
              child: Text(
                'Hızlı Teslimat',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  color: Colors.white.withOpacity(0.9),
                  letterSpacing: 1,
                  shadows: [
                    Shadow(
                      color: Colors.black.withOpacity(0.2),
                      offset: const Offset(0, 1),
                      blurRadius: 2,
                    ),
                  ],
                ),
              ),
            ),
          ],
        );
      },
    );
  }

  Widget _buildProgress() {
    return AnimatedBuilder(
      animation: _progressController,
      builder: (context, child) {
        return Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            // Animated progress bar with glow effect
            Center(
              child: Container(
                width: 250,
                height: 6,
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(3),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.white.withOpacity(0.1),
                      blurRadius: 4,
                      spreadRadius: 1,
                    ),
                  ],
                ),
                child: AnimatedBuilder(
                  animation: _progressValue,
                  builder: (context, child) {
                    return Stack(
                      children: [
                        // Background
                        Container(
                          width: double.infinity,
                          height: double.infinity,
                          decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.2),
                            borderRadius: BorderRadius.circular(3),
                          ),
                        ),
                        // Progress fill with gradient
                        FractionallySizedBox(
                          alignment: Alignment.centerLeft,
                          widthFactor: _progressValue.value,
                          child: Container(
                            decoration: BoxDecoration(
                              gradient: LinearGradient(
                                colors: [
                                  Colors.white,
                                  Colors.white.withOpacity(0.9),
                                ],
                              ),
                              borderRadius: BorderRadius.circular(3),
                              boxShadow: [
                                BoxShadow(
                                  color: Colors.white.withOpacity(0.5),
                                  blurRadius: 8,
                                  spreadRadius: 1,
                                ),
                              ],
                            ),
                          ),
                        ),
                        // Animated shimmer effect
                        if (_progressValue.value > 0)
                          FractionallySizedBox(
                            alignment: Alignment.centerLeft,
                            widthFactor: _progressValue.value,
                            child: Container(
                              decoration: BoxDecoration(
                                gradient: LinearGradient(
                                  colors: [
                                    Colors.transparent,
                                    Colors.white.withOpacity(0.3),
                                    Colors.transparent,
                                  ],
                                  stops: const [0.0, 0.5, 1.0],
                                  begin: Alignment.centerLeft,
                                  end: Alignment.centerRight,
                                ),
                                borderRadius: BorderRadius.circular(3),
                              ),
                            ),
                          ),
                      ],
                    );
                  },
                ),
              ),
            ),

            const SizedBox(height: 20),

            // Loading text with dots animation
            Center(
              child: AnimatedBuilder(
                animation: _progressController,
                builder: (context, child) {
                  final dots =
                      '.' * ((_progressController.value * 3).floor() + 1);
                  return Text(
                    'Yükleniyor$dots',
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: Colors.white.withOpacity(0.9),
                      letterSpacing: 1,
                      shadows: [
                        Shadow(
                          color: Colors.black.withOpacity(0.3),
                          offset: const Offset(0, 1),
                          blurRadius: 2,
                        ),
                      ],
                    ),
                  );
                },
              ),
            ),
          ],
        );
      },
    );
  }
}

/// Splash screen with custom loading animation
class AnimatedSplashPage extends StatefulWidget {
  const AnimatedSplashPage({super.key});

  @override
  State<AnimatedSplashPage> createState() => _AnimatedSplashPageState();
}

class _AnimatedSplashPageState extends State<AnimatedSplashPage>
    with TickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _rotation;
  late Animation<double> _scale;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(seconds: 2),
      vsync: this,
    );

    _rotation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.linear));

    _scale = Tween<double>(
      begin: 0.8,
      end: 1.2,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));

    _controller.repeat(reverse: true);

    // Navigate after 3 seconds
    Future.delayed(const Duration(seconds: 3), () async {
      if (mounted) {
        final encryptionService = getIt<SecureEncryptionService>();
        final hasToken = await encryptionService.hasAccessToken();

        if (hasToken) {
          debugPrint('✅ [AnimatedSplashPage] Token found, navigating to home');
          context.go(RouteConstants.home);
        } else {
          debugPrint(
            '⚠️ [AnimatedSplashPage] No token, navigating to onboarding',
          );
          context.go(RouteConstants.onboarding);
        }
      }
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.primary,
      body: Center(
        child: AnimatedBuilder(
          animation: _controller,
          builder: (context, child) {
            return Transform.rotate(
              angle: _rotation.value * 2 * 3.14159,
              child: Transform.scale(
                scale: _scale.value,
                child: LoadingAnimations.getirLoader(
                  size: 100,
                  color: Colors.white,
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}
