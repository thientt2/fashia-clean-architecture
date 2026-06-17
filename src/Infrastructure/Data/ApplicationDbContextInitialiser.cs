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
    private readonly IInventoryMatrixInitializer _inventoryInitializer;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IInventoryMatrixInitializer inventoryInitializer
    )
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _inventoryInitializer = inventoryInitializer;
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
            await SeedProductsAsync();
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
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1780836893/temporary/products/4424ae5b88f442b5b16251764116f493_8a74966c70a19c149e693be1d8fea0fb.jpg",
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

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "0989fb63e2cff36d8ccf27201c5e853b_fufsq8",
                    originalFileName: "0989fb63e2cff36d8ccf27201c5e853b.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503387/0989fb63e2cff36d8ccf27201c5e853b_fufsq8.jpg",
                    publicId: "0989fb63e2cff36d8ccf27201c5e853b_fufsq8",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "7a692907af840024c66374ca9265a8c7_pwtiam",
                    originalFileName: "7a692907af840024c66374ca9265a8c7.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503374/7a692907af840024c66374ca9265a8c7_pwtiam.jpg",
                    publicId: "7a692907af840024c66374ca9265a8c7_pwtiam",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "caa2b655edf335edd77f1d1621c93618_hjz8ka",
                    originalFileName: "caa2b655edf335edd77f1d1621c93618.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503296/caa2b655edf335edd77f1d1621c93618_hjz8ka.jpg",
                    publicId: "caa2b655edf335edd77f1d1621c93618_hjz8ka",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "e3b2220f03c4d6cc1e7d7c44fc7cd21f_l0xkek",
                    originalFileName: "e3b2220f03c4d6cc1e7d7c44fc7cd21f.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503292/e3b2220f03c4d6cc1e7d7c44fc7cd21f_l0xkek.jpg",
                    publicId: "e3b2220f03c4d6cc1e7d7c44fc7cd21f_l0xkek",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "4c74153eef36bd6bb01728a680e765d9_fwbwim",
                    originalFileName: "4c74153eef36bd6bb01728a680e765d9.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503230/4c74153eef36bd6bb01728a680e765d9_fwbwim.jpg",
                    publicId: "4c74153eef36bd6bb01728a680e765d9_fwbwim",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "78cf7400573e668e882098d3bc7b1f66_qlombv",
                    originalFileName: "78cf7400573e668e882098d3bc7b1f66.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503225/78cf7400573e668e882098d3bc7b1f66_qlombv.jpg",
                    publicId: "78cf7400573e668e882098d3bc7b1f66_qlombv",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "ba37e7a4c76ff2d35482e7aab79e0db9_efrvy7",
                    originalFileName: "ba37e7a4c76ff2d35482e7aab79e0db9.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503150/ba37e7a4c76ff2d35482e7aab79e0db9_efrvy7.jpg",
                    publicId: "ba37e7a4c76ff2d35482e7aab79e0db9_efrvy7",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "8f00d4a1f6c2189d180f613faaba6a8f_nffblb",
                    originalFileName: "8f00d4a1f6c2189d180f613faaba6a8f.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503201/8f00d4a1f6c2189d180f613faaba6a8f_nffblb.jpg",
                    publicId: "8f00d4a1f6c2189d180f613faaba6a8f_nffblb",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "9f87fab2a45b5b91183a9b4411a655cf_mizwqk",
                    originalFileName: "9f87fab2a45b5b91183a9b4411a655cf.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503177/9f87fab2a45b5b91183a9b4411a655cf_mizwqk.jpg",
                    publicId: "9f87fab2a45b5b91183a9b4411a655cf_mizwqk",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "b3f13a3c394632ecfaa48e0cb2b9e944_ytht2p",
                    originalFileName: "b3f13a3c394632ecfaa48e0cb2b9e944.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781502915/b3f13a3c394632ecfaa48e0cb2b9e944_ytht2p.jpg",
                    publicId: "b3f13a3c394632ecfaa48e0cb2b9e944_ytht2p",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "5580b224d0a95daa4857f083cfd688de_g0t2co",
                    originalFileName: "5580b224d0a95daa4857f083cfd688de.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503089/5580b224d0a95daa4857f083cfd688de_g0t2co.jpg",
                    publicId: "5580b224d0a95daa4857f083cfd688de_g0t2co",
                    folder: "temporary/products"
                )
            );

            _context.UploadedFiles.Add(
                new UploadedFile(
                    fileName: "df1629314124e6c3cdbbcf5590396b9b_blukqa",
                    originalFileName: "df1629314124e6c3cdbbcf5590396b9b.jpg",
                    contentType: "image/jpg",
                    sizeInBytes: 204715,
                    url: "https://res.cloudinary.com/dln6n09l2/image/upload/v1781503123/df1629314124e6c3cdbbcf5590396b9b_blukqa.jpg",
                    publicId: "df1629314124e6c3cdbbcf5590396b9b_blukqa",
                    folder: "temporary/products"
                )
            );

            await _context.SaveChangesAsync();
        }

        var admin = await _userManager.Users.FirstOrDefaultAsync(u =>
            u.UserName == "administrator@localhost"
        );

        if (admin == null)
        {
            throw new Exception("Administrator user not found.");
        }

        await _context
            .UploadedFiles.Where(x => x.CreatedBy == null)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(x => x.CreatedBy, admin.Id)
                    .SetProperty(x => x.LastModifiedBy, admin.Id)
            );
    }

    private async Task SeedProductsAsync()
    {
        if (!_context.Products.Any())
        {
            // admin user is the creator of all seeded products and related data
            var admin = await _userManager.Users.FirstOrDefaultAsync(u =>
                u.UserName == "administrator@localhost"
            );

            if (admin == null)
            {
                throw new Exception("Administrator user not found.");
            }

            // Lookup brands by name
            var nike = await _context.Brands.FirstAsync(b => b.Name == "Nike");
            var zara = await _context.Brands.FirstAsync(b => b.Name == "Zara");

            // Lookup categories by name (leaf categories)
            var T_Shirts_Men = await _context.Categories.FirstAsync(c =>
                c.Name == "Áo thun nam ngắn tay không cổ"
            );

            var T_Shirts_Women = await _context.Categories.FirstAsync(c =>
                c.Name == "Áo thun nữ ngắn tay không cổ"
            );

            // Lookup attribute values by name
            var colorBlack = await _context.AttributeValues.FirstAsync(v => v.Value == "Đen");
            var colorWhite = await _context.AttributeValues.FirstAsync(v => v.Value == "Trắng");
            var colorRed = await _context.AttributeValues.FirstAsync(v => v.Value == "Đỏ");

            var sizeS = await _context.AttributeValues.FirstAsync(v => v.Value == "S");
            var sizeM = await _context.AttributeValues.FirstAsync(v => v.Value == "M");
            var sizeL = await _context.AttributeValues.FirstAsync(v => v.Value == "L");

            var materialCotton = await _context.AttributeValues.FirstAsync(v =>
                v.Value == "Cotton"
            );

            // ── Product 1: Áo thun nam Nike Sportswear ──
            var product1 = new Product(
                name: "Áo thun nam Nike Sportswear",
                categoryId: T_Shirts_Men.Id,
                brandId: nike.Id,
                description: "Áo thun nam Nike Sportswear với chất liệu cotton cao cấp, thoáng mát và thoải mái. Thiết kế basic dễ phối đồ, phù hợp cho mọi hoạt động hàng ngày.",
                uploadedImageIds: new List<int> { 1, 2, 3 }
            );

            var v1_1 = product1.AddVariant(
                Money.Create(650_000),
                new[] { colorBlack.Id, sizeM.Id, materialCotton.Id }
            );
            v1_1.AddImage(uploadedFileId: 4, displayOrder: 0);

            var v1_2 = product1.AddVariant(
                Money.Create(650_000),
                new[] { colorBlack.Id, sizeL.Id, materialCotton.Id }
            );
            v1_2.AddImage(uploadedFileId: 5, displayOrder: 0);

            var v1_3 = product1.AddVariant(
                Money.Create(650_000),
                new[] { colorWhite.Id, sizeM.Id, materialCotton.Id }
            );
            v1_3.AddImage(uploadedFileId: 6, displayOrder: 0);

            var v1_4 = product1.AddVariant(
                Money.Create(650_000),
                new[] { colorWhite.Id, sizeL.Id, materialCotton.Id }
            );
            v1_4.AddImage(uploadedFileId: 7, displayOrder: 0);

            _context.Products.Add(product1);

            // ── Product 2: Áo sơ mi nữ Zara Basic ──
            var product2 = new Product(
                name: "Áo sơ mi nữ Zara Basic",
                categoryId: T_Shirts_Women.Id,
                brandId: zara.Id,
                description: "Áo sơ mi nữ Zara Basic tay dài, phong cách thanh lịch phù hợp đi làm và dạo phố. Chất vải mềm mại, form dáng thoải mái.",
                uploadedImageIds: new List<int> { 8, 9, 10 }
            );

            var v2_1 = product2.AddVariant(
                Money.Create(750_000),
                new[] { colorWhite.Id, sizeS.Id }
            );
            v2_1.AddImage(uploadedFileId: 11, displayOrder: 0);

            var v2_2 = product2.AddVariant(
                Money.Create(750_000),
                new[] { colorWhite.Id, sizeM.Id }
            );
            v2_2.AddImage(uploadedFileId: 12, displayOrder: 0);

            var v2_3 = product2.AddVariant(Money.Create(750_000), new[] { colorRed.Id, sizeS.Id });
            v2_3.AddImage(uploadedFileId: 13, displayOrder: 0);

            _context.Products.Add(product2);

            await _context.SaveChangesAsync(CancellationToken.None);

            await _context
                .Products.Where(x => x.CreatedBy == null)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(x => x.CreatedBy, admin.Id)
                        .SetProperty(x => x.LastModifiedBy, admin.Id)
                );

            await _context
                .ProductVariants.Where(x => x.CreatedBy == null)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(x => x.CreatedBy, admin.Id)
                        .SetProperty(x => x.LastModifiedBy, admin.Id)
                );

            _logger.LogInformation("Seeded {Count} products with variants.", 2);
        }

        await EnsureBranchVariantInventoriesAsync();
    }

    private async Task EnsureBranchVariantInventoriesAsync()
    {
        var variantIds = await _context
            .ProductVariants.Select(x => x.Id)
            .ToListAsync(CancellationToken.None);

        if (variantIds.Count == 0)
            return;

        await _inventoryInitializer.EnsureForProductVariantsAsync(
            variantIds,
            CancellationToken.None
        );

        await _context.SaveChangesAsync(CancellationToken.None);
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
