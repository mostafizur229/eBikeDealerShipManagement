namespace eBike.Application.Features.Menus.DTOs;

public record MenuActionPermissionsDto(
    bool CanView,
    bool CanCreate,
    bool CanUpdate,
    bool CanDelete
);

public record NavigationItemDto(
    int Id,
    string Title,
    string? Url,
    string Icon,
    int Sequence,
    string Category,
    string Slug,
    MenuActionPermissionsDto Permissions
);

public record NavigationGroupDto(
    string Title,
    IReadOnlyList<NavigationItemDto> Items
);
