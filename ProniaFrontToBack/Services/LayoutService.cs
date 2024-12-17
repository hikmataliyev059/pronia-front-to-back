using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;

namespace ProniaFrontToBack.Services;

public class LayoutService
{
    private readonly AppDbContext _context;

    public LayoutService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, string>> GetSettings()
    {
        Dictionary<string, string> setting = await _context.Settings
            .ToDictionaryAsync(s => s.Key, s => s.Value);
        return setting;
    }
}