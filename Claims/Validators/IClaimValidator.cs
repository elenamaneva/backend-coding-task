namespace Claims.Validators;

public interface IClaimValidator
{
    Task<(bool IsValid, string? Error)> ValidateAsync(Claim claim);
}
