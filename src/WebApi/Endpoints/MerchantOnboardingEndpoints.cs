using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.Merchants;
using Getir.Application.DTO;
using Getir.Application.Common;
using System.Security.Claims;
using Getir.WebApi.Extensions;

namespace Getir.WebApi.Endpoints;

public static class MerchantOnboardingEndpoints
{
    public static void MapMerchantOnboardingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/merchants/{merchantId:guid}/onboarding")
            .WithTags("Merchant Onboarding");

        // Get onboarding status
        group.MapGet("/", async (
            [FromRoute] Guid merchantId,
            ClaimsPrincipal user,
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetOnboardingStatusAsync(merchantId, ct);
            return result.ToIResult();
        })
        .WithName("GetOnboardingStatus")
        .Produces<MerchantOnboardingResponse>(200)
        .Produces(404)
        .RequireAuthorization();

        // Get onboarding progress
        group.MapGet("/progress", async (
            [FromRoute] Guid merchantId,
            ClaimsPrincipal user,
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetOnboardingProgressAsync(merchantId, ct);
            return result.ToIResult();
        })
        .WithName("GetOnboardingProgress")
        .Produces<OnboardingProgressResponse>(200)
        .Produces(404)
        .RequireAuthorization();

        // Get onboarding steps
        group.MapGet("/steps", async (
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct) =>
        {
            var result = await service.GetOnboardingStepsAsync(ct);
            return result.ToIResult();
        })
        .WithName("GetOnboardingSteps")
        .Produces<List<OnboardingStepResponse>>(200)
        .RequireAuthorization();

        // Create onboarding
        group.MapPost("/", async (
            [FromRoute] Guid merchantId,
            ClaimsPrincipal user,
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var request = new CreateMerchantOnboardingRequest(merchantId, userId);
            var result = await service.CreateOnboardingAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateOnboarding")
        .Produces<MerchantOnboardingResponse>(201)
        .Produces(400)
        .Produces(409)
        .RequireAuthorization();

        // Update onboarding step
        group.MapPut("/steps", async (
            [FromRoute] Guid merchantId,
            [FromBody] UpdateOnboardingStepRequest request,
            ClaimsPrincipal user,
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateOnboardingStepAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateOnboardingStep")
        .Produces<MerchantOnboardingResponse>(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization();

        // Complete onboarding
        group.MapPost("/complete", async (
            [FromRoute] Guid merchantId,
            ClaimsPrincipal user,
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            // Get onboarding ID first
            var statusResult = await service.GetOnboardingStatusAsync(merchantId, ct);
            if (!statusResult.Success)
            {
                return statusResult.ToIResult();
            }

            var request = new CompleteOnboardingRequest(statusResult.Value!.Id);
            var result = await service.CompleteOnboardingAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CompleteOnboarding")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization();

        // Check if merchant can start trading
        group.MapGet("/can-start-trading", async (
            [FromRoute] Guid merchantId,
            ClaimsPrincipal user,
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.CanMerchantStartTradingAsync(merchantId, ct);
            return result.ToIResult();
        })
        .WithName("CanMerchantStartTrading")
        .Produces<bool>(200)
        .Produces(404)
        .RequireAuthorization();

        // Admin endpoints
        var adminGroup = app.MapGroup("/api/v1/admin/merchants/onboarding")
            .WithTags("Admin - Merchant Onboarding")
            .RequireAuthorization("Admin");

        // Get pending approvals
        adminGroup.MapGet("/pending", async (
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var paginationQuery = new PaginationQuery { Page = page, PageSize = pageSize };
            var query = new GetPendingApprovalsQuery(paginationQuery);
            var result = await service.GetPendingApprovalsAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetPendingApprovals")
        .Produces<PagedResult<MerchantOnboardingResponse>>(200)
        .RequireAuthorization("Admin");

        // Approve/Reject merchant
        adminGroup.MapPost("/{onboardingId:guid}/approve", async (
            [FromRoute] Guid onboardingId,
            [FromBody] ApproveMerchantRequest request,
            [FromServices] IMerchantOnboardingService service,
            CancellationToken ct) =>
        {
            var result = await service.ApproveMerchantAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("ApproveMerchant")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("Admin");
    }
}
