using Microsoft.AspNetCore.Mvc;
using Bridgo.Services.Interfaces;
using Bridgo.Models.Enums;
using Bridgo.Services;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Tip tablolari API - Code-based type'lari frontend'e sunar
/// </summary>
[ApiController]
[Route("api/types")]
public class TypesApiController : ControllerBase
{
    private readonly ILocalizationService _localization;

    public TypesApiController(ILocalizationService localization)
    {
        _localization = localization;
    }

    /// <summary>
    /// Adres tiplerini getir
    /// </summary>
    [HttpGet("address")]
    public IActionResult GetAddressTypes()
    {
        var items = AddressTypes.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            Name = _localization.GetResource(t.NameResourceKey, t.SystemName),
            t.Description,
            t.Icon,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Depo tiplerini getir
    /// </summary>
    [HttpGet("warehouse")]
    public IActionResult GetWarehouseTypes()
    {
        var items = WarehouseTypes.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            Name = _localization.GetResource(t.NameResourceKey, t.SystemName),
            t.Description,
            t.Icon,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Vendor durumlarini getir
    /// </summary>
    [HttpGet("vendor-status")]
    public IActionResult GetVendorStatuses()
    {
        var items = VendorStatuses.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            Name = _localization.GetResource(t.NameResourceKey, t.SystemName),
            t.Description,
            t.Icon,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Urun durumlarini getir
    /// </summary>
    [HttpGet("product-status")]
    public IActionResult GetProductStatuses()
    {
        var items = ProductStatuses.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            Name = _localization.GetResource(t.NameResourceKey, t.SystemName),
            t.Description,
            t.Icon,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Ekip uyesi durumlarini getir
    /// </summary>
    [HttpGet("team-member-status")]
    public IActionResult GetTeamMemberStatuses()
    {
        var items = TeamMemberStatuses.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            Name = _localization.GetResource(t.NameResourceKey, t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Ekip uyesi tiplerini getir (tumu)
    /// </summary>
    [HttpGet("team-member-type")]
    public IActionResult GetTeamMemberTypes()
    {
        var items = TeamMemberTypes.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            Name = _localization.GetResource(t.NameResourceKey, t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Kurumsal ekip uyesi tiplerini getir (Employee haric)
    /// </summary>
    [HttpGet("team-member-type/corporate")]
    public IActionResult GetCorporateTeamMemberTypes()
    {
        var items = TeamMemberTypes.CorporateTypes.Select(t => new
        {
            t.Id,
            t.SystemName,
            Name = _localization.GetResource(t.NameResourceKey, t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Uye dogrulama durumlarini getir
    /// </summary>
    [HttpGet("member-verification-status")]
    public IActionResult GetMemberVerificationStatuses()
    {
        var items = MemberVerificationStatuses.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            Name = _localization.GetResource(t.NameResourceKey, t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    // ============================================================
    // INCOTERMS ENDPOINTS
    // ============================================================

    /// <summary>
    /// Tum Incoterms'leri getir
    /// </summary>
    [HttpGet("incoterms")]
    public IActionResult GetIncoterms()
    {
        var items = Incoterms.All.Select(t => new
        {
            t.Id,
            code = t.SystemName,
            name = _localization.GetResource(t.NameResourceKey, t.Description ?? t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder,
            isSeaOnly = Incoterms.SeaOnly.Contains(t)
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Sadece tum tasima modlari icin gecerli Incoterms'leri getir (EXW, FCA, CPT, CIP, DAP, DPU, DDP)
    /// </summary>
    [HttpGet("incoterms/all-modes")]
    public IActionResult GetAllModesIncoterms()
    {
        var items = Incoterms.AllModes.Select(t => new
        {
            t.Id,
            code = t.SystemName,
            name = _localization.GetResource(t.NameResourceKey, t.Description ?? t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder,
            isSeaOnly = false
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Sadece deniz tasimaciligi icin gecerli Incoterms'leri getir (FAS, FOB, CFR, CIF)
    /// </summary>
    [HttpGet("incoterms/sea-only")]
    public IActionResult GetSeaOnlyIncoterms()
    {
        var items = Incoterms.SeaOnly.Select(t => new
        {
            t.Id,
            code = t.SystemName,
            name = _localization.GetResource(t.NameResourceKey, t.Description ?? t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder,
            isSeaOnly = true
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Belirli bir Incoterm icin sorumluluk matrisini getir
    /// </summary>
    [HttpGet("incoterms/{id:int}/responsibilities")]
    public IActionResult GetIncotermResponsibilities(int id)
    {
        var incoterm = Incoterms.GetById(id);
        if (incoterm == null)
            return NotFound(new { message = "Incoterm bulunamadi" });

        var responsibilities = IncotermResponsibilities.GetAllResponsibilities(id);

        var sellerResponsibilities = new List<object>();
        var buyerResponsibilities = new List<object>();

        foreach (var resp in responsibilities)
        {
            var respType = IncotermResponsibilityTypes.GetById(resp.Key);
            if (respType == null) continue;

            var item = new
            {
                typeId = resp.Key,
                name = _localization.GetResource(respType.NameResourceKey, respType.Description ?? respType.SystemName),
                icon = respType.Icon,
                cssClass = respType.CssClass,
                canRequestFinancing = resp.Key != IncotermResponsibilityTypes.Ids.Loading && resp.Key != IncotermResponsibilityTypes.Ids.Unloading,
                financingSubjectId = FinancingSubjects.FromResponsibilityType(resp.Key)
            };

            if (resp.Value == IncotermResponsibilities.Seller)
                sellerResponsibilities.Add(item);
            else if (resp.Value == IncotermResponsibilities.Buyer)
                buyerResponsibilities.Add(item);
            // Optional olanlar her iki tarafa da eklenmez, UI'da ayri gosterilir
        }

        return Ok(new
        {
            incotermId = id,
            incotermCode = incoterm.SystemName,
            incotermName = _localization.GetResource(incoterm.NameResourceKey, incoterm.Description ?? incoterm.SystemName),
            sellerResponsibilities,
            buyerResponsibilities
        });
    }

    /// <summary>
    /// Finansman konularini getir
    /// </summary>
    [HttpGet("financing-subjects")]
    public IActionResult GetFinancingSubjects()
    {
        var items = FinancingSubjects.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            name = _localization.GetResource(t.NameResourceKey, t.Description ?? t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Sorumluluk tiplerini getir
    /// </summary>
    [HttpGet("responsibility-types")]
    public IActionResult GetResponsibilityTypes()
    {
        var items = IncotermResponsibilityTypes.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            name = _localization.GetResource(t.NameResourceKey, t.Description ?? t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Tum olcu birimlerini getir
    /// </summary>
    [HttpGet("units")]
    public IActionResult GetUnitTypes()
    {
        var items = UnitTypes.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            name = _localization.GetResource(t.NameResourceKey, t.Description ?? t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder,
            isPackagingUnit = UnitTypes.IsPackagingUnit(t.Id)
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Temel birimleri getir (Product.SalesUnitId icin)
    /// Piece, Kilogram, Meter, Liter, vb.
    /// </summary>
    [HttpGet("units/base")]
    public IActionResult GetBaseUnits()
    {
        var items = UnitTypes.BaseUnits.Select(t => new
        {
            t.Id,
            t.SystemName,
            name = _localization.GetResource(t.NameResourceKey, t.Description ?? t.SystemName),
            t.Description,
            t.Icon,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }

    /// <summary>
    /// Paketleme birimlerini getir (ProductPackaging.UnitId icin)
    /// Pack, Box, Carton, Pallet, Container, Roll
    /// </summary>
    [HttpGet("units/packaging")]
    public IActionResult GetPackagingUnits()
    {
        var items = UnitTypes.PackagingUnits.Select(t => new
        {
            t.Id,
            t.SystemName,
            name = _localization.GetResource(t.NameResourceKey, t.Description ?? t.SystemName),
            t.Description,
            t.Icon,
            t.CssClass,
            t.IsDefault,
            t.DisplayOrder
        }).OrderBy(t => t.DisplayOrder);

        return Ok(items);
    }
}
