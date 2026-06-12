using BanHang.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BanHang.Services.Interfaces
{
    public interface IAdminProductService
    {
        Task<List<SanPham>> GetAllAsync();
        Task<SanPham?> GetByIdAsync(int id);
        Task<bool> CreateAsync(SanPham model, ModelStateDictionary modelState);
        Task<bool> UpdateAsync(int id, SanPham model, ModelStateDictionary modelState);
        Task<bool> DeleteAsync(int id);
        Task LoadDropdownsAsync(dynamic viewBag, SanPham? model = null);
    }
}
