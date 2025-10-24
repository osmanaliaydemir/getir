using FluentValidation;
using WebApp.Models;

namespace WebApp.Models.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi gereklidir")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin")
            .MaximumLength(256).WithMessage("E-posta adresi çok uzun");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre gereklidir")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Şifre çok uzun");
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi gereklidir")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin")
            .MaximumLength(256).WithMessage("E-posta adresi çok uzun");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre gereklidir")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Şifre çok uzun")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Şifre en az bir küçük harf, bir büyük harf ve bir rakam içermelidir");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad gereklidir")
            .MaximumLength(50).WithMessage("Ad çok uzun")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad sadece harf içerebilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad gereklidir")
            .MaximumLength(50).WithMessage("Soyad çok uzun")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Soyad sadece harf içerebilir");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+90|0)?[5][0-9]{9}$").WithMessage("Geçerli bir telefon numarası girin")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi gereklidir")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin");
    }
}

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID gereklidir");

        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Mağaza ID gereklidir");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Miktar çok fazla");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notlar çok uzun")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Miktar çok fazla");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notlar çok uzun")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Mağaza ID gereklidir");

        RuleFor(x => x.DeliveryAddressId)
            .NotEmpty().WithMessage("Teslimat adresi gereklidir");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Ödeme yöntemi gereklidir")
            .Must(method => new[] { "Cash", "CreditCard", "DebitCard", "Online" }.Contains(method))
            .WithMessage("Geçerli bir ödeme yöntemi seçin");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sipariş öğeleri gereklidir")
            .Must(items => items.Count > 0).WithMessage("En az bir ürün seçmelisiniz");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notlar çok uzun")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID gereklidir");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Miktar çok fazla");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Birim fiyat 0'dan büyük olmalıdır");
    }
}

public class AddAddressRequestValidator : AbstractValidator<AddAddressRequest>
{
    public AddAddressRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Adres başlığı gereklidir")
            .MaximumLength(100).WithMessage("Adres başlığı çok uzun");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Adres satırı 1 gereklidir")
            .MaximumLength(200).WithMessage("Adres satırı çok uzun");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir gereklidir")
            .MaximumLength(50).WithMessage("Şehir adı çok uzun");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("İlçe gereklidir")
            .MaximumLength(50).WithMessage("İlçe adı çok uzun");

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{5}$").WithMessage("Posta kodu 5 haneli olmalıdır");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ülke gereklidir")
            .MaximumLength(50).WithMessage("Ülke adı çok uzun");

        RuleFor(x => x.Instructions)
            .MaximumLength(500).WithMessage("Talimatlar çok uzun")
            .When(x => !string.IsNullOrEmpty(x.Instructions));
    }
}

public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Adres ID gereklidir");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Adres başlığı gereklidir")
            .MaximumLength(100).WithMessage("Adres başlığı çok uzun");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Adres satırı 1 gereklidir")
            .MaximumLength(200).WithMessage("Adres satırı çok uzun");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir gereklidir")
            .MaximumLength(50).WithMessage("Şehir adı çok uzun");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("İlçe gereklidir")
            .MaximumLength(50).WithMessage("İlçe adı çok uzun");

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{5}$").WithMessage("Posta kodu 5 haneli olmalıdır");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ülke gereklidir")
            .MaximumLength(50).WithMessage("Ülke adı çok uzun");

        RuleFor(x => x.Instructions)
            .MaximumLength(500).WithMessage("Talimatlar çok uzun")
            .When(x => !string.IsNullOrEmpty(x.Instructions));
    }
}

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad gereklidir")
            .MaximumLength(50).WithMessage("Ad çok uzun")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad sadece harf içerebilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad gereklidir")
            .MaximumLength(50).WithMessage("Soyad çok uzun")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Soyad sadece harf içerebilir");

        RuleFor(x => x.Phone)
            .Matches(@"^(\+90|0)?[5][0-9]{9}$").WithMessage("Geçerli bir telefon numarası girin")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.Now.AddYears(-13)).WithMessage("Yaşınız en az 13 olmalıdır")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Geçerli bir doğum tarihi girin");

        RuleFor(x => x.Gender)
            .Must(gender => new[] { "Male", "Female", "Other" }.Contains(gender))
            .WithMessage("Geçerli bir cinsiyet seçin");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre gereklidir");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre gereklidir")
            .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Yeni şifre çok uzun")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Yeni şifre en az bir küçük harf, bir büyük harf ve bir rakam içermelidir")
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre mevcut şifre ile aynı olamaz");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre onayı gereklidir")
            .Equal(x => x.NewPassword).WithMessage("Şifre onayı yeni şifre ile eşleşmiyor");
    }
}
