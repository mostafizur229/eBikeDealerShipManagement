using System.Reflection;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Common;
using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace eBike.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUser? _currentUser;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
    public DbSet<ApplicationUserRole> UserRoles => Set<ApplicationUserRole>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public DbSet<ApplicationMenuItem> MenuItems => Set<ApplicationMenuItem>();
    public DbSet<ApplicationRoleMenu> ApplicationRoleMenus => Set<ApplicationRoleMenu>();
    public DbSet<ApplicationPermission> Permissions => Set<ApplicationPermission>();
    public DbSet<ApplicationMenuPermission> MenuPermissions => Set<ApplicationMenuPermission>();
    public DbSet<SisterConcern> SisterConcerns => Set<SisterConcern>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PLocation> PLocations => Set<PLocation>();
    public DbSet<Godown> Godowns => Set<Godown>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Thana> Thanas => Set<Thana>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Religion> Religions => Set<Religion>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<ExpenseItem> ExpenseItems => Set<ExpenseItem>();
    public DbSet<Expenditure> Expenditures => Set<Expenditure>();
    public DbSet<CashCollection> CashCollections => Set<CashCollection>();
    public DbSet<POrder> POrders => Set<POrder>();
    public DbSet<POrderDetail> POrderDetails => Set<POrderDetail>();
    public DbSet<POProductDetail> POProductDetails => Set<POProductDetail>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockDetail> StockDetails => Set<StockDetail>();
    public DbSet<SOrder> SOrders => Set<SOrder>();
    public DbSet<SOrderDetail> SOrderDetails => Set<SOrderDetail>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<BRTARate> BRTARates => Set<BRTARate>();
    public DbSet<CreditSale> CreditSales => Set<CreditSale>();
    public DbSet<CreditSaleDetail> CreditSaleDetails => Set<CreditSaleDetail>();
    public DbSet<CreditSalesSchedule> CreditSalesSchedules => Set<CreditSalesSchedule>();
    public DbSet<CreditInterestPercentage> CreditInterestPercentages => Set<CreditInterestPercentage>();
    public DbSet<PurchaseDO> DOes => Set<PurchaseDO>();
    public DbSet<PurchaseDODetail> DODetails => Set<PurchaseDODetail>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<TransferDetail> TransferDetails => Set<TransferDetail>();
    public DbSet<TransferHistory> TransferHistories => Set<TransferHistory>();
    public DbSet<SystemInformation> SystemInformations => Set<SystemInformation>();

    // Inventory & Service Management Tables
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<EBikeMaster> EBikes => Set<EBikeMaster>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerVehicle> CustomerVehicles => Set<CustomerVehicle>();
    public DbSet<ServiceFollowUpLog> ServiceFollowUpLogs => Set<ServiceFollowUpLog>();
    public DbSet<JobCard> JobCards => Set<JobCard>();
    public DbSet<JobCardLaborDetail> JobCardLaborDetails => Set<JobCardLaborDetail>();
    public DbSet<JobCardPartsDetail> JobCardPartsDetails => Set<JobCardPartsDetail>();
    public DbSet<VehiclePeriodicChecklist> VehiclePeriodicChecklists => Set<VehiclePeriodicChecklist>();
    public DbSet<ServiceTechnician> ServiceTechnicians => Set<ServiceTechnician>();
    public DbSet<ServiceTicket> ServiceTickets => Set<ServiceTicket>();
    public DbSet<ServiceTicketItem> ServiceTicketItems => Set<ServiceTicketItem>();
    public DbSet<WorkshopBay> WorkshopBays => Set<WorkshopBay>();
    public DbSet<WorkshopTimeSlot> WorkshopTimeSlots => Set<WorkshopTimeSlot>();
    public DbSet<WorkshopChecklistMaster> WorkshopChecklistMasters => Set<WorkshopChecklistMaster>();
    public DbSet<CustomerServiceRating> CustomerServiceRatings => Set<CustomerServiceRating>();
    public DbSet<PostServiceCallFeedback> PostServiceCallFeedbacks => Set<PostServiceCallFeedback>();
    public DbSet<SaleOffer> SaleOffers => Set<SaleOffer>();
    public DbSet<HolidayCalender> HolidayCalenders => Set<HolidayCalender>();
    public DbSet<TargetSetup> TargetSetups => Set<TargetSetup>();
    public DbSet<TargetSetupDetail> TargetSetupDetails => Set<TargetSetupDetail>();
    public DbSet<CommissionSetup> CommissionSetups => Set<CommissionSetup>();
    public DbSet<DesWiseCommission> DesWiseCommissions => Set<DesWiseCommission>();

    public DatabaseFacade DatabaseFacade => base.Database;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global Query Filters for Soft Delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, falseConstant), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        // Decimal Precision defaults
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        modelBuilder.Entity<VehiclePeriodicChecklist>(entity =>
        {
            entity.HasKey(e => e.ChecklistID);
            entity.ToTable("VehiclePeriodicChecklists");
        });

        modelBuilder.Entity<WorkshopBay>(entity =>
        {
            entity.HasKey(e => e.BayId);
            entity.ToTable("WorkshopBays");
        });

        modelBuilder.Entity<WorkshopTimeSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId);
            entity.ToTable("WorkshopTimeSlots");
        });

        modelBuilder.Entity<WorkshopChecklistMaster>(entity =>
        {
            entity.HasKey(e => e.ItemId);
            entity.ToTable("WorkshopChecklistMasters");
        });

        modelBuilder.Entity<CustomerServiceRating>(entity =>
        {
            entity.HasKey(e => e.RatingID);
            entity.ToTable("CustomerServiceRatings");
        });

        modelBuilder.Entity<PostServiceCallFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackID);
            entity.ToTable("PostServiceCallFeedbacks");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUser?.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                entry.Entity.DeletedBy = currentUserId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

