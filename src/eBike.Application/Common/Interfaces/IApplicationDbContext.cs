using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace eBike.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // DB-First Identity & Security
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<ApplicationUserRole> UserRoles { get; }
    DbSet<UserRefreshToken> UserRefreshTokens { get; }
    DbSet<ApplicationMenuItem> MenuItems { get; }
    DbSet<ApplicationRoleMenu> ApplicationRoleMenus { get; }
    DbSet<ApplicationPermission> Permissions { get; }
    DbSet<ApplicationMenuPermission> MenuPermissions { get; }
    DbSet<SisterConcern> SisterConcerns { get; }
    DbSet<Company> Companies { get; }
    DbSet<Category> Categories { get; }
    DbSet<Color> Colors { get; }
    DbSet<Part> Parts { get; }
    DbSet<PLocation> PLocations { get; }
    DbSet<Godown> Godowns { get; }
    DbSet<Product> Products { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<District> Districts { get; }
    DbSet<Thana> Thanas { get; }
    DbSet<Designation> Designations { get; }
    DbSet<Department> Departments { get; }
    DbSet<Religion> Religions { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Bank> Banks { get; }
    DbSet<BankTransaction> BankTransactions { get; }
    DbSet<ExpenseItem> ExpenseItems { get; }
    DbSet<Expenditure> Expenditures { get; }
    DbSet<CashCollection> CashCollections { get; }
    DbSet<POrder> POrders { get; }
    DbSet<POrderDetail> POrderDetails { get; }
    DbSet<POProductDetail> POProductDetails { get; }
    DbSet<Stock> Stocks { get; }
    DbSet<StockDetail> StockDetails { get; }
    DbSet<SOrder> SOrders { get; }
    DbSet<SOrderDetail> SOrderDetails { get; }
    DbSet<Registration> Registrations { get; }
    DbSet<BRTARate> BRTARates { get; }
    DbSet<CreditSale> CreditSales { get; }
    DbSet<CreditSaleDetail> CreditSaleDetails { get; }
    DbSet<CreditSalesSchedule> CreditSalesSchedules { get; }
    DbSet<CreditInterestPercentage> CreditInterestPercentages { get; }
    DbSet<PurchaseDO> DOes { get; }
    DbSet<PurchaseDODetail> DODetails { get; }
    DbSet<Transfer> Transfers { get; }
    DbSet<TransferDetail> TransferDetails { get; }
    DbSet<TransferHistory> TransferHistories { get; }
    DbSet<SystemInformation> SystemInformations { get; }

    // Domain Entities
    DbSet<Warehouse> Warehouses { get; }
    DbSet<EBikeMaster> EBikes { get; }
    DbSet<SparePart> SpareParts { get; }
    DbSet<StockMovement> StockMovements { get; }

    DbSet<Customer> Customers { get; }
    DbSet<CustomerVehicle> CustomerVehicles { get; }
    DbSet<ServiceFollowUpLog> ServiceFollowUpLogs { get; }
    DbSet<JobCard> JobCards { get; }
    DbSet<JobCardLaborDetail> JobCardLaborDetails { get; }
    DbSet<JobCardPartsDetail> JobCardPartsDetails { get; }
    DbSet<VehiclePeriodicChecklist> VehiclePeriodicChecklists { get; }
    DbSet<ServiceTechnician> ServiceTechnicians { get; }
    DbSet<ServiceTicket> ServiceTickets { get; }
    DbSet<ServiceTicketItem> ServiceTicketItems { get; }
    DbSet<WorkshopBay> WorkshopBays { get; }
    DbSet<WorkshopTimeSlot> WorkshopTimeSlots { get; }
    DbSet<WorkshopChecklistMaster> WorkshopChecklistMasters { get; }
    DbSet<CustomerServiceRating> CustomerServiceRatings { get; }
    DbSet<PostServiceCallFeedback> PostServiceCallFeedbacks { get; }
    DbSet<SaleOffer> SaleOffers { get; }
    DbSet<HolidayCalender> HolidayCalenders { get; }
    DbSet<TargetSetup> TargetSetups { get; }
    DbSet<TargetSetupDetail> TargetSetupDetails { get; }
    DbSet<CommissionSetup> CommissionSetups { get; }
    DbSet<DesWiseCommission> DesWiseCommissions { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
