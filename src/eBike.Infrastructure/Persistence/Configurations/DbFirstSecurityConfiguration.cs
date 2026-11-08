using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Email).HasMaxLength(256);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        builder.HasOne(u => u.SisterConcern)
            .WithMany()
            .HasForeignKey(u => u.ConcernID)
            .IsRequired(false);

        builder.HasOne(u => u.Employee)
            .WithMany()
            .HasForeignKey(u => u.EmployeeID)
            .IsRequired(false);
    }
}

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(256);

        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId);

        builder.HasMany(r => r.RoleMenus)
            .WithOne(rm => rm.Role)
            .HasForeignKey(rm => rm.RoleId);
    }
}

public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
    }
}

public class ApplicationMenuItemConfiguration : IEntityTypeConfiguration<ApplicationMenuItem>
{
    public void Configure(EntityTypeBuilder<ApplicationMenuItem> builder)
    {
        builder.ToTable("MenuItems");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(256);
        builder.Property(m => m.Url).HasMaxLength(256);
        builder.Property(m => m.Icon).HasMaxLength(100);

        builder.HasOne(m => m.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(m => m.ParentId)
            .IsRequired(false);
    }
}

public class ApplicationRoleMenuConfiguration : IEntityTypeConfiguration<ApplicationRoleMenu>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleMenu> builder)
    {
        builder.ToTable("ApplicationRoleMenus");
        builder.HasKey(arm => new { arm.RoleId, arm.MenuId });

        builder.HasOne(arm => arm.Role)
            .WithMany(r => r.RoleMenus)
            .HasForeignKey(arm => arm.RoleId);

        builder.HasOne(arm => arm.Menu)
            .WithMany(m => m.RoleMenus)
            .HasForeignKey(arm => arm.MenuId);
    }
}

public class ApplicationPermissionConfiguration : IEntityTypeConfiguration<ApplicationPermission>
{
    public void Configure(EntityTypeBuilder<ApplicationPermission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
    }
}

public class ApplicationMenuPermissionConfiguration : IEntityTypeConfiguration<ApplicationMenuPermission>
{
    public void Configure(EntityTypeBuilder<ApplicationMenuPermission> builder)
    {
        builder.ToTable("MenuPermissions");
        builder.HasKey(mp => new { mp.RoleMenuId, mp.PermissionId });
    }
}

public class SisterConcernConfiguration : IEntityTypeConfiguration<SisterConcern>
{
    public void Configure(EntityTypeBuilder<SisterConcern> builder)
    {
        builder.ToTable("SisterConcerns");
        builder.HasKey(sc => sc.ConcernID);
    }
}
