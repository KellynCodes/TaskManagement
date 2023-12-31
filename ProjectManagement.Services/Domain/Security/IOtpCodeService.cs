﻿using ProjectManagement.Models.Domains.Security.Enums;

namespace ProjectManagement.Services.Domains.Security;

/// <summary>
/// Generates and validates otp codes
/// </summary>
public interface IOtpCodeService
{
    /// <summary>
    /// Generates a time based OTP for a given operation
    /// </summary>
    /// <param name="userId">the user whose account is under consideration</param>
    /// <param name="otp">The otp code entered by the user</param>
    /// <param name="operation">The operation for which we are attempting to validate via 2FA</param>
    /// <returns></returns>
    Task<string> GenerateOtpAsync(string userId, NotificationType operation);

    /// <summary>
    /// Verifies the validity of the generated OTP code
    /// </summary>
    /// <param name="userId">the user whose account is under consideration</param>
    /// <param name="otp">The otp code entered by the user</param>
    /// <param name="operation">The operation for which we are attempting to validate via 2FA</param>
    /// <returns></returns>
    Task<bool> VerifyOtpAsync(string userId, string otp, NotificationType operation);
}