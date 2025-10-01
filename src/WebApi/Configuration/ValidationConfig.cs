using FluentValidation;
using Getir.Application.DTO;
using Getir.Application.Validators;

namespace Getir.WebApi.Configuration;

public static class ValidationConfig
{
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
        // Register validators
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RefreshTokenRequest>, RefreshTokenRequestValidator>();
        services.AddScoped<IValidator<CreateMerchantRequest>, CreateMerchantRequestValidator>();
        services.AddScoped<IValidator<UpdateMerchantRequest>, UpdateMerchantRequestValidator>();
        services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
        services.AddScoped<IValidator<UpdateProductRequest>, UpdateProductRequestValidator>();
        services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
        services.AddScoped<IValidator<CreateAddressRequest>, CreateAddressRequestValidator>();
        services.AddScoped<IValidator<UpdateAddressRequest>, UpdateAddressRequestValidator>();
        services.AddScoped<IValidator<AddToCartRequest>, AddToCartRequestValidator>();
        services.AddScoped<IValidator<UpdateCartItemRequest>, UpdateCartItemRequestValidator>();
        services.AddScoped<IValidator<CreateCouponRequest>, CreateCouponRequestValidator>();
        services.AddScoped<IValidator<CreateProductCategoryRequest>, CreateProductCategoryRequestValidator>();
        services.AddScoped<IValidator<UpdateProductCategoryRequest>, UpdateProductCategoryRequestValidator>();
        services.AddScoped<IValidator<CreateServiceCategoryRequest>, CreateServiceCategoryRequestValidator>();
        services.AddScoped<IValidator<UpdateServiceCategoryRequest>, UpdateServiceCategoryRequestValidator>();
        services.AddScoped<IValidator<CreateWorkingHoursRequest>, CreateWorkingHoursRequestValidator>();
        services.AddScoped<IValidator<UpdateWorkingHoursRequest>, UpdateWorkingHoursRequestValidator>();
        services.AddScoped<IValidator<BulkUpdateWorkingHoursRequest>, BulkUpdateWorkingHoursRequestValidator>();
        services.AddScoped<IValidator<CreateDeliveryZoneRequest>, CreateDeliveryZoneRequestValidator>();
        services.AddScoped<IValidator<UpdateDeliveryZoneRequest>, UpdateDeliveryZoneRequestValidator>();
        services.AddScoped<IValidator<CheckDeliveryZoneRequest>, CheckDeliveryZoneRequestValidator>();
        services.AddScoped<IValidator<CreateMerchantOnboardingRequest>, CreateMerchantOnboardingRequestValidator>();
        services.AddScoped<IValidator<UpdateOnboardingStepRequest>, UpdateOnboardingStepRequestValidator>();
        services.AddScoped<IValidator<CompleteOnboardingRequest>, CompleteOnboardingRequestValidator>();
        services.AddScoped<IValidator<ApproveMerchantRequest>, ApproveMerchantRequestValidator>();
        services.AddScoped<IValidator<UpdateProductStockRequest>, UpdateProductStockRequestValidator>();
        services.AddScoped<IValidator<UpdateProductOrderRequest>, UpdateProductOrderRequestValidator>();
        services.AddScoped<IValidator<BulkUpdateProductOrderRequest>, BulkUpdateProductOrderRequestValidator>();
        services.AddScoped<IValidator<ToggleProductAvailabilityRequest>, ToggleProductAvailabilityRequestValidator>();
        services.AddScoped<IValidator<RejectOrderRequest>, RejectOrderRequestValidator>();
        services.AddScoped<IValidator<CancelOrderRequest>, CancelOrderRequestValidator>();
        services.AddScoped<IValidator<UpdateOrderStatusRequest>, UpdateOrderStatusRequestValidator>();
        services.AddScoped<IValidator<CreateProductOptionGroupRequest>, CreateProductOptionGroupRequestValidator>();
        services.AddScoped<IValidator<UpdateProductOptionGroupRequest>, UpdateProductOptionGroupRequestValidator>();
        services.AddScoped<IValidator<CreateProductOptionRequest>, CreateProductOptionRequestValidator>();
        services.AddScoped<IValidator<UpdateProductOptionRequest>, UpdateProductOptionRequestValidator>();
        services.AddScoped<IValidator<BulkCreateProductOptionsRequest>, BulkCreateProductOptionsRequestValidator>();
        services.AddScoped<IValidator<BulkUpdateProductOptionsRequest>, BulkUpdateProductOptionsRequestValidator>();
        services.AddScoped<IValidator<CreateOrderLineOptionRequest>, CreateOrderLineOptionRequestValidator>();

        return services;
    }
}
