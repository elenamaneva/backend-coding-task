namespace Claims.Validators;

public interface ICoverValidator
{
    (bool IsValid, string? Error) Validate(Cover cover);
}
