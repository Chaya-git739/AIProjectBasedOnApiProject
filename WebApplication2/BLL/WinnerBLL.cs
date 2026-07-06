using WebApplication2.DAL;
using WebApplication2.Models;

namespace WebApplication2.BLL
{
    public class WinnerBLL
    {
        private readonly IWinnerDAL _winnerDal;
        private readonly IEmailService _emailService;
        //BLL   all the crud actions on winner
        public WinnerBLL(IWinnerDAL winnerDal, IEmailService emailService)
        {
            _winnerDal = winnerDal;
            _emailService = emailService;
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

        public async Task AddWinnerAndNotifyAsync(WinnerModel winner)
        {
            await _winnerDal.AddWinner(winner);

            await NotifyWinnerAsync(winner);
        }

        public async Task NotifyWinnerAsync(WinnerModel winner)
        {
            var notifiedWinner = await _winnerDal.WinnerBYId(winner.Id);
            if (notifiedWinner == null || notifiedWinner.User == null || notifiedWinner.Gift == null)
            {
                return;
            }

            try
            {
                await _emailService.SendWinnerNotificationAsync(
                    notifiedWinner.User.Email,
                    notifiedWinner.User.Name,
                    notifiedWinner.Gift.Name
                );
            }
            catch
            {
                // Notification failure must not break winner persistence.
            }
        }
    }
}
