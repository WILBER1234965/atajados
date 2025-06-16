using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using atajados.Data;
using atajados.Models;
using Microsoft.EntityFrameworkCore;

namespace atajados.Services
{
    public class DatabaseService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        public DatabaseService(IDbContextFactory<AppDbContext> dbFactory)
            => _dbFactory = dbFactory;

        // --------------------------------------------------------------------
        //  Inicialización: eliminar y migrar BD
        // --------------------------------------------------------------------
        public async Task InitializeAsync()
        {
            await using var ctx = _dbFactory.CreateDbContext();

#if DEBUG
            // Borra la base de datos en cada arranque (solo Debug)
            await ctx.Database.EnsureDeletedAsync();
#endif
            // Crea o actualiza esquema
            await ctx.Database.MigrateAsync();
        }

        // --------------------------------------------------------------------
        //  Ítems
        // --------------------------------------------------------------------
        public async Task<List<Item>> GetItemsAsync()
        {
            await using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Items.AsNoTracking().ToListAsync();
        }

        public async Task AddItemAsync(Item item)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            ctx.Items.Add(item);
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(Item item)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            ctx.Items.Update(item);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            var item = await ctx.Items.FindAsync(id);
            if (item != null)
            {
                ctx.Items.Remove(item);
                await ctx.SaveChangesAsync();
            }
        }

        // --------------------------------------------------------------------
        //  Atajados
        // --------------------------------------------------------------------
        public async Task<List<Atajado>> GetAtajadosAsync()
        {
            await using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Atajados.AsNoTracking().ToListAsync();
        }

        public async Task AddAtajadoAsync(Atajado atajado)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            ctx.Atajados.Add(atajado);
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateAtajadoAsync(Atajado atajado)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            ctx.Atajados.Update(atajado);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteAtajadoAsync(int id)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            var a = await ctx.Atajados.FindAsync(id);
            if (a != null)
            {
                ctx.Atajados.Remove(a);
                await ctx.SaveChangesAsync();
            }
        }

        // --------------------------------------------------------------------
        //  Seguimientos
        // --------------------------------------------------------------------
        public async Task<List<Seguimiento>> GetSeguimientosByAtajadoAsync(int atajadoId)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Seguimientos
                .Include(s => s.Item)
                .AsNoTracking()
                .Where(s => s.AtajadoId == atajadoId)
                .ToListAsync();
        }

        public async Task AddSeguimientoAsync(Seguimiento seguimiento)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            ctx.Seguimientos.Add(seguimiento);
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateSeguimientoAsync(Seguimiento seguimiento)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            ctx.Seguimientos.Update(seguimiento);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteSeguimientoAsync(int id)
        {
            await using var ctx = _dbFactory.CreateDbContext();
            var s = await ctx.Seguimientos.FindAsync(id);
            if (s != null)
            {
                ctx.Seguimientos.Remove(s);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
