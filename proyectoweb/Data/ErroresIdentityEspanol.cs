using Microsoft.AspNetCore.Identity;

namespace proyectoweb.Data
{
    public class ErroresIdentityEspanol : IdentityErrorDescriber
    {
        private static IdentityError Error(string codigo, string descripcion)
        {
            return new IdentityError { Code = codigo, Description = descripcion };
        }

        public override IdentityError DuplicateUserName(string userName) =>
            Error(nameof(DuplicateUserName), $"El correo '{userName}' ya está registrado.");

        public override IdentityError DuplicateEmail(string email) =>
            Error(nameof(DuplicateEmail), $"El correo '{email}' ya está registrado.");

        public override IdentityError InvalidEmail(string? email) =>
            Error(nameof(InvalidEmail), "El correo no es válido.");

        public override IdentityError InvalidUserName(string? userName) =>
            Error(nameof(InvalidUserName), "El correo no es válido.");

        public override IdentityError PasswordTooShort(int length) =>
            Error(nameof(PasswordTooShort), $"La contraseña debe tener al menos {length} caracteres.");

        public override IdentityError PasswordRequiresDigit() =>
            Error(nameof(PasswordRequiresDigit), "La contraseña debe tener al menos un número.");

        public override IdentityError PasswordRequiresLower() =>
            Error(nameof(PasswordRequiresLower), "La contraseña debe tener al menos una letra minúscula.");

        public override IdentityError PasswordRequiresUpper() =>
            Error(nameof(PasswordRequiresUpper), "La contraseña debe tener al menos una letra mayúscula.");

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            Error(nameof(PasswordRequiresNonAlphanumeric), "La contraseña debe tener al menos un símbolo, por ejemplo # o !.");

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            Error(nameof(PasswordRequiresUniqueChars), $"La contraseña debe tener al menos {uniqueChars} caracteres distintos.");
    }
}
