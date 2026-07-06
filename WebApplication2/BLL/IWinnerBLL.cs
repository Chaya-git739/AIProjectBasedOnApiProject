using WebApplication2.Models;

namespace WebApplication2.BLL
{
    public interface IWinnerBLL
    {
   
        public Task DeleteWinner(int userId);
        //public   Task UpdateWinner(WinnerModel winner);
        public Task AddWinner(WinnerModel winner);
        public Task AddWinnerAndNotifyAsync(WinnerModel winner);
        public Task NotifyWinnerAsync(WinnerModel winner);
        public Task<WinnerModel> GetWinnerById(int userId);
        // פונקציה חדשה לשליפת כל הזוכים
        public Task<List<WinnerModel>> GetAllWinners();
    }
}
