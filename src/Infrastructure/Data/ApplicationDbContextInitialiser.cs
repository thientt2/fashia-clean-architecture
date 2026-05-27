using Fashia.Domain.Constants;
using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using Fashia.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fashia.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);
        var branchManagerRole = new IdentityRole(Roles.BranchManager);
        var customerRole = new IdentityRole(Roles.Customer);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        if (_roleManager.Roles.All(r => r.Name != branchManagerRole.Name))
        {
            await _roleManager.CreateAsync(branchManagerRole);
        }

        if (_roleManager.Roles.All(r => r.Name != customerRole.Name))
        {
            await _roleManager.CreateAsync(customerRole);
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };
        var branchManager = new ApplicationUser { UserName = "branchmanager@localhost", Email = "branchmanager@localhost" };
        var customer = new ApplicationUser { UserName = "customer@localhost", Email = "customer@localhost" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }

        if (_userManager.Users.All(u => u.UserName != branchManager.UserName))
        {
            await _userManager.CreateAsync(branchManager, "BranchManager1!");
            if (!string.IsNullOrWhiteSpace(branchManagerRole.Name))
            {
                await _userManager.AddToRolesAsync(branchManager, new [] { branchManagerRole.Name });
            }
        }

        if (_userManager.Users.All(u => u.UserName != customer.UserName))
        {
            await _userManager.CreateAsync(customer, "Customer1!");
            if (!string.IsNullOrWhiteSpace(customerRole.Name))
            {
                await _userManager.AddToRolesAsync(customer, new [] { customerRole.Name });
            }
        }

        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Tasks",
                Colour = Colour.Green,
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });

            await _context.SaveChangesAsync();
        }

        if (!_context.Categories.Any())
        {
            _context.Categories.Add(new Category("Thời trang Nam", "Thời trang dành cho nam giới", null, null));
            _context.Categories.Add(new Category("Thời trang Nữ", "Thời trang dành cho nữ giới", null, null));
                      

            await _context.SaveChangesAsync();
        }

        if (!_context.Brands.Any())
        {
            _context.Brands.Add(new Brand("Nike", "Thương hiệu thể thao nổi tiếng"));
            _context.Brands.Add(new Brand("Adidas", "Thương hiệu thể thao nổi tiếng"));
            _context.Brands.Add(new Brand("Zara", "Thương hiệu thời trang nổi tiếng"));
            _context.Brands.Add(new Brand("H&M", "Thương hiệu thời trang nổi tiếng"));

            await _context.SaveChangesAsync();
        }

        if (!_context.Attributes.Any())
        {
            _context.Attributes.Add(new ProductAttribute("Màu sắc"));
            _context.Attributes.Add(new ProductAttribute("Kích thước"));
            _context.Attributes.Add(new ProductAttribute("Chất liệu"));

            await _context.SaveChangesAsync();
        }

        if (!_context.AttributeValues.Any())
        {
            var colorAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "Màu sắc");
            var sizeAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "Kích thước");
            var materialAttribute = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == "Chất liệu");

            if (colorAttribute != null)
            {
                _context.AttributeValues.Add(new ProductAttributeValue(colorAttribute.Id, "Đỏ", "#FF0000"));
                _context.AttributeValues.Add(new ProductAttributeValue(colorAttribute.Id, "Xanh", "#00FF00"));
                _context.AttributeValues.Add(new ProductAttributeValue(colorAttribute.Id, "Đen", "#000000"));
                _context.AttributeValues.Add(new ProductAttributeValue(colorAttribute.Id, "Trắng", "#FFFFFF"));
            }

            if (sizeAttribute != null)
            {
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "S"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "M"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "L"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "XL"));
            }

            if (materialAttribute != null)
            {
                _context.AttributeValues.Add(new ProductAttributeValue(materialAttribute.Id, "Cotton"));
                _context.AttributeValues.Add(new ProductAttributeValue(materialAttribute.Id, "Polyester"));
                _context.AttributeValues.Add(new ProductAttributeValue(materialAttribute.Id, "Leather"));
                _context.AttributeValues.Add(new ProductAttributeValue(materialAttribute.Id, "Denim"));
            }

            await _context.SaveChangesAsync();
        }
    }
}
