using Microsoft.AspNetCore.Identity;

namespace Homework_portal.Utility
{
    // Identity hata mesajlarýný Türkçeleþtirmek için özel tanýmlayýcý
    public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateUserName(string userName)
            => new IdentityError { Code = nameof(DuplicateUserName), Description = $"Bu kullanýcý adý zaten kullanýmda: {userName}" };

        public override IdentityError DuplicateEmail(string email)
            => new IdentityError { Code = nameof(DuplicateEmail), Description = $"Bu e-posta adresi zaten kayýtlý: {email}" };

        public override IdentityError InvalidUserName(string? userName)
            => new IdentityError { Code = nameof(InvalidUserName), Description = "Geçersiz kullanýcý adý." };

        public override IdentityError InvalidEmail(string? email)
            => new IdentityError { Code = nameof(InvalidEmail), Description = "Geçersiz e-posta adresi." };

        public override IdentityError PasswordTooShort(int length)
            => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Þifre en az {length} karakter olmalýdýr." };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Þifre en az bir özel karakter içermelidir (örn: !@#$%^&*)." };

        public override IdentityError PasswordRequiresDigit()
            => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Þifre en az bir rakam içermelidir (0-9)." };

        public override IdentityError PasswordRequiresLower()
            => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Þifre en az bir küçük harf içermelidir (a-z)." };

        public override IdentityError PasswordRequiresUpper()
            => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Þifre en az bir büyük harf içermelidir (A-Z)." };

        public override IdentityError PasswordMismatch()
            => new IdentityError { Code = nameof(PasswordMismatch), Description = "Geçersiz þifre." };

        public override IdentityError UserAlreadyHasPassword()
            => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "Kullanýcýnýn zaten bir þifresi var." };

        public override IdentityError UserLockoutNotEnabled()
            => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Kullanýcý için kilitleme etkin deðil." };

        public override IdentityError ConcurrencyFailure()
            => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Eþzamanlýlýk hatasý oluþtu, lütfen tekrar deneyin." };

        public override IdentityError DefaultError()
            => new IdentityError { Code = nameof(DefaultError), Description = "Bilinmeyen bir hata oluþtu." };
    }
}
