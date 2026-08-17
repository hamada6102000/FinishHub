namespace test.Models;

/// <summary>
/// Behaviour bucket a user falls into. This is NOT the user-facing label — that lives in the
/// configurable <see cref="UserType"/> table and is chosen by the administrator. Each UserType
/// row carries the Kind that gets copied onto <see cref="User.UserType"/> whenever a user's type
/// is assigned, so the existing engineer/client rules keep working without a join.
/// </summary>
public enum UserTypeKind
{
    User = 1,
    Engineer = 2
}

public enum PropertyType
{
    Apartment,
    Villa,
    Office,
    Commercial,
    Other
}

public enum MediaType
{
    Image,
    Video
}

public enum DesignService
{
    InteriorDesign,
    FullFinishing,
    Architecture,
    Consultation
}
