using System.Text.Json;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bridgo.Services;

/// <summary>
/// Kategori seed servisi - Global kategoriler (Admin yonetimli)
/// JSON dosyasindan kategori verilerini yukler
/// </summary>
public interface ICategorySeedService
{
    Task<ServiceResult> SeedCategoriesAsync();
    Task<ServiceResult> SeedCategoriesFromJsonAsync(string jsonContent);
}

public class CategorySeedService : ICategorySeedService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public CategorySeedService(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<ServiceResult> SeedCategoriesAsync()
    {
        var jsonPath = Path.Combine(_env.ContentRootPath, "App_Data", "Category", "categories-seed.json");

        if (!File.Exists(jsonPath))
            return ServiceResult.Fail("Seed dosyasi bulunamadi: categories-seed.json");

        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        return await SeedCategoriesFromJsonAsync(jsonContent);
    }

    public async Task<ServiceResult> SeedCategoriesFromJsonAsync(string jsonContent)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var seedData = JsonSerializer.Deserialize<CategorySeedRoot>(jsonContent, options);
            if (seedData?.Categories == null || !seedData.Categories.Any())
                return ServiceResult.Fail("JSON dosyasi bos veya hatali");

            // Mevcut kategorileri kontrol et
            var existingCount = await _context.ProductCategories
                .Where(c => !c.IsDeleted)
                .CountAsync();

            if (existingCount > 0)
                return ServiceResult.Fail($"Zaten {existingCount} kategori mevcut. Once mevcut kategorileri silin.");

            var createdCount = 0;
            var categoryMap = new Dictionary<string, int>(); // name -> id mapping

            foreach (var rootCategory in seedData.Categories)
            {
                // Root kategoriyi olustur
                var rootEntity = new ProductCategory
                {
                    Name = rootCategory.Name,
                    Icon = rootCategory.Icon,
                    DisplayOrder = rootCategory.DisplayOrder,
                    IsActive = true,
                    Slug = GenerateSlug(rootCategory.Name)
                };

                _context.ProductCategories.Add(rootEntity);
                await _context.SaveChangesAsync();
                categoryMap[rootCategory.Name] = rootEntity.Id;
                createdCount++;

                // Child kategorileri isle
                if (rootCategory.Children != null && rootCategory.Children.Any())
                {
                    // Once parent'i olmayan child'lari isle
                    var directChildren = rootCategory.Children.Where(c => string.IsNullOrEmpty(c.Parent)).ToList();
                    foreach (var child in directChildren)
                    {
                        var childEntity = new ProductCategory
                        {
                            Name = child.Name,
                            Icon = child.Icon,
                            DisplayOrder = child.DisplayOrder,
                            ParentId = rootEntity.Id,
                            Level = 1,
                            IsActive = true,
                            Slug = GenerateSlug(child.Name)
                        };

                        _context.ProductCategories.Add(childEntity);
                        await _context.SaveChangesAsync();
                        categoryMap[child.Name] = childEntity.Id;
                        createdCount++;
                    }

                    // Sonra parent'i olan child'lari isle (nested children)
                    var nestedChildren = rootCategory.Children.Where(c => !string.IsNullOrEmpty(c.Parent)).ToList();
                    foreach (var child in nestedChildren)
                    {
                        if (categoryMap.TryGetValue(child.Parent!, out var parentId))
                        {
                            var childEntity = new ProductCategory
                            {
                                Name = child.Name,
                                Icon = child.Icon,
                                DisplayOrder = child.DisplayOrder,
                                ParentId = parentId,
                                Level = 2,
                                IsActive = true,
                                Slug = GenerateSlug(child.Name)
                            };

                            _context.ProductCategories.Add(childEntity);
                            await _context.SaveChangesAsync();
                            categoryMap[child.Name] = childEntity.Id;
                            createdCount++;
                        }
                    }
                }
            }

            return ServiceResult.Ok($"{createdCount} kategori basariyla yuklendi");
        }
        catch (JsonException ex)
        {
            return ServiceResult.Fail($"JSON parse hatasi: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Beklenmeyen hata: {ex.Message}");
        }
    }

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "");

        // Turkce karakterleri cevir
        slug = slug
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");

        // Sadece alfanumerik ve tire bırak
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Ardisik tireleri tek tireye indir
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        return slug.Trim('-');
    }
}

// JSON deserialization siniflari
public class CategorySeedRoot
{
    public List<CategorySeedItem> Categories { get; set; } = new();
}

public class CategorySeedItem
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public string? Parent { get; set; }
    public List<CategorySeedItem>? Children { get; set; }
}
