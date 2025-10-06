import 'package:flutter/material.dart';

/// Responsive design service for different screen sizes
class ResponsiveService {
  static final ResponsiveService _instance = ResponsiveService._internal();
  factory ResponsiveService() => _instance;
  ResponsiveService._internal();

  /// Breakpoints for different screen sizes
  static const double mobileBreakpoint = 600;
  static const double tabletBreakpoint = 900;
  static const double desktopBreakpoint = 1200;

  /// Get screen size category
  static ScreenSize getScreenSize(BuildContext context) {
    final width = MediaQuery.of(context).size.width;

    if (width < mobileBreakpoint) {
      return ScreenSize.mobile;
    } else if (width < tabletBreakpoint) {
      return ScreenSize.tablet;
    } else if (width < desktopBreakpoint) {
      return ScreenSize.desktop;
    } else {
      return ScreenSize.largeDesktop;
    }
  }

  /// Check if screen is mobile
  static bool isMobile(BuildContext context) {
    return getScreenSize(context) == ScreenSize.mobile;
  }

  /// Check if screen is tablet
  static bool isTablet(BuildContext context) {
    return getScreenSize(context) == ScreenSize.tablet;
  }

  /// Check if screen is desktop
  static bool isDesktop(BuildContext context) {
    return getScreenSize(context) == ScreenSize.desktop;
  }

  /// Check if screen is large desktop
  static bool isLargeDesktop(BuildContext context) {
    return getScreenSize(context) == ScreenSize.largeDesktop;
  }

  /// Get responsive value based on screen size
  static T responsive<T>(
    BuildContext context, {
    required T mobile,
    T? tablet,
    T? desktop,
    T? largeDesktop,
  }) {
    final screenSize = getScreenSize(context);

    switch (screenSize) {
      case ScreenSize.mobile:
        return mobile;
      case ScreenSize.tablet:
        return tablet ?? mobile;
      case ScreenSize.desktop:
        return desktop ?? tablet ?? mobile;
      case ScreenSize.largeDesktop:
        return largeDesktop ?? desktop ?? tablet ?? mobile;
    }
  }

  /// Get responsive padding
  static EdgeInsets responsivePadding(
    BuildContext context, {
    EdgeInsets? mobile,
    EdgeInsets? tablet,
    EdgeInsets? desktop,
    EdgeInsets? largeDesktop,
  }) {
    return responsive(
      context,
      mobile: mobile ?? const EdgeInsets.all(16),
      tablet: tablet ?? const EdgeInsets.all(24),
      desktop: desktop ?? const EdgeInsets.all(32),
      largeDesktop: largeDesktop ?? const EdgeInsets.all(40),
    );
  }

  /// Get responsive margin
  static EdgeInsets responsiveMargin(
    BuildContext context, {
    EdgeInsets? mobile,
    EdgeInsets? tablet,
    EdgeInsets? desktop,
    EdgeInsets? largeDesktop,
  }) {
    return responsive(
      context,
      mobile: mobile ?? const EdgeInsets.all(8),
      tablet: tablet ?? const EdgeInsets.all(12),
      desktop: desktop ?? const EdgeInsets.all(16),
      largeDesktop: largeDesktop ?? const EdgeInsets.all(20),
    );
  }

  /// Get responsive font size
  static double responsiveFontSize(
    BuildContext context, {
    required double mobile,
    double? tablet,
    double? desktop,
    double? largeDesktop,
  }) {
    return responsive(
      context,
      mobile: mobile,
      tablet: tablet ?? mobile * 1.1,
      desktop: desktop ?? mobile * 1.2,
      largeDesktop: largeDesktop ?? mobile * 1.3,
    );
  }

  /// Get responsive icon size
  static double responsiveIconSize(
    BuildContext context, {
    required double mobile,
    double? tablet,
    double? desktop,
    double? largeDesktop,
  }) {
    return responsive(
      context,
      mobile: mobile,
      tablet: tablet ?? mobile * 1.2,
      desktop: desktop ?? mobile * 1.4,
      largeDesktop: largeDesktop ?? mobile * 1.6,
    );
  }

  /// Get responsive grid columns
  static int responsiveGridColumns(BuildContext context) {
    return responsive(
      context,
      mobile: 1,
      tablet: 2,
      desktop: 3,
      largeDesktop: 4,
    );
  }

  /// Get responsive card width
  static double responsiveCardWidth(BuildContext context) {
    return responsive(
      context,
      mobile: double.infinity,
      tablet: 300,
      desktop: 350,
      largeDesktop: 400,
    );
  }

  /// Get responsive max width
  static double responsiveMaxWidth(BuildContext context) {
    return responsive(
      context,
      mobile: double.infinity,
      tablet: 600,
      desktop: 800,
      largeDesktop: 1000,
    );
  }
}

/// Screen size categories
enum ScreenSize { mobile, tablet, desktop, largeDesktop }

/// Responsive widget mixin
mixin ResponsiveMixin {
  /// Get responsive value
  T responsive<T>(
    BuildContext context, {
    required T mobile,
    T? tablet,
    T? desktop,
    T? largeDesktop,
  }) {
    return ResponsiveService.responsive(
      context,
      mobile: mobile,
      tablet: tablet,
      desktop: desktop,
      largeDesktop: largeDesktop,
    );
  }

  /// Check if mobile
  bool isMobile(BuildContext context) => ResponsiveService.isMobile(context);

  /// Check if tablet
  bool isTablet(BuildContext context) => ResponsiveService.isTablet(context);

  /// Check if desktop
  bool isDesktop(BuildContext context) => ResponsiveService.isDesktop(context);

  /// Check if large desktop
  bool isLargeDesktop(BuildContext context) =>
      ResponsiveService.isLargeDesktop(context);
}

/// Responsive container widget
class ResponsiveContainer extends StatelessWidget {
  final Widget child;
  final EdgeInsets? padding;
  final EdgeInsets? margin;
  final double? maxWidth;
  final Alignment? alignment;

  const ResponsiveContainer({
    super.key,
    required this.child,
    this.padding,
    this.margin,
    this.maxWidth,
    this.alignment,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: padding ?? ResponsiveService.responsivePadding(context),
      margin: margin ?? ResponsiveService.responsiveMargin(context),
      constraints: BoxConstraints(
        maxWidth: maxWidth ?? ResponsiveService.responsiveMaxWidth(context),
      ),
      alignment: alignment,
      child: child,
    );
  }
}

/// Responsive grid view
class ResponsiveGridView extends StatelessWidget {
  final List<Widget> children;
  final double? childAspectRatio;
  final double? crossAxisSpacing;
  final double? mainAxisSpacing;
  final EdgeInsets? padding;

  const ResponsiveGridView({
    super.key,
    required this.children,
    this.childAspectRatio,
    this.crossAxisSpacing,
    this.mainAxisSpacing,
    this.padding,
  });

  @override
  Widget build(BuildContext context) {
    final columns = ResponsiveService.responsiveGridColumns(context);

    return GridView.builder(
      padding: padding ?? ResponsiveService.responsivePadding(context),
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: columns,
        childAspectRatio: childAspectRatio ?? 1.0,
        crossAxisSpacing: crossAxisSpacing ?? 16,
        mainAxisSpacing: mainAxisSpacing ?? 16,
      ),
      itemCount: children.length,
      itemBuilder: (context, index) => children[index],
    );
  }
}

/// Responsive list view
class ResponsiveListView extends StatelessWidget {
  final List<Widget> children;
  final Axis scrollDirection;
  final EdgeInsets? padding;
  final double? spacing;

  const ResponsiveListView({
    super.key,
    required this.children,
    this.scrollDirection = Axis.vertical,
    this.padding,
    this.spacing,
  });

  @override
  Widget build(BuildContext context) {
    return ListView.separated(
      padding: padding ?? ResponsiveService.responsivePadding(context),
      scrollDirection: scrollDirection,
      itemCount: children.length,
      separatorBuilder: (context, index) => SizedBox(
        width: scrollDirection == Axis.horizontal ? (spacing ?? 16) : 0,
        height: scrollDirection == Axis.vertical ? (spacing ?? 16) : 0,
      ),
      itemBuilder: (context, index) => children[index],
    );
  }
}

/// Responsive text widget
class ResponsiveText extends StatelessWidget {
  final String text;
  final TextStyle? style;
  final TextAlign? textAlign;
  final int? maxLines;
  final TextOverflow? overflow;

  const ResponsiveText(
    this.text, {
    super.key,
    this.style,
    this.textAlign,
    this.maxLines,
    this.overflow,
  });

  @override
  Widget build(BuildContext context) {
    final responsiveStyle = style?.copyWith(
      fontSize: ResponsiveService.responsiveFontSize(
        context,
        mobile: style?.fontSize ?? 14,
      ),
    );

    return Text(
      text,
      style: responsiveStyle,
      textAlign: textAlign,
      maxLines: maxLines,
      overflow: overflow,
    );
  }
}

/// Responsive icon widget
class ResponsiveIcon extends StatelessWidget {
  final IconData icon;
  final Color? color;
  final double? size;

  const ResponsiveIcon(this.icon, {super.key, this.color, this.size});

  @override
  Widget build(BuildContext context) {
    final responsiveSize = ResponsiveService.responsiveIconSize(
      context,
      mobile: size ?? 24,
    );

    return Icon(icon, color: color, size: responsiveSize);
  }
}

/// Responsive button widget
class ResponsiveButton extends StatelessWidget {
  final Widget child;
  final VoidCallback? onPressed;
  final ButtonStyle? style;
  final EdgeInsets? padding;

  const ResponsiveButton({
    super.key,
    required this.child,
    required this.onPressed,
    this.style,
    this.padding,
  });

  @override
  Widget build(BuildContext context) {
    final responsivePadding = ResponsiveService.responsivePadding(
      context,
      mobile: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      tablet: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      desktop: const EdgeInsets.symmetric(horizontal: 32, vertical: 20),
    );

    return ElevatedButton(
      onPressed: onPressed,
      style: style?.copyWith(
        padding: WidgetStateProperty.all(padding ?? responsivePadding),
      ),
      child: child,
    );
  }
}

/// Responsive card widget
class ResponsiveCard extends StatelessWidget {
  final Widget child;
  final EdgeInsets? margin;
  final EdgeInsets? padding;
  final double? elevation;
  final Color? color;

  const ResponsiveCard({
    super.key,
    required this.child,
    this.margin,
    this.padding,
    this.elevation,
    this.color,
  });

  @override
  Widget build(BuildContext context) {
    final responsiveMargin = ResponsiveService.responsiveMargin(context);
    final responsivePadding = ResponsiveService.responsivePadding(context);

    return Card(
      margin: margin ?? responsiveMargin,
      elevation: elevation ?? 2,
      color: color,
      child: Padding(padding: padding ?? responsivePadding, child: child),
    );
  }
}

/// Responsive app bar widget
class ResponsiveAppBar extends StatelessWidget implements PreferredSizeWidget {
  final String? title;
  final List<Widget>? actions;
  final Widget? leading;
  final bool centerTitle;
  final double? elevation;

  const ResponsiveAppBar({
    super.key,
    this.title,
    this.actions,
    this.leading,
    this.centerTitle = true,
    this.elevation,
  });

  @override
  Widget build(BuildContext context) {
    final responsiveElevation = ResponsiveService.responsive(
      context,
      mobile: 0.0,
      tablet: 1.0,
      desktop: 2.0,
    );

    return AppBar(
      title: title != null
          ? ResponsiveText(
              title!,
              style: Theme.of(context).textTheme.titleLarge,
            )
          : null,
      actions: actions,
      leading: leading,
      centerTitle: centerTitle,
      elevation: elevation ?? responsiveElevation,
    );
  }

  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight);
}

/// Responsive bottom navigation bar
class ResponsiveBottomNavigationBar extends StatelessWidget {
  final int currentIndex;
  final ValueChanged<int>? onTap;
  final List<BottomNavigationBarItem> items;
  final BottomNavigationBarType? type;

  const ResponsiveBottomNavigationBar({
    super.key,
    required this.currentIndex,
    required this.onTap,
    required this.items,
    this.type,
  });

  @override
  Widget build(BuildContext context) {
    final responsiveType = ResponsiveService.responsive(
      context,
      mobile: BottomNavigationBarType.fixed,
      tablet: BottomNavigationBarType.fixed,
      desktop: BottomNavigationBarType.fixed,
    );

    return BottomNavigationBar(
      currentIndex: currentIndex,
      onTap: onTap,
      items: items,
      type: type ?? responsiveType,
    );
  }
}
