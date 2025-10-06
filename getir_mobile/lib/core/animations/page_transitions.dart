import 'package:flutter/material.dart';

/// Custom page transition animations for the app
class PageTransitions {
  /// Slide transition from right to left
  static Widget slideFromRight(Widget child) {
    return SlideTransition(
      position: Tween<Offset>(begin: const Offset(1.0, 0.0), end: Offset.zero)
          .animate(
            CurvedAnimation(
              parent: kAlwaysCompleteAnimation,
              curve: Curves.easeInOut,
            ),
          ),
      child: child,
    );
  }

  /// Slide transition from left to right
  static Widget slideFromLeft(Widget child) {
    return SlideTransition(
      position: Tween<Offset>(begin: const Offset(-1.0, 0.0), end: Offset.zero)
          .animate(
            CurvedAnimation(
              parent: kAlwaysCompleteAnimation,
              curve: Curves.easeInOut,
            ),
          ),
      child: child,
    );
  }

  /// Slide transition from bottom to top
  static Widget slideFromBottom(Widget child) {
    return SlideTransition(
      position: Tween<Offset>(begin: const Offset(0.0, 1.0), end: Offset.zero)
          .animate(
            CurvedAnimation(
              parent: kAlwaysCompleteAnimation,
              curve: Curves.easeOutCubic,
            ),
          ),
      child: child,
    );
  }

  /// Fade transition
  static Widget fadeTransition(Widget child) {
    return FadeTransition(
      opacity: Tween<double>(begin: 0.0, end: 1.0).animate(
        CurvedAnimation(parent: kAlwaysCompleteAnimation, curve: Curves.easeIn),
      ),
      child: child,
    );
  }

  /// Scale transition
  static Widget scaleTransition(Widget child) {
    return ScaleTransition(
      scale: Tween<double>(begin: 0.8, end: 1.0).animate(
        CurvedAnimation(
          parent: kAlwaysCompleteAnimation,
          curve: Curves.elasticOut,
        ),
      ),
      child: child,
    );
  }

  /// Combined slide and fade transition
  static Widget slideAndFade(Widget child) {
    return SlideTransition(
      position: Tween<Offset>(begin: const Offset(0.0, 0.3), end: Offset.zero)
          .animate(
            CurvedAnimation(
              parent: kAlwaysCompleteAnimation,
              curve: Curves.easeOutCubic,
            ),
          ),
      child: FadeTransition(
        opacity: Tween<double>(begin: 0.0, end: 1.0).animate(
          CurvedAnimation(
            parent: kAlwaysCompleteAnimation,
            curve: Curves.easeIn,
          ),
        ),
        child: child,
      ),
    );
  }

  /// Hero transition for product cards
  static Widget heroTransition(Widget child, String heroTag) {
    return Hero(
      tag: heroTag,
      child: Material(color: Colors.transparent, child: child),
    );
  }

  /// Custom route transition builder
  static PageRouteBuilder<T> buildRoute<T extends Object?>(
    Widget page, {
    String transitionType = 'slideRight',
    Duration duration = const Duration(milliseconds: 300),
  }) {
    return PageRouteBuilder<T>(
      pageBuilder: (context, animation, secondaryAnimation) => page,
      transitionDuration: duration,
      reverseTransitionDuration: duration,
      transitionsBuilder: (context, animation, secondaryAnimation, child) {
        switch (transitionType) {
          case 'slideRight':
            return slideFromRight(child);
          case 'slideLeft':
            return slideFromLeft(child);
          case 'slideBottom':
            return slideFromBottom(child);
          case 'fade':
            return fadeTransition(child);
          case 'scale':
            return scaleTransition(child);
          case 'slideAndFade':
            return slideAndFade(child);
          default:
            return slideFromRight(child);
        }
      },
    );
  }
}

/// Custom page route for Getir app
class GetirPageRoute<T> extends PageRouteBuilder<T> {
  final Widget page;
  final String transitionType;
  final Duration duration;

  GetirPageRoute({
    required this.page,
    this.transitionType = 'slideRight',
    this.duration = const Duration(milliseconds: 300),
  }) : super(
         pageBuilder: (context, animation, secondaryAnimation) => page,
         transitionDuration: duration,
         reverseTransitionDuration: duration,
         transitionsBuilder: (context, animation, secondaryAnimation, child) {
           switch (transitionType) {
             case 'slideRight':
               return PageTransitions.slideFromRight(child);
             case 'slideLeft':
               return PageTransitions.slideFromLeft(child);
             case 'slideBottom':
               return PageTransitions.slideFromBottom(child);
             case 'fade':
               return PageTransitions.fadeTransition(child);
             case 'scale':
               return PageTransitions.scaleTransition(child);
             case 'slideAndFade':
               return PageTransitions.slideAndFade(child);
             default:
               return PageTransitions.slideFromRight(child);
           }
         },
       );
}
