using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
using WebApplication2.Models.DTO;
using AutoMapper.QueryableExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;

namespace WebApplication2.DAL
{
    public class GiftDAL : IGiftDal
    {
        private readonly StoreContext _context;
        private readonly IMapper _mapper;

        public GiftDAL(StoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<GiftDTO>> GetByFilter(string? name, string? donorName, int? minPurchasers)
        {
            var query = _context.Gifts
                .Where(g => !g.IsDeleted)
                .Include(g => g.Category)
                .Include(g => g.Donor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(g => EF.Functions.Like(g.Name, $"%{name}%"));

            if (!string.IsNullOrEmpty(donorName))
                query = query.Where(g => EF.Functions.Like(g.Donor.Name, $"%{donorName}%"));

            if (minPurchasers.HasValue)
            {
                var min = minPurchasers.Value;
                query = query.Where(g =>
                    _context.OrderTicket
                        .Where(t => t.GiftId == g.Id)
                        .Sum(t => (int?)t.Quantity) >= min);
            }

            var gifts = await query.AsNoTracking().ToListAsync();

            var giftDtos = new List<GiftDTO>();
            foreach (var gift in gifts)
            {
                var dto = _mapper.Map<GiftDTO>(gift);
                
                var ticketCount = await _context.OrderTicket
                    .Where(t => t.GiftId == gift.Id && t.Order.IsDraft == false)
                    .SumAsync(t => (int?)t.Quantity);
                dto.TicketsSold = ticketCount ?? 0;
                
                var hasWinner = await _context.Winners.AnyAsync(w => w.GiftId == gift.Id);
                dto.HasWinner = hasWinner;
                
                giftDtos.Add(dto);
            }

            return giftDtos;
        }

        public  Task<List<GiftDTO>> GetGiftsSortedByPrice() =>
            _context.Gifts
                .AsNoTracking()
                .OrderByDescending(g => g.TicketPrice)
                .ProjectTo<GiftDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public Task<List<GiftDTO>> GetMostPurchasedGifts() =>
            _context.Gifts
                .AsNoTracking()
                .OrderByDescending(g => _context.OrderTicket.Where(t => t.GiftId == g.Id).Sum(t => t.Quantity))
                .ProjectTo<GiftDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task Add(GiftDTO giftDto)
        {
            var gift = _mapper.Map<GiftModel>(giftDto);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == giftDto.Category.ToLower());
            if (category == null)
            {
                category = new CategoryModel { Name = giftDto.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            gift.CategoryId = category.Id;
            gift.Category = null!;

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Name == giftDto.DonorName);
            if (donor != null) gift.DonorId = donor.Id;

            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();
        }

        public async Task Update(GiftDTO giftDto)
        {
            var existingGift = await _context.Gifts.FindAsync(giftDto.Id);
            if (existingGift != null)
            {
                _mapper.Map(giftDto, existingGift);
                await _context.SaveChangesAsync();
            }
        }

        // Connected partial update for Gift:
        // Example: await UpdatePartialAsync(giftId, g => g.TicketPrice = 9.99m, g => g.TicketPrice);
        public async Task UpdatePartial(int id, Action<GiftModel> setValues, params Expression<Func<GiftModel, object>>[] modifiedProperties)
        {
            var entity = new GiftModel { Id = id };
            _context.Gifts.Attach(entity);
            setValues(entity);

            var entry = _context.Entry(entity);
            foreach (var prop in modifiedProperties)
            {
                var propName = GetPropertyName(prop);
                entry.Property(propName).IsModified = true;
            }

            await _context.SaveChangesAsync();
        }

        // Soft-delete for Gift
        public async Task Delete(int id)
        {
            var gift = await _context.Gifts.FindAsync(id);
            if (gift != null)
            {
                gift.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<GiftDTO>> GetAll()
        {
            var gifts = await _context.Gifts
                .AsNoTracking()
                .Include(g => g.Category)
                .Include(g => g.Donor)
                .ToListAsync();

            var giftDtos = new List<GiftDTO>();
            foreach (var gift in gifts)
            {
                var dto = _mapper.Map<GiftDTO>(gift);
                
                // ספירת כרטיסים שנמכרו (רק הזמנות מאושרות, לא טיוטות)
                var ticketCount = await _context.OrderTicket
                    .Where(t => t.GiftId == gift.Id && t.Order.IsDraft == false)
                    .SumAsync(t => (int?)t.Quantity);
                dto.TicketsSold = ticketCount ?? 0;
                
                // בדיקה אם יש זוכה
                var hasWinner = await _context.Winners.AnyAsync(w => w.GiftId == gift.Id);
                dto.HasWinner = hasWinner;
                
                giftDtos.Add(dto);
            }

            return giftDtos;
        }

        public async Task<decimal> GetTotalSalesAsync()
        {
            return await _context.Orders
                .Where(o => o.IsDraft == false)
                .SumAsync(o => (decimal)o.TotalAmount);
        }

        public async Task<List<GiftSalesDto>> GetSalesPerGiftAsync()
        {
            return await _context.OrderTicket
                .Where(ot => ot.Order.IsDraft == false)
                .GroupBy(ot => new { ot.GiftId, ot.Gift.Name, ot.Gift.TicketPrice })
                .Select(g => new GiftSalesDto
                {
                    GiftName = g.Key.Name,
                    PurchaseCount = g.Sum(ot => ot.Quantity),
                    RevenueFromGift = g.Sum(ot => ot.Quantity * g.Key.TicketPrice)
                })
                .ToListAsync();
        }

        public async Task<List<GiftWinnerDto>> GetGiftsWithWinnersAsync()
        {
            return await _context.Gifts
                .AsNoTracking()
                .GroupJoin(_context.Winners,
                    g => g.Id,
                    w => w.GiftId,
                    (g, winners) => new { Gift = g, Winners = winners })
                .SelectMany(x => x.Winners.DefaultIfEmpty(),
                    (x, winner) => new GiftWinnerDto
                    {
                        GiftId = x.Gift.Id,
                        GiftName = x.Gift.Name,
                        WinnerName = winner != null ? winner.User.Name : null
                    })
                .ToListAsync();
        }

        private static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            if (expression.Body is MemberExpression member)
                return member.Member.Name;

            if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberOperand)
                return memberOperand.Member.Name;

            throw new ArgumentException("Invalid expression");
        }
    }
}