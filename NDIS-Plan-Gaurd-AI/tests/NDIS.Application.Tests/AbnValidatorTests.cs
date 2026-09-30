using NDIS.Application.Common.Validation;

namespace NDIS.Application.Tests;

public class AbnValidatorTests
{
    [Fact]
    public void Valid_abn_passes_checksum() => Assert.True(AbnValidator.IsValid("51824753556"));

    [Fact]
    public void Invalid_abn_fails_checksum() => Assert.False(AbnValidator.IsValid("12345678901"));
}
