using System.Reflection;
using WebApplication2.Models;
using WebApplication2.Models.DTO;

namespace WebApplication2.DAL
{
    public interface IWinnerDAL
    {
     public  Task AddWinner(WinnerModel winner);
        public Task DeleteWinner(int userId);
        //public   Task UpdateWinner(WinnerModel winner);
        public Task<WinnerModel> WinnerBYId(int userId);
        // פונקציה חדשה לשליפת כל הזוכים
        public  List<WinnerModel> GetAllWinners();
        public Task<bool> IsGiftAlreadyWonAsync(int giftId);Task<WinnerModel?> GetWinnerByGiftIdAsync(int giftId); 

}
}
