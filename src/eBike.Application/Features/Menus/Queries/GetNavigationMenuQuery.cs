using System.Text.RegularExpressions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Menus.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Menus.Queries;

public record GetNavigationMenuQuery(int UserId) : IRequest<IReadOnlyList<NavigationGroupDto>>;

public class GetNavigationMenuQueryHandler : IRequestHandler<GetNavigationMenuQuery, IReadOnlyList<NavigationGroupDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNavigationMenuQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NavigationGroupDto>> Handle(GetNavigationMenuQuery request, CancellationToken cancellationToken)
    {
        // 1. Get user roles & admin flag
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == request.UserId)
            .Include(ur => ur.Role)
            .ToListAsync(cancellationToken);

        var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();
        bool isAdmin = userRoles.Any(ur => ur.Role != null && 
            (ur.Role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
             ur.Role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)));

        // 2. Fetch all menu items from database
        var allMenus = await _context.MenuItems
            .AsNoTracking()
            .OrderBy(m => m.Sequence)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);

        // 3. Determine permitted menu IDs
        HashSet<int> permittedMenuIds;
        if (isAdmin)
        {
            permittedMenuIds = new HashSet<int>(allMenus.Select(m => m.Id));
        }
        else
        {
            var allowedFromRoles = await _context.ApplicationRoleMenus
                .Where(arm => roleIds.Contains(arm.RoleId))
                .Select(arm => arm.MenuId)
                .Distinct()
                .ToListAsync(cancellationToken);

            permittedMenuIds = new HashSet<int>(allowedFromRoles);
        }

        // 4. Identify Root Parent menus (ParentId == 0 or null)
        var rootMenus = allMenus
            .Where(m => !m.ParentId.HasValue || m.ParentId.Value == 0)
            .OrderBy(m => m.Sequence)
            .ThenBy(m => m.Id)
            .ToList();

        var navGroups = new List<NavigationGroupDto>();

        foreach (var root in rootMenus)
        {
            var directChildren = allMenus
                .Where(m => m.ParentId == root.Id)
                .OrderBy(m => m.Sequence)
                .ThenBy(m => m.Id)
                .ToList();

            var items = new List<NavigationItemDto>();

            foreach (var child in directChildren)
            {
                if (!isAdmin && !permittedMenuIds.Contains(child.Id))
                    continue;

                var mappedUrl = MapToFrontendUrl(child.Url);

                // 1. If child itself has an actionable page route, add it
                if (!string.IsNullOrWhiteSpace(mappedUrl) && mappedUrl != "#" && mappedUrl != "##" && mappedUrl != "###")
                {
                    items.Add(new NavigationItemDto(
                        Id: child.Id,
                        Title: child.Title,
                        Url: mappedUrl,
                        Icon: ExtractIcon(child.Icon),
                        Sequence: child.Sequence,
                        Category: root.Title,
                        Slug: NormalizeSlug(child.Title),
                        Permissions: new MenuActionPermissionsDto(true, isAdmin, isAdmin, isAdmin)
                    ));
                }

                // 2. Check if child has nested sub-children (e.g. 3-level menu)
                var subChildren = allMenus
                    .Where(m => m.ParentId == child.Id)
                    .OrderBy(m => m.Sequence)
                    .ThenBy(m => m.Id)
                    .ToList();

                if (subChildren.Count > 0)
                {
                    foreach (var sub in subChildren)
                    {
                        if (!isAdmin && !permittedMenuIds.Contains(sub.Id))
                            continue;

                        var subUrl = MapToFrontendUrl(sub.Url);
                        if (string.IsNullOrWhiteSpace(subUrl) || subUrl == "#" || subUrl == "##" || subUrl == "###")
                            continue;

                        items.Add(new NavigationItemDto(
                            Id: sub.Id,
                            Title: sub.Title,
                            Url: subUrl,
                            Icon: ExtractIcon(!string.IsNullOrWhiteSpace(sub.Icon) ? sub.Icon : child.Icon),
                            Sequence: sub.Sequence,
                            Category: root.Title,
                            Slug: NormalizeSlug(sub.Title),
                            Permissions: new MenuActionPermissionsDto(true, isAdmin, isAdmin, isAdmin)
                        ));
                    }
                }
            }

            if (items.Count > 0)
            {
                navGroups.Add(new NavigationGroupDto(root.Title, items));
            }
        }

        return navGroups;
    }

    private static string NormalizeSlug(string title)
    {
        return title.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_").Replace("&", "and");
    }

    private static string ExtractIcon(string? rawIcon)
    {
        if (string.IsNullOrWhiteSpace(rawIcon))
            return "pi pi-circle";

        var str = rawIcon.ToLowerInvariant();

        if (str.Contains("fa-cogs") || str.Contains("fa-cog")) return "pi pi-cog";
        if (str.Contains("fa-users") || str.Contains("fa-user")) return "pi pi-users";
        if (str.Contains("fa-address-book")) return "pi pi-id-card";
        if (str.Contains("fa-calculator") || str.Contains("fa-briefcase")) return "pi pi-briefcase";
        if (str.Contains("fa-flag-checkered") || str.Contains("fa-line-chart")) return "pi pi-chart-line";
        if (str.Contains("fa-money")) return "pi pi-dollar";
        if (str.Contains("fa-product-hunt")) return "pi pi-box";
        if (str.Contains("fa-commenting")) return "pi pi-comments";
        if (str.Contains("fa-shield")) return "pi pi-shield";
        if (str.Contains("fa-shopping-bag")) return "pi pi-shopping-bag";
        if (str.Contains("fa-truck")) return "pi pi-truck";
        if (str.Contains("fa-wrench")) return "pi pi-wrench";
        if (str.Contains("fa-calendar")) return "pi pi-calendar";

        if (rawIcon.StartsWith("pi "))
            return rawIcon;

        return "pi pi-circle";
    }

    public static string MapToFrontendUrl(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl) || rawUrl == "##" || rawUrl == "###" || rawUrl.StartsWith("#"))
            return string.Empty;

        var url = rawUrl.Trim();
        var lower = url.ToLowerInvariant().TrimEnd('/');

        return lower switch
        {
            // System, Security & Core
            "/user" or "user" => "/system/users",
            "/role" or "role" => "/system/roles",
            "/menu" or "menu" => "/system/menus",
            "/systeminformation" or "systeminformation" or "/systeminfo" or "systeminfo" => "/system/system-info",
            "/sisterconcern" or "sisterconcern" or "/sisterconcerns" or "sisterconcerns" => "/system/sister-concerns",

            // Catalog & Master Data
            "/company" or "company" => "/catalog/companies",
            "/category" or "category" => "/catalog/categories",
            "/color" or "color" => "/catalog/colors",
            "/product" or "product" => "/catalog/products",
            "/partsno" or "partsno" or "/part" or "part" or "/parts" or "parts" => "/catalog/parts",
            "/godown" or "godown" => "/catalog/godowns",
            "/plocation" or "plocation" => "/catalog/rack-locations",
            "/district" or "district" => "/catalog/districts-thanas",
            "/thana" or "thana" => "/catalog/districts-thanas",
            "/saleoffer" or "saleoffer" or "/offer" or "offer" => "/sales/offers",
            "/targetsetup" or "targetsetup" => "/sales/target-setup",
            "/commissionsetup" or "commissionsetup" => "/sales/commission-setup",

            // HR & Staff
            "/designation" or "designation" => "/hr/departments-designations",
            "/department" or "department" => "/hr/departments-designations",
            "/employee" or "employee" => "/hr/employees",
            "/employeeleave" or "employeeleave" => "/hr/leaves-holidays",
            "/holidaycalender" or "holidaycalender" => "/hr/leaves-holidays",
            "/uploadattendence" or "uploadattendence" => "/hr/attendance",
            "/uploadattendence/getmachinedata" => "/hr/attendance",
            "/salaryprocess" or "salaryprocess" or "/salaryprocess/getsalarysheet" or "/salaryprocess/getpayslip" => "/payroll/salary-process",
            "/advancesalary" or "advancesalary" => "/payroll/salary-process",
            "/grade" or "grade" or "/gradesalaryassignment" => "/hr/departments-designations",
            "/allowancededuction" or "allowancededuction" => "/payroll/salary-process",

            // Customers & Suppliers & Procurement
            "/customer" or "customer" => "/sales/customers",
            "/supplier" or "supplier" => "/procurement/suppliers",
            "/purchasedo" or "purchasedo" => "/procurement/purchase-do",
            "/purchasedo/doreport" => "/procurement/purchase-do",
            "/purchaseorder" or "purchaseorder" => "/procurement/purchase-orders",
            "/purchaseorder/supplierledger" => "/procurement/purchase-orders",
            "/poreturn" or "poreturn" => "/procurement/purchase-returns",
            "/roorder" or "roorder" => "/procurement/requisitions",

            // Sales & EMI & Registrations
            "/salesorder" or "salesorder" => "/sales/orders",
            "/creditsalesorder" or "creditsalesorder" or "/creditsales" or "creditsales" => "/credit/agreements",
            "/creditinterestpercentage" or "creditinterestpercentage" => "/credit/interest-setup",
            "/return/index" or "/return" or "return" => "/sales/sales-returns",
            "/replacementorder/index" or "/replacementorder" or "replacementorder" => "/sales/replacement-orders",
            "/registration" or "registration" => "/sales/registrations",
            "/vatsetup" or "vatsetup" => "/sales/vat-setup",
            "/customer/customerledger" => "/sales/customers",
            "/customer/customerduereport" => "/sales/customers",

            // Inventory & Stock
            "/stock/index" or "/stock" or "stock" or "/stock/stockdetails" or "/stock/stockreport" => "/inventory/stock-overview",
            "/transfer" or "transfer" or "/transfer/transferreport" => "/inventory/branch-transfers",

            // Banking & Finance
            "/bank/index" or "/bank" or "bank" => "/finance/banks",
            "/banktransaction" or "banktransaction" => "/finance/bank-transactions",
            "/expenseitem" or "expenseitem" => "/finance/expense-items",
            "/expenditure" or "expenditure" => "/finance/expenditures",
            "/cashcollection" or "cashcollection" => "/credit/collections",
            "/cashdelivery" or "cashdelivery" => "/finance/cash-deliveries",
            "/cashcollection/cashinhandreport" or "/cashcollection/dailycash" => "/accounts/cash-book",
            "/expenditure/trialbalance" or "/expenditure/profitlossaccount" or "/expenditure/balancesheetnew" => "/accounts/cash-book",
            "/investmenthead" or "investmenthead" or "/investment" or "investment" or "/investment/currentasset" => "/finance/capital-investments",

            // Auto Parts
            "/approduct" or "approduct" => "/catalog/parts",
            "/appurchaseorder" or "appurchaseorder" => "/procurement/purchase-orders",
            "/apsalesorder" or "apsalesorder" => "/sales/orders",
            "/apcashcollection" or "apcashcollection" => "/credit/collections",

            _ => url.StartsWith("/") ? url : "/" + url
        };
    }
}
