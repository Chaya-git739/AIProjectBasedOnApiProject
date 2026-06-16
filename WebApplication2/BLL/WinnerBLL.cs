using WebApplication2.DAL;
using WebApplication2.Models;

namespace WebApplication2.BLL
{
    public class WinnerBLL
    {
        private IWinnerDAL _winnerDal;
        //BLL   all the crud actions on winner
        public WinnerBLL(IWinnerDAL winnerDal)
        {
            _winnerDal = winnerDal;
        }
        public async Task<List<WinnerModel>> GetAllWinners() // מחזיר את כל הזוכים
        {
            return _winnerDal.GetAllWinners();
        }
        public async Task<WinnerModel> GetWinnerById(int userId) // מחזיר זוכה לפי מזהה משתמש
        {
            return await _winnerDal.WinnerBYId(userId);

        }
        public async Task DeleteWinner(int winnerId) // מוסיף זוכה חדש
        {
           await _winnerDal.DeleteWinner(winnerId);
        }
        public async Task AddWinner(WinnerModel winner) // מוחק זוכה לפי מזהה משתמש
        {
            await _winnerDal.AddWinner(winner);
        }
    }
}
