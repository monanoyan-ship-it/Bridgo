using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Authorization;
using Bridgo.DTOs.Branch;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Branches API Controller
/// Controller'da SQL/DbContext KULLANILMAZ - Service layer kullanilir
/// </summary>
[ApiController]
[Route("api/branches")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class BranchesApiController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesApiController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    // GET /api/branches
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var branches = await _branchService.GetAllAsync(isActive);
        return Ok(branches);
    }

    // GET /api/branches/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);

        if (branch == null)
            return NotFound(new { message = "Branch.NotFound" });

        return Ok(branch);
    }

    // POST /api/branches
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Kod benzersizlik kontrolu
        if (await _branchService.CodeExistsAsync(dto.Code))
            return BadRequest(new { message = "Branch.Code.AlreadyExists" });

        var branch = await _branchService.CreateAsync(dto, User.Identity?.Name);

        return CreatedAtAction(nameof(GetById), new { id = branch.Id }, branch);
    }

    // PUT /api/branches/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Kod benzersizlik kontrolu (kendi id'si haric)
        if (await _branchService.CodeExistsAsync(dto.Code, id))
            return BadRequest(new { message = "Branch.Code.AlreadyExists" });

        var result = await _branchService.UpdateAsync(id, dto, User.Identity?.Name);

        if (!result)
            return NotFound(new { message = "Branch.NotFound" });

        return Ok(new { message = "Branch.Updated" });
    }

    // DELETE /api/branches/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _branchService.DeleteAsync(id, User.Identity?.Name);

        if (!result)
            return NotFound(new { message = "Branch.NotFound" });

        return Ok(new { message = "Branch.Deleted" });
    }
}
