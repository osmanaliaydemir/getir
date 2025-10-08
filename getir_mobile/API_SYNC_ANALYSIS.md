# 🔴 Backend-Mobile API Synchronization - ACTION ITEMS

**Date:** 8 Ekim 2025  
**Backend:** .NET WebApi (ajilgo.runasp.net)  
**Mobile:** Flutter (Dio client)

---

## 📊 Executive Summary

**Status:** 🟡 **96% SYNCHRONIZED**

**Working Perfectly:** 10/11 modules  
**Issues Found:** 1 CRITICAL mismatch  
**Action Required:** Add missing password recovery endpoints

---

## 🔴 CRITICAL ISSUE: Password Recovery Missing

### Problem: Forgot/Reset Password Endpoints Do NOT Exist

**Mobile Implementation (READY):**
```dart
// auth_datasource_impl.dart - Lines 136-169
POST /api/v1/Auth/forgot-password
POST /api/v1/Auth/reset-password

// Mobile has full implementation:
✅ AuthService.forgotPassword(email)
✅ AuthService.resetPassword(email, resetCode, newPassword)
✅ AuthBloc events and states
✅ UI screens ready
✅ Validation logic ready
```

**Backend Status:**
```csharp
// AuthController.cs - MISSING!
❌ [HttpPost("forgot-password")] - DOES NOT EXIST
❌ [HttpPost("reset-password")] - DOES NOT EXIST

Existing endpoints:
✅ POST /api/v1/Auth/register
✅ POST /api/v1/Auth/login
✅ POST /api/v1/Auth/refresh
✅ POST /api/v1/Auth/logout
```

**Impact:** 🔴 **CRITICAL**
- Password recovery feature will fail (404 error)
- Users cannot reset forgotten passwords
- Mobile UI will show error state
- Feature is incomplete

---

## 🎯 REQUIRED BACKEND CHANGES

### Add to AuthController.cs

```csharp
/// <summary>
/// Initiate password reset flow - sends reset code to email
/// </summary>
[HttpPost("forgot-password")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordRequest request,
    CancellationToken ct = default)
{
    var validationResult = HandleValidationErrors();
    if (validationResult != null) return validationResult;

    var result = await _authService.ForgotPasswordAsync(request, ct);
    if (result.Success)
    {
        return NoContent();
    }
    return ToActionResult(result);
}

/// <summary>
/// Complete password reset with reset code
/// </summary>
[HttpPost("reset-password")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ResetPassword(
    [FromBody] ResetPasswordRequest request,
    CancellationToken ct = default)
{
    var validationResult = HandleValidationErrors();
    if (validationResult != null) return validationResult;

    var result = await _authService.ResetPasswordAsync(request, ct);
    if (result.Success)
    {
        return NoContent();
    }
    return ToActionResult(result);
}
```

### Required DTOs

```csharp
// Application/DTO/Auth/ForgotPasswordRequest.cs
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

// Application/DTO/Auth/ResetPasswordRequest.cs
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string ResetCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
```

### Required Service Methods

```csharp
// Application/Services/Auth/IAuthService.cs
Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct);
Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct);
```

---

## 📋 Implementation Checklist

### Backend Tasks (2-3 hours)

- [ ] Create ForgotPasswordRequest DTO
- [ ] Create ResetPasswordRequest DTO
- [ ] Add ForgotPasswordAsync to IAuthService
- [ ] Add ResetPasswordAsync to IAuthService
- [ ] Implement password reset logic:
  - [ ] Generate reset code (6-digit or GUID)
  - [ ] Store reset code with expiration (e.g., 15 minutes)
  - [ ] Send email with reset code
  - [ ] Validate reset code
  - [ ] Update password
- [ ] Add ForgotPassword endpoint to AuthController
- [ ] Add ResetPassword endpoint to AuthController
- [ ] Add validation
- [ ] Add unit tests
- [ ] Test with Postman
- [ ] Deploy to ajilgo.runasp.net

### Mobile Tasks (Nothing - Already Done!)

```
✅ UI screens implemented
✅ BLoC events/states ready
✅ Service methods ready
✅ Datasource ready
✅ Repository ready
✅ Validation logic ready

Mobile is WAITING for backend!
```

---

## 🟢 Alternative: Disable Feature in Mobile

**If you don't want to implement password recovery now:**

### Mobile Changes Required

```dart
// 1. Comment out in AuthService
// Future<Result<void>> forgotPassword(String email) async { ... }
// Future<Result<void>> resetPassword(...) async { ... }

// 2. Comment out in AuthRepository
// Future<Result<void>> forgotPassword(String email);
// Future<Result<void>> resetPassword(...);

// 3. Remove from AuthBloc
// on<AuthForgotPasswordRequested>
// on<AuthResetPasswordRequested>

// 4. Hide UI
// Remove "Forgot Password?" link from login screen
```

**Time:** 15 minutes  
**Impact:** Feature disabled, users can't recover passwords

---

## 📊 Sync Status After Fix

### If Backend Implemented:
```
✅ Authentication: 100% (6/6 endpoints)
✅ Cart: 100%
✅ Order: 100%
✅ Product: 100%
✅ Merchant: 100%
✅ Address: 100%
✅ Profile: 100%
✅ Reviews: 100%
✅ Working Hours: 100%
✅ Notifications: 100%

Overall: 100% ✅ PERFECT SYNC!
```

### If Feature Disabled in Mobile:
```
✅ Authentication: 100% (4/4 endpoints)
   (forgot/reset removed)
✅ All other modules: 100%

Overall: 100% ✅ PERFECT SYNC!
```

---

## 💡 RECOMMENDATION

### Option 1: Implement in Backend (RECOMMENDED) 🎯

**Pros:**
- Feature complete
- User experience improved
- Production-ready

**Cons:**
- 2-3 hours backend work
- Need email service

**Timeline:** This week

---

### Option 2: Disable in Mobile

**Pros:**
- Quick (15 minutes)
- No backend work

**Cons:**
- Feature incomplete
- Poor user experience
- Users can't recover passwords

**Timeline:** Now

---

## 🎯 Action Plan

### Recommended: Backend Implementation

**Week 1 (This Week):**
1. Implement ForgotPassword/ResetPassword in backend (2-3 hours)
2. Test with Postman (30 minutes)
3. Deploy to ajilgo.runasp.net (15 minutes)
4. Test from mobile app (15 minutes)

**Total Time:** 3-4 hours

**Result:** 100% API sync, feature complete ✅

---

## 📈 Current Sync Quality

| Module | Endpoints | Sync Rate |
|--------|-----------|-----------|
| Auth (Core) | 4/4 | 100% ✅ |
| Auth (Password) | 0/2 | 0% ❌ |
| Cart | 7/7 | 100% ✅ |
| Order | 4/4 | 100% ✅ |
| Product | 3/3 | 100% ✅ |
| Merchant | 4/4 | 100% ✅ |
| Address | 6/6 | 100% ✅ |
| Profile | 2/2 | 100% ✅ |
| Reviews | 3/3 | 100% ✅ |
| Working Hours | 2/2 | 100% ✅ |
| Notifications | 3/3 | 100% ✅ |
| **TOTAL** | **38/40** | **96%** |

---

## 🎉 Conclusion

**Backend ve Mobile NEREDEYSE PERFECT senkronize!**

**Tek eksik:**
- Forgot/Reset Password (2 endpoint)

**Karar ver:**
1. 🚀 Backend'e ekle → %100 sync
2. 🔧 Mobile'dan kaldır → %100 sync (ama feature eksik)

**Önerim:** Backend'e ekle, 3 saat sürer, feature tamamlanır! 🎯

---

*Mobile kodu HAZIR ve BEKLİYOR! Backend endpoint'leri eklersen hemen çalışır!*