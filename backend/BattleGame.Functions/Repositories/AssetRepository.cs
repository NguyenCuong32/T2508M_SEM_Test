using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleGame.Functions.Data;
using BattleGame.Functions.Models;
using BattleGame.Functions.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly BattleGameDbContext _context;

        public AssetRepository(BattleGameDbContext context)
        {
            _context = context;
        }

        public async Task<Asset?> GetByIdAsync(Guid assetId)
        {
            return await _context.Assets.FindAsync(assetId);
        }

        public async Task<Asset?> GetByNameAsync(string assetName)
        {
            return await _context.Assets
                .FirstOrDefaultAsync(a => a.AssetName.ToLower() == assetName.ToLower());
        }

        public async Task<bool> ExistsByNameAsync(string assetName)
        {
            return await _context.Assets
                .AnyAsync(a => a.AssetName.ToLower() == assetName.ToLower());
        }

        public async Task<IEnumerable<Asset>> GetAllAsync()
        {
            return await _context.Assets.AsNoTracking().ToListAsync();
        }

        public async Task AddAsync(Asset asset)
        {
            await _context.Assets.AddAsync(asset);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
