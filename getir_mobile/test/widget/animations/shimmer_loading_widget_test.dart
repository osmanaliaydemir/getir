import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:getir_mobile/presentation/widgets/common/shimmer_loading.dart';

void main() {
  group('ShimmerLoading Widget Tests -', () {
    testWidgets('renders child widget', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: ShimmerLoading(child: Text('Test'))),
        ),
      );

      expect(find.text('Test'), findsOneWidget);
    });

    testWidgets('applies shimmer effect to child', (tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: ShimmerLoading(
              child: Container(width: 100, height: 100, color: Colors.grey),
            ),
          ),
        ),
      );

      expect(find.byType(ShimmerLoading), findsOneWidget);
    });

    testWidgets('uses custom colors when provided', (tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: ShimmerLoading(
              baseColor: Colors.red,
              highlightColor: Colors.blue,
              child: Container(width: 100, height: 100),
            ),
          ),
        ),
      );

      expect(find.byType(ShimmerLoading), findsOneWidget);
    });
  });

  group('ProductCardShimmer Widget Tests -', () {
    testWidgets('renders product shimmer structure', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: ProductCardShimmer())),
      );

      expect(find.byType(ProductCardShimmer), findsOneWidget);
      expect(find.byType(ShimmerLoading), findsWidgets);
    });

    testWidgets('has correct layout structure', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: ProductCardShimmer())),
      );

      // Row for horizontal layout
      expect(find.byType(Row), findsWidgets);
      // Container for card background
      expect(find.byType(Container), findsWidgets);
    });
  });

  group('MerchantCardShimmer Widget Tests -', () {
    testWidgets('renders merchant shimmer structure', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: MerchantCardShimmer())),
      );

      expect(find.byType(MerchantCardShimmer), findsOneWidget);
      expect(find.byType(ShimmerLoading), findsWidgets);
    });

    testWidgets('displays multiple shimmer placeholders', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: MerchantCardShimmer())),
      );

      // Should have multiple ShimmerLoading widgets
      expect(find.byType(ShimmerLoading), findsAtLeast(3));
    });
  });

  group('CartItemShimmer Widget Tests -', () {
    testWidgets('renders cart item shimmer structure', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: CartItemShimmer())),
      );

      expect(find.byType(CartItemShimmer), findsOneWidget);
      expect(find.byType(ShimmerLoading), findsWidgets);
    });

    testWidgets('has horizontal layout', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: CartItemShimmer())),
      );

      expect(find.byType(Row), findsWidgets);
    });
  });

  group('OrderItemShimmer Widget Tests -', () {
    testWidgets('renders order item shimmer structure', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: OrderItemShimmer())),
      );

      expect(find.byType(OrderItemShimmer), findsOneWidget);
      expect(find.byType(ShimmerLoading), findsWidgets);
    });

    testWidgets('displays multiple item placeholders', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: OrderItemShimmer())),
      );

      // Should generate 2 item shimmers
      final columns = tester.widgetList<Column>(find.byType(Column));
      expect(columns.length, greaterThan(0));
    });
  });

  group('ProfileSectionShimmer Widget Tests -', () {
    testWidgets('renders profile shimmer with avatar', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: ProfileSectionShimmer())),
      );

      expect(find.byType(ProfileSectionShimmer), findsOneWidget);
      expect(find.byType(ShimmerLoading), findsWidgets);
    });

    testWidgets('displays circular avatar shimmer', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: ProfileSectionShimmer())),
      );

      // Check for circular container (avatar)
      final containers = tester.widgetList<Container>(find.byType(Container));
      final circularContainers = containers.where((c) {
        final decoration = c.decoration;
        if (decoration is BoxDecoration) {
          return decoration.shape == BoxShape.circle;
        }
        return false;
      });

      expect(circularContainers.length, greaterThan(0));
    });

    testWidgets('displays form field shimmers', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: ProfileSectionShimmer())),
      );

      // Should have multiple shimmer placeholders
      expect(find.byType(ShimmerLoading), findsAtLeast(5));
    });
  });

  group('ListItemShimmer Widget Tests -', () {
    testWidgets('renders with default height', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: ListItemShimmer())),
      );

      expect(find.byType(ListItemShimmer), findsOneWidget);
    });

    testWidgets('uses custom height when provided', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(home: Scaffold(body: ListItemShimmer(height: 100))),
      );

      final shimmer = tester.widget<ListItemShimmer>(
        find.byType(ListItemShimmer),
      );

      expect(shimmer.height, 100);
    });

    testWidgets('uses custom padding when provided', (tester) async {
      const customPadding = EdgeInsets.all(20);

      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: ListItemShimmer(padding: customPadding)),
        ),
      );

      final shimmer = tester.widget<ListItemShimmer>(
        find.byType(ListItemShimmer),
      );

      expect(shimmer.padding, customPadding);
    });
  });
}
