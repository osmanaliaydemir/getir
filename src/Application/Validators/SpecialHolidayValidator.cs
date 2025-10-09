using FluentValidation;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateSpecialHolidayRequestValidator : AbstractValidator<CreateSpecialHolidayRequest>
{
    public CreateSpecialHolidayRequestValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty()
            .WithMessage("Merchant ID boş olamaz");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Başlık boş olamaz")
            .MaximumLength(200)
            .WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Açıklama en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Başlangıç tarihi boş olamaz");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("Bitiş tarihi boş olamaz")
            .GreaterThan(x => x.StartDate)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");

        RuleFor(x => x)
            .Must(x => x.EndDate.Subtract(x.StartDate).TotalDays <= 365)
            .WithMessage("Özel tatil süresi 365 günü geçemez")
            .When(x => x.EndDate > x.StartDate);

        // Özel açılış saatleri validasyonu
        RuleFor(x => x.SpecialOpenTime)
            .NotNull()
            .WithMessage("Kapalı değilse, özel açılış saati belirtilmelidir")
            .When(x => !x.IsClosed);

        RuleFor(x => x.SpecialCloseTime)
            .NotNull()
            .WithMessage("Kapalı değilse, özel kapanış saati belirtilmelidir")
            .When(x => !x.IsClosed);

        RuleFor(x => x)
            .Must(x => x.SpecialCloseTime > x.SpecialOpenTime || 
                      (x.SpecialCloseTime < x.SpecialOpenTime)) // Gece yarısını geçen durumlar için
            .WithMessage("Geçerli açılış ve kapanış saatleri belirtilmelidir")
            .When(x => !x.IsClosed && x.SpecialOpenTime.HasValue && x.SpecialCloseTime.HasValue);

        // Kapalıysa özel saatler null olmalı
        RuleFor(x => x.SpecialOpenTime)
            .Null()
            .WithMessage("Kapalı olarak işaretlendiyse özel açılış saati belirtilmemelidir")
            .When(x => x.IsClosed);

        RuleFor(x => x.SpecialCloseTime)
            .Null()
            .WithMessage("Kapalı olarak işaretlendiyse özel kapanış saati belirtilmemelidir")
            .When(x => x.IsClosed);
    }
}

public class UpdateSpecialHolidayRequestValidator : AbstractValidator<UpdateSpecialHolidayRequest>
{
    public UpdateSpecialHolidayRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Başlık boş olamaz")
            .MaximumLength(200)
            .WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Açıklama en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Başlangıç tarihi boş olamaz");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("Bitiş tarihi boş olamaz")
            .GreaterThan(x => x.StartDate)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");

        RuleFor(x => x)
            .Must(x => x.EndDate.Subtract(x.StartDate).TotalDays <= 365)
            .WithMessage("Özel tatil süresi 365 günü geçemez")
            .When(x => x.EndDate > x.StartDate);

        // Özel açılış saatleri validasyonu
        RuleFor(x => x.SpecialOpenTime)
            .NotNull()
            .WithMessage("Kapalı değilse, özel açılış saati belirtilmelidir")
            .When(x => !x.IsClosed);

        RuleFor(x => x.SpecialCloseTime)
            .NotNull()
            .WithMessage("Kapalı değilse, özel kapanış saati belirtilmelidir")
            .When(x => !x.IsClosed);

        RuleFor(x => x)
            .Must(x => x.SpecialCloseTime > x.SpecialOpenTime || 
                      (x.SpecialCloseTime < x.SpecialOpenTime))
            .WithMessage("Geçerli açılış ve kapanış saatleri belirtilmelidir")
            .When(x => !x.IsClosed && x.SpecialOpenTime.HasValue && x.SpecialCloseTime.HasValue);

        // Kapalıysa özel saatler null olmalı
        RuleFor(x => x.SpecialOpenTime)
            .Null()
            .WithMessage("Kapalı olarak işaretlendiyse özel açılış saati belirtilmemelidir")
            .When(x => x.IsClosed);

        RuleFor(x => x.SpecialCloseTime)
            .Null()
            .WithMessage("Kapalı olarak işaretlendiyse özel kapanış saati belirtilmemelidir")
            .When(x => x.IsClosed);
    }
}

