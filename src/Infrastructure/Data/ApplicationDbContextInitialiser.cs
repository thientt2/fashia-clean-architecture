using System.Text.Json;
using Fashia.Domain.Constants;
using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using Fashia.Infrastructure.Data.SeedData;
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

        var initialiser =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

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

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
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
            await SeedCategoriesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Seed branches
        if (!_context.Branches.Any())
        {
            _context.Branches.Add(
                new Branch(
                    name: "Chi nhánh 1",
                    phone: PhoneNumber.Create("0369405891"),
                    email: EmailVO.Create("branch1@localhost"),
                    address: Address.Create(
                        line1: "123 Main St",
                        ward: "District 1",
                        district: "HCM City",
                        province: "Vietnam"
                    ),
                    location: GeoLocation.Create(latitude: 10.7094913M, longitude: 106.7056713M)
                )
            );
            _context.Branches.Add(
                new Branch(
                    name: "Chi nhánh 2",
                    phone: PhoneNumber.Create("0369405892"),
                    email: EmailVO.Create("branch2@localhost"),
                    address: Address.Create(
                        line1: "456 Elm St",
                        ward: "District 2",
                        district: "HCM City",
                        province: "Vietnam"
                    ),
                    location: GeoLocation.Create(latitude: 10.7094913M, longitude: 106.7056713M)
                )
            );
            _context.Branches.Add(
                new Branch(
                    name: "Chi nhánh 3",
                    phone: PhoneNumber.Create("0369405893"),
                    email: EmailVO.Create("branch3@localhost"),
                    address: Address.Create(
                        line1: "789 Pine Rd",
                        ward: "District 3",
                        district: "HCM City",
                        province: "Vietnam"
                    ),
                    location: GeoLocation.Create(latitude: 10.7094913M, longitude: 106.7056713M)
                )
            );

            await _context.SaveChangesAsync();
        }
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);
        var branchManagerRole = new IdentityRole(Roles.BranchManager);
        var customerRole = new IdentityRole(Roles.Customer);
        var employeeRole = new IdentityRole(Roles.Employee);

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

        if (_roleManager.Roles.All(r => r.Name != employeeRole.Name))
        {
            await _roleManager.CreateAsync(employeeRole);
        }

        // Default users
        var administrator = new ApplicationUser
        {
            UserName = "administrator@localhost",
            Email = EmailVO.Create("administrator@localhost"),
        };
        var branchManager1 = new ApplicationUser
        {
            UserName = "branchmanager1@localhost",
            Email = EmailVO.Create("branchmanager1@localhost"),
            BranchId = 1,
        };
        var branchManager2 = new ApplicationUser
        {
            UserName = "branchmanager2@localhost",
            Email = EmailVO.Create("branchmanager2@localhost"),
            BranchId = 2,
        };
        var branchManager3 = new ApplicationUser
        {
            UserName = "branchmanager3@localhost",
            Email = EmailVO.Create("branchmanager3@localhost"),
            BranchId = 3,
        };
        var customer = new ApplicationUser
        {
            UserName = "customer@localhost",
            Email = "customer@localhost",
        };
        var employee = new ApplicationUser
        {
            UserName = "employee@localhost",
            Email = EmailVO.Create("employee@localhost"),
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        if (_userManager.Users.All(u => u.UserName != branchManager1.UserName))
        {
            await _userManager.CreateAsync(branchManager1, "BranchManager1!");
            if (!string.IsNullOrWhiteSpace(branchManagerRole.Name))
            {
                await _userManager.AddToRolesAsync(
                    branchManager1,
                    new[] { branchManagerRole.Name }
                );
            }
        }

        if (_userManager.Users.All(u => u.UserName != branchManager2.UserName))
        {
            await _userManager.CreateAsync(branchManager2, "BranchManager2!");
            if (!string.IsNullOrWhiteSpace(branchManagerRole.Name))
            {
                await _userManager.AddToRolesAsync(
                    branchManager2,
                    new[] { branchManagerRole.Name }
                );
            }
        }

        if (_userManager.Users.All(u => u.UserName != branchManager3.UserName))
        {
            await _userManager.CreateAsync(branchManager3, "BranchManager3!");
            if (!string.IsNullOrWhiteSpace(branchManagerRole.Name))
            {
                await _userManager.AddToRolesAsync(
                    branchManager3,
                    new[] { branchManagerRole.Name }
                );
            }
        }

        if (_userManager.Users.All(u => u.UserName != customer.UserName))
        {
            await _userManager.CreateAsync(customer, "Customer1!");
            if (!string.IsNullOrWhiteSpace(customerRole.Name))
            {
                await _userManager.AddToRolesAsync(customer, new[] { customerRole.Name });
            }
        }

        if (_userManager.Users.All(u => u.UserName != employee.UserName))
        {
            await _userManager.CreateAsync(employee, "Employee1!");
            if (!string.IsNullOrWhiteSpace(employeeRole.Name))
            {
                await _userManager.AddToRolesAsync(employee, new[] { employeeRole.Name });
            }
        }

        // Default data
        // Seed, if necessary
        // Seed brands
        if (!_context.Brands.Any())
        {
            _context.Brands.Add(new Brand("Nike", "Thương hiệu thể thao nổi tiếng"));
            _context.Brands.Add(new Brand("Adidas", "Thương hiệu thể thao nổi tiếng"));
            _context.Brands.Add(new Brand("Zara", "Thương hiệu thời trang nổi tiếng"));
            _context.Brands.Add(new Brand("H&M", "Thương hiệu thời trang nổi tiếng"));
            _context.Brands.Add(
                new Brand("Phúc An Fashion", "Một thương hiệu thời trang Việt Nam")
            );

            await _context.SaveChangesAsync();
        }

        // Seed attributes
        if (!_context.Attributes.Any())
        {
            _context.Attributes.Add(new ProductAttribute("Màu sắc"));
            _context.Attributes.Add(new ProductAttribute("Kích thước"));
            _context.Attributes.Add(new ProductAttribute("Chất liệu"));

            await _context.SaveChangesAsync();
        }

        // Seed attribute values
        if (!_context.AttributeValues.Any())
        {
            var colorAttribute = await _context.Attributes.FirstOrDefaultAsync(a =>
                a.Name == "Màu sắc"
            );
            var sizeAttribute = await _context.Attributes.FirstOrDefaultAsync(a =>
                a.Name == "Kích thước"
            );
            var materialAttribute = await _context.Attributes.FirstOrDefaultAsync(a =>
                a.Name == "Chất liệu"
            );

            if (colorAttribute != null)
            {
                _context.AttributeValues.Add(
                    new ProductAttributeValue(colorAttribute.Id, "Đỏ", "#FF0000")
                );
                _context.AttributeValues.Add(
                    new ProductAttributeValue(colorAttribute.Id, "Xanh", "#00FF00")
                );
                _context.AttributeValues.Add(
                    new ProductAttributeValue(colorAttribute.Id, "Đen", "#000000")
                );
                _context.AttributeValues.Add(
                    new ProductAttributeValue(colorAttribute.Id, "Trắng", "#FFFFFF")
                );
            }

            if (sizeAttribute != null)
            {
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "S"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "M"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "L"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "XL"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "XXL"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "2XL"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "3XL"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "4XL"));
                _context.AttributeValues.Add(
                    new ProductAttributeValue(sizeAttribute.Id, "Free Size")
                );
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "26"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "27"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "28"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "29"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "30"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "31"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "32"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "33"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "34"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "35"));
                _context.AttributeValues.Add(new ProductAttributeValue(sizeAttribute.Id, "36"));
            }

            if (materialAttribute != null)
            {
                _context.AttributeValues.Add(
                    new ProductAttributeValue(materialAttribute.Id, "Cotton")
                );
                _context.AttributeValues.Add(
                    new ProductAttributeValue(materialAttribute.Id, "Polyester")
                );
                _context.AttributeValues.Add(
                    new ProductAttributeValue(materialAttribute.Id, "Leather")
                );
                _context.AttributeValues.Add(
                    new ProductAttributeValue(materialAttribute.Id, "Denim")
                );
            }
            await _context.SaveChangesAsync();
        }

        // Seed Uploaded files
        if (!_context.UploadedFiles.Any())
        {
            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "4424ae5b88f442b5b16251764116f493_8a74966c70a19c149e693be1d8fea0fb",
                    originalFileName: "8a74966c70a19c149e693be1d8fea0fb.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "	https://res.cloudinary.com/dln6n09l2/image/upload/v1780836893/temporary/products/4424ae5b88f442b5b16251764116f493_8a74966c70a19c149e693be1d8fea0fb.jpg",
                    publicId: "4424ae5b88f442b5b16251764116f493_8a74966c70a19c149e693be1d8fea0fb",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "5b2c5078f5d743f3a7052e889da0b7fd_43738243b8b7d9ced86cdbd41305f14e",
                    originalFileName: "43738243b8b7d9ced86cdbd41305f14e.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1780817727/temporary/products/5b2c5078f5d743f3a7052e889da0b7fd_43738243b8b7d9ced86cdbd41305f14e.jpg",
                    publicId: "5b2c5078f5d743f3a7052e889da0b7fd_43738243b8b7d9ced86cdbd41305f14e",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "044a9ac3b241429c8e461ce3f6f65b1d_78fbc6c88d59fecb699bced3118fe56a",
                    originalFileName: "78fbc6c88d59fecb699bced3118fe56a.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1780817711/temporary/products/044a9ac3b241429c8e461ce3f6f65b1d_78fbc6c88d59fecb699bced3118fe56a.jpg",
                    publicId: "044a9ac3b241429c8e461ce3f6f65b1d_78fbc6c88d59fecb699bced3118fe56a",
                    folder: "temporary/products"
                )
            );

            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (_context.Categories.Any())
            return;

        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "SeedData",
            "categories.json"
        );

        var json = await File.ReadAllTextAsync(filePath);

        var items =
            JsonSerializer.Deserialize<List<CategorySeedModel>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? [];

        foreach (var item in items)
        {
            await CreateCategoryTreeAsync(item, null);
        }

        await _context.SaveChangesAsync();
    }

    private async Task CreateCategoryTreeAsync(CategorySeedModel model, Category? parent)
    {
        var category = new Category(model.Name, parentId: parent?.Id);
        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        foreach (var child in model.Children)
        {
            await CreateCategoryTreeAsync(child, category);
        }
    }
}
